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

namespace X2
{
    class QATestLauncher
    {
        Form1 form;
        public QATest qATest;
        Structs.TestPlan testPlan;

        public QATestLauncher(Form1 form1)
        {
            var form = form1;

            string extension = Regex.Match(form.testSetup.fileName, "\\.[0-9a-z]+$").Value;
            //Structs.TestPlan testPlan;
            DataTable dataTable;

            switch (extension)
            {
                case ".xlsx":
                    dataTable = XlsxReader.ReadExcellSheet(form.testSetup);                    
                    break;

                case ".csv":
                    dataTable = CsvReader.ReadCsv(form.testSetup);
                    break;

                default:
                    dataTable = CsvReader.ReadCsv(form.testSetup);
                    break;
            }

            if (TestPlanFromDataTable.IsValid(dataTable))
            {
                testPlan = TestPlanFromDataTable.GetTestPlan(dataTable);
                qATest = new QATest(testPlan, form1.testSetup);
                qATest.RunFinishedEvent += OnRunFinished;
                qATest.StepFinishedEvent += OnStepFinished;
            }            
        }

        public void Run() 
        {
            if (testPlan.testSteps != null)
            {
                form.testSetup.seleniumThread = new Thread(ActualRun);
                form.testSetup.seleniumThread.IsBackground = true;
                form.testSetup.seleniumThread.Start();
            }            
        }
        
        private delegate void UpdateResultDelegate();
        private delegate void UpdateProgressDelegate();
        private delegate void BringToFrontDelegate();

        void OnRunFinished(object sender, EventArgs e)
        {
            //Console.WriteLine("QATestLauncher: RunFinishedEvent caught");                        
            form.Invoke(new UpdateResultDelegate(form.UpdateResult));
        }

        void OnStepFinished(object sender, EventArgs e)
        {
            //Console.WriteLine("QATestLauncher: StepFinishedEvent caught");
            form.Invoke(new UpdateProgressDelegate(form.UpdateProgress));
        }

        void ActualRun()
        {
            form.testSetup.Init();            
            qATest.Run(); 

            if (form.testSetup.killDriver)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Settings.killDriverDelay)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                form.testSetup.TearDownTest();
            }
        }       
    }
}

