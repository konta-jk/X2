/*
 * uniwersalna klasa do odpalania testu
 * trzyma śmieci, których nie chcę w QATest; wątek itp
 * bierze form, a w nim setupa, a w nim filename
 * ładuje wyniki do setupa
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection;

namespace X2
{
    class QATestLauncher
    {
        IQATestLaunchPoint launchPoint;
        QATestSetup testSetup;
        public QATest qATest;
        Structs.TestPlan testPlan;

        public QATestLauncher(IQATestLaunchPoint launchPoint1)
        {
            launchPoint = launchPoint1; //np. Form1
            testSetup = launchPoint.GetTestSetup();

            string extension = Regex.Match(testSetup.fileName, "\\.[0-9a-z]+$").Value;
            //Structs.TestPlan testPlan;
            DataTable dataTable;

            switch (extension)
            {
                case ".xlsx":
                    dataTable = XlsxReader.ReadExcellSheet(testSetup);                    
                    break;

                case ".csv":
                    dataTable = CsvReader.ReadCsv(testSetup);
                    break;

                default:
                    dataTable = CsvReader.ReadCsv(testSetup);
                    break;
            }

            if (TestPlanFromDataTable.IsValid(dataTable))
            {
                testPlan = TestPlanFromDataTable.GetTestPlan(dataTable);
                qATest = new QATest(testPlan, testSetup);
                qATest.RunFinishedEvent += OnRunFinished;
                qATest.StepFinishedEvent += OnStepFinished;
            }            
        }

        public void Run() 
        {
            if (testPlan.testSteps != null)
            {
                testSetup.seleniumThread = new Thread(ActualRun);
                testSetup.seleniumThread.IsBackground = true;
                testSetup.seleniumThread.Start();
            }            
        }
        
        //private delegate void UpdateResultDelegate();
        //private delegate void UpdateProgressDelegate();
        private delegate void BringToFrontDelegate();

        void OnRunFinished(object sender, EventArgs e)
        {
            //Console.WriteLine("QATestLauncher: RunFinishedEvent caught");                        
            //launchPoint.DoInvoke(new UpdateResultDelegate(launchPoint.UpdateResult)); //działające
            launchPoint.OnTestFinish();
        }

        void OnStepFinished(object sender, EventArgs e)
        {
            //Console.WriteLine("QATestLauncher: StepFinishedEvent caught");
            //launchPoint.DoInvoke(new UpdateProgressDelegate(launchPoint.UpdateProgress));
            launchPoint.OnTestProgress();
        }

        void ActualRun()
        {
            testSetup.Init();            
            qATest.Run(); 

            if (testSetup.killDriver)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Settings.killDriverDelay)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                testSetup.TearDownTest();
            }
        }       
    }
}

