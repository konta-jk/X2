/*
 * Implementuje IQATestLaunchPoint, aby mógł przeprowadzać testy za pomocą QA Test Launchera
 * Wybiera pierwszy niezrealizowany test z zadanych do zrobienia i go przeprowadza, o ile nie jest zajęty; wywoływany przez timer
 * 
 */
 
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace X2
{
    public class TestManager : IQATestLaunchPoint
    {
        private List<int> batches = new List<int>();
        private int currentBatch;
        private int lastBatch = -1;
        private List<int> testPlans = new List<int>(); //dla bieżącego batcha
        private int currentTestPlan;
        private QATestStuff currentTestStuff;
        private DateTime currentTestStart;
        private string currentLogPath = "";
        private bool testRunning = false;
        private Logger techLogger;
        public List<Structs.Variable> batchVariables = new List<Structs.Variable>();

        public TestManager()
        {
            techLogger = new Logger(@"\Daemon\", "Log.txt");
        }

        public IQATestLaunchPoint GetLaunchPoint()
        {
            return this;
        }

        public DataTable GetTestPlanAsDataTable() //musi być przy użyciu globalnej zmiennej z id 
        {
            return GetTestSteps(currentTestPlan);
        }

        public QATestStuff GetTestStuff()
        {
            return currentTestStuff;
        }

        public void OnTestProgress()
        {
            //Console.WriteLine("TestManager.OnTestProgress()...");
        }

        public void OnTestCancel(string reason)
        {
            //Console.WriteLine("TestManager.OnTestCancel()... reason: " + reason);

            string query = @"INSERT INTO dps.dpsdynamic.QA_TEST_RESULT (IdPlan, IdBatch, TestResult, DurationSeconds, DateTime, LogPath, ScreenshotsPath) " +
                @"VALUES (" + currentTestPlan + @", " + currentBatch +  @", 'FAIL', " + ((int)((DateTime.Now - currentTestStart).TotalSeconds)).ToString() + @", '" + currentTestStart.ToString() + @"', '" + currentTestStuff.logger.GetFolderPath() + @"', '-1')";

            new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable); //docelowo showExcMsg = false 

            if (currentTestStuff.killDriver)
            {
                currentTestStuff.TearDownTest();
            }

            testRunning = false;
        }

        public void OnTestFinish()
        {
            //Console.WriteLine("TestManager.OnTestFinish()...");

            UpdateResult(); //na pałę wywołanie bez delegata //czyli nic nie rozumiem z delegatów, ale to działa; wcześniejsze uzycie delegatów było tylko objeściem ograniczeń control-ki

            if (currentTestStuff.killDriver)
            {
                currentTestStuff.TearDownTest();
            }

            testRunning = false;
        }

        private void UpdateResult() 
        {
            int stepCount = currentTestStuff.testPlan.testSteps.Count();
            int stepResultsOk = currentTestStuff.testResult.testStepResults.Where(t => t.result == "ok").Count();
            
            string successStr = "";
            if (stepCount == stepResultsOk)
            {
                successStr = "PASS";
            }
            else
            {
                successStr = "FAIL";
            }

            SaveLog();


            
            string query = @"INSERT INTO dps.dpsdynamic.QA_TEST_RESULT (IdPlan, IdBatch, TestResult, DurationSeconds, DateTime, LogPath, ScreenshotsPath) " +
                @"VALUES (" + currentTestPlan + @", " + currentBatch + @", '" + successStr + @"', " + ((int)((DateTime.Now - currentTestStart).TotalSeconds)).ToString() + @", '" + currentTestStart.ToString() + @"', '" + currentTestStuff.logger.GetFolderPath() + @"', '-1')";

            new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable); 
        }

        private void StartTest()
        {
            testRunning = true;

            currentTestStart = DateTime.Now;

            QATestStuff.QATestStuffOptions stuffOptions = new QATestStuff.QATestStuffOptions();
            stuffOptions.killDriver = true;
            stuffOptions.minRow = 2;
            stuffOptions.maxRow = 1000000;
            currentTestStuff = new QATestStuff(stuffOptions);
            currentTestStuff.testManager = this; //tylko klasa test manager to robi, form1 zostawia null
            currentTestStuff.CreateDriver(); //a może testStuff.driver = ..?
            currentTestStuff.Init();
            new QATestLauncher(this).Run();
        }

        
        private void SaveLog()
        {
            string output = currentTestStuff.testResult.ToCsvString();

            string s = "";
            foreach (Structs.Variable v in currentTestStuff.variables)
            {
                s += v.name + " = " + v.value + "\r\n";
            }
            if (s.Length > 0)
            {
                output += "\r\nVariables:\r\n" + s;
            }

            output += "\r\nLog:\r\n" + currentTestStuff.logger.GetLogString();

            currentLogPath = currentTestStuff.logger.GetFolderPath(); 
            currentLogPath = currentLogPath + @"\Output.txt";
            System.IO.File.WriteAllText(currentLogPath, output);
        }
        

        private int FindFirstTest(out int batch)
        {
            //znajdź pierwszy test plan z aktywnego batcha, dla którego nie wykonano dzisiaj testu
            //chyba wyjąć do settingsów całe query
            string query =
                @"select A1.IdPlan, A2.IdBatch, A2.OrderInBatch from dps.dpsdynamic.qa_test_plan A1
                join dps.dpsdynamic.qa_test_plan_in_batch A2
                on A1.IdPlan = A2.IdPlan
                join dps.dpsdynamic.QA_TEST_BATCH A3
                on A3.IdBatch = A2.IdBatch
                where (select count (*) from dps.dpsdynamic.qa_test_step_in_plan A5 where A5.idplan = A1.IdPlan) > 0

                except

                select A1.IdPlan, A2.IdBatch, A2.OrderInBatch from dps.dpsdynamic.qa_test_plan A1
                join dps.dpsdynamic.qa_test_plan_in_batch A2
                on A1.IdPlan = A2.IdPlan
                join dps.dpsdynamic.QA_TEST_BATCH A3
                on A3.IdBatch = A2.IdBatch
                right join dps.dpsdynamic.QA_TEST_RESULT A4
                on A4.IdBatch = A2.IdBatch and A4.IdPlan = A2.IdPlan
                where CAST(A4.DateTime as date) = CAST(GETDATE() AS date)
                order by A2.IdBatch, A2.OrderInBatch";
                

            string result = new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable); //msg flase

            if (result != "ok")
            {
                techLogger.Log("TestManager.FindFirstTest() DB connection error: " + result);
            }


            if ((dataTable.Rows.Count > 0) && (dataTable.Rows[0][0].ToString() != "NO_RESULT_TABLE"))
            {
                batch = (int)dataTable.Rows[0][1];
                return (int)dataTable.Rows[0][0];
            }
            else
            {
                batch = -1;
                return -1;
            }
                
        }

        public void RunFirst() //robi pierwszy z brzegu test jeszcze nie wykonany
        {   
            if ((FindFirstTest(out int temp) != -1) && !testRunning)
            {
                currentTestPlan = FindFirstTest(out currentBatch);

                if (currentBatch != lastBatch)
                {
                    InitBatch();
                }

                techLogger.Log("TestManager.RunFirst() called, will start test for plan " + currentTestPlan.ToString() + ", batch " + currentBatch.ToString() + ".");
                StartTest();
            }
        }

        /*
        private void GetBatches() 
        {
            batches.Clear();
            string query = "select IdTestBatch from dps.dpsdynamic.QA_TEST_BATCH where IsActive = 'YES'";
            string result = new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable);
            if (result == "ok")
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    batches.Add((int)row[0]);
                }
            }
            batches.Sort();
        }

        private void GetTestPlans(int batch) 
        {
            testPlans.Clear();
            string query = "select IdTestPlan from dps.dpsdynamic.QA_TEST_PLAN where IdTestBatch = " + batch.ToString();
            string result = new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, true, out DataTable dataTable); //msg false docelowo
            if (result == "ok")
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    testPlans.Add((int)row[0]);
                }
            }
            testPlans.Sort();
        }
        */



        private DataTable GetTestSteps(int testPlan) 
        {
            string query = "select Description, Command, Text, XPath from dps.dpsdynamic.QA_TEST_STEP A1 " +
                "join dps.dpsdynamic.QA_TEST_STEP_IN_PLAN A2 on A1.IdStep = A2.IdStep where A2.IdPlan = '" + testPlan.ToString() + "' order by A2.OrderInTest";
            string result = new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable);

            return dataTable;
        }
        

        private void InitBatch()
        {
            batchVariables.Clear();
        }


    }
}
