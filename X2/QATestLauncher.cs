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
        //object result = null;
        Form1 form;
        public QATest qATest;        

        public QATestLauncher(Form1 form1)
        {
            form = form1;

            //* zakomentowane ze względu na niemożliwość obsługi excella
            //Structs.TestPlan testPlan = TestPlanFromDataTable.GetTestPlan(ExcellReader.ReadExcellSheet(Globals.fileName));
            Structs.TestPlan testPlan = TestPlanFromDataTable.GetTestPlan(CsvReader.ReadCsv(Globals.fileName));

            qATest = new QATest(testPlan);
            //* subskrypcja - zbędne; tych informacji potrzebuje form
            qATest.RunFinishedEvent += OnRunFinished;
            qATest.StepFinishedEvent += OnStepFinished;
        }

        public void Run(Form1 form1) 
        {
            form = form1;
            Globals.seleniumThread = new Thread(ActualRun);
            Globals.seleniumThread.IsBackground = true;
            Globals.seleniumThread.Start();
        }




        private delegate void UpdateResultDelegate();
        private delegate void UpdateProgressDelegate();

        void OnRunFinished(object sender, EventArgs e)
        {
            Console.WriteLine("QATestLauncher: RunFinishedEvent caught");
            form.Invoke(new UpdateResultDelegate(form.UpdateResult));
        }

        void OnStepFinished(object sender, EventArgs e)
        {
            Console.WriteLine("QATestLauncher: StepFinishedEvent caught");
            form.Invoke(new UpdateProgressDelegate(form.UpdateResult));
        }
        

        void ActualRun()
        {
            Globals.Init();

            

            
            qATest.Run();       

            if(Globals.killDriver)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                Globals.TearDownTest();
            }

            //result = test.GetResult();
            Globals.testResult = qATest.GetResult();
        }       

    }

}

