/*
 * uniwersalna klasa do odpalania testu
 * trzyma śmieci, których nie chcę w QATest; wątek itp
 * bierze DataTable
 * zwraca obiekt, którego głównym składnikiem jest DataTable
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;


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

            //* zakomentowane ze względu na niemożliwość obsługi excella
            testPlan = TestPlanFromDataTable.GetTestPlan(XlsxReader.ReadExcellSheet(form.testSetup));
            //* dla csv
            //Structs.TestPlan testPlan = TestPlanFromDataTable.GetTestPlan(CsvReader.ReadCsv(form.testSetup));

            qATest = new QATest(testPlan);
            //* subskrypcja - zbędne; tych informacji potrzebuje form
            qATest.RunFinishedEvent += OnRunFinished;
            qATest.StepFinishedEvent += OnStepFinished;
        }

        public void Run(Form1 form1) 
        {
            form = form1;
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
            form.Invoke(new UpdateProgressDelegate(form.UpdateResult));
        }

        void ActualRun()
        {
            form.testSetup.Init();
   
            qATest.Run(testPlan, form.testSetup); 

            if (form.testSetup.killDriver)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                form.testSetup.TearDownTest();
            }
        }       
    }
}

