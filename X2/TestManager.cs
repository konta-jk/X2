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
            Console.WriteLine("TestManager.OnTestProgress()...");
        }

        public void OnTestCancel(string reason)
        {
            Console.WriteLine("TestManager.OnTestCancel()... reason: " + reason);

            string query = @"INSERT INTO dps.dpsdynamic.QA_TEST_RESULT (IdTestPlan, TestResult, DurationSeconds, DateTime, LogPath, ScreenshotsPath) " +
                @"VALUES (" + currentTestPlan + @", 'FAIL', " + ((int)((DateTime.Now - currentTestStart).TotalSeconds)).ToString() + @", '" + currentTestStart.ToString() + @"', '" + currentTestStuff.logger.GetFolderPath() + @"', '-1')";

            new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable); //docelowo showExcMsg = false 

            if (currentTestStuff.killDriver)
            {
                currentTestStuff.TearDownTest();
            }

            testRunning = false;
        }

        public void OnTestFinish()
        {
            Console.WriteLine("TestManager.OnTestFinish()...");

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

            string query = @"INSERT INTO dps.dpsdynamic.QA_TEST_RESULT (IdTestPlan, TestResult, DurationSeconds, DateTime, LogPath, ScreenshotsPath) " +
                @"VALUES (" + currentTestPlan + @", '" + successStr + @"', " + ((int)((DateTime.Now - currentTestStart).TotalSeconds)).ToString() + @", '" + currentTestStart.ToString() + @"', '" + currentTestStuff.logger.GetFolderPath() + @"', '-1')";

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
            //testować to jeszcze długo, pierwszy podejrzany w razie błędów
            //do refaktoryzacji żeby używać prostszego sql-a (kompatybilne z inną bazą)
            string query =
                @"with q1 
                as
                (
                select IdTestPlan, CAST(DateTime as Date) Date1, count(*) Count1 from dps.dpsdynamic.QA_TEST_RESULT
                group by IdTestPlan, CAST(DateTime as Date)
                ),

                q2 
                as
                (
                select P.IdTestPlan, P.IdTestBatch, P.Name, P.[Version], P.OrderInBatch, q1.Date1, q1.Count1, B.IsDaily, B.IsActive from dps.dpsdynamic.QA_TEST_PLAN P
                join  dps.dpsdynamic.QA_TEST_BATCH B on P.IdTestBatch = B.IdTestBatch
                left join q1 on P.IdTestPlan = q1.IdTestPlan
                ),

                q3
                as
                (
                select IdTestPlan from q2
                where IsActive = 'YES' and IsDaily = 'YES'

                except 

                select IdTestPlan from q2
                where Date1 = cast(getdate() as date)
                )

                --select * from q3
                select top 1 q3.IdTestPlan, q2.IdTestBatch from q3
                join q2 on q3.IdTestPlan = q2.IdTestPlan
                order by idtestbatch, orderinbatch";

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

        private void GetBatches() 
        {
            batches.Clear();
            string query = "select IdTestBatch from dps.dpsdynamic.QA_TEST_BATCH where IsDaily = 'YES' and IsActive = 'YES'";
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

        private DataTable GetTestSteps(int testPlan) 
        {
            string query = "select Description, Command, Text, XPath, OrderInTest from dps.dpsdynamic.QA_TEST_STEP where IdTestPlan = '" + testPlan.ToString() + "' order by OrderInTest"; //ORDER MORONIE!
            string result = new ReaderWriterDataBase().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable);

            return dataTable;
        }

        private void InitBatch()
        {
            batchVariables.Clear();
        }


    }
}
