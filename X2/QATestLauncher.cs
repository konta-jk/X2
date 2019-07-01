﻿/*
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

namespace X2
{
    class QATestLauncher
    {
        Form1 form;
        public QATest qATest;
        Structs.TestPlan testPlan;

        public QATestLauncher(Form1 form1)
        {
            form = form1;

            string extension = Regex.Match(form.testSetup.fileName, "\\.[0-9a-z]+$").Value;
            Structs.TestPlan testPlan;
            switch (extension)
            {
                case ".xlsx":
                    testPlan = TestPlanFromDataTable.GetTestPlan(XlsxReader.ReadExcellSheet(form.testSetup));
                    break;

                case ".csv":
                    testPlan = TestPlanFromDataTable.GetTestPlan(CsvReader.ReadCsv(form.testSetup));
                    break;

                default:
                    testPlan = TestPlanFromDataTable.GetTestPlan(CsvReader.ReadCsv(form.testSetup));
                    break;
            }

            qATest = new QATest(testPlan, form1.testSetup);
            qATest.RunFinishedEvent += OnRunFinished;
            qATest.StepFinishedEvent += OnStepFinished;
        }

        public void Run() 
        {
            form.testSetup.seleniumThread = new Thread(ActualRun);
            form.testSetup.seleniumThread.IsBackground = true;
            form.testSetup.seleniumThread.Start();
        }
        
        private delegate void UpdateResultDelegate();
        private delegate void UpdateProgressDelegate();

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
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                form.testSetup.TearDownTest();
            }
        }       
    }
}

