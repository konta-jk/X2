/*
 * Implementuje IQATestLaunchPoint, aby mógł przeprowadzać testy za pomocą QA Test Launchera
 * Ładuje dane testów do pamięci, bo i tak nie zdąży ich zapchać, bo testy robią się powoli i definicje testów na jedną noc powinny się zmieścić w 10 MB
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
    class TestManager : IQATestLaunchPoint
    {
        private List<int> batches = new List<int>();
        private int currentBatch;
        private List<int> testPlans = new List<int>(); //dla bieżącego batcha
        private int currentTestPlan;
        private QATestStuff currentTestStuff;
        private DateTime currentTestStart;
        private string currentLogPath = "";
        private bool testRunning = false;

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

        public void OnTestFinish()
        {
            Console.WriteLine("# FINISH TestManager.OnTestFinish()...");

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
                @"VALUES (" + currentTestPlan + @", '" + successStr + @"', " + ((int)((DateTime.Now - currentTestStart).TotalSeconds)).ToString() + @", '" + currentTestStart.ToString() + @"', '" + currentLogPath + @"', '-1')";

            new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable); //docelowo showExcMsg = false 


            

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

            output += "\r\nLog:\r\n" + currentTestStuff.log;            

            currentLogPath = currentTestStuff.GetPathMakeFolder(@"\Logs\");
            currentLogPath = currentLogPath + @"\" + currentTestStuff.testRunId + ".txt";
            System.IO.File.WriteAllText(currentLogPath, output);
        }


        //lista id batch testów do zrobienia
        //dla każdego
        //zrobić listę id testów do zrobienia
        //dla każdego
        //normalnie test jak dotąd

        private int FindFirstTest()
        {
            //znajdź pierwszy test plan z aktywnego batcha, dla którego nie wykonano dzisiaj tylu testów ilu wymaga
            //SIANO!!! Testować to zapytanie jeszcze długo! Pierwszy podejrzany w razie błędów
            //do refaktoryzacji żeby używać prostszego sql-a
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
                select top 1 q3.IdTestPlan from q3
                join q2 on q3.IdTestPlan = q2.IdTestPlan
                order by idtestbatch, orderinbatch";

            new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, true, out DataTable dataTable); //msg flase

            if ((dataTable.Rows.Count > 0) && (dataTable.Rows[0][0].ToString() != "NO_RESULT_TABLE"))
            {
                return (int)dataTable.Rows[0][0];
            }
            else
                return -1;
        }

        public void RunFirst() //robi pierwszy z brzegu test jeszcze nie wykonany
        {   
            if ((FindFirstTest() != -1) && !testRunning)
            {
                currentTestPlan = FindFirstTest();
                StartTest();
            }
        }






        private void GetBatches() 
        {
            batches.Clear();
            string query = "select IdTestBatch from dps.dpsdynamic.QA_TEST_BATCH where IsDaily = 'YES' and IsActive = 'YES'";
            string result = new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable);
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
            string result = new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, true, out DataTable dataTable); //msg false docelowo
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
            string query = "select Description, Command, Text, XPath from dps.dpsdynamic.QA_TEST_STEP where IdTestPlan = '" + testPlan.ToString() + "'";
            string result = new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, false, out DataTable dataTable);

            return dataTable;
        }



    }
}
