using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace X2
{
    class CompleteTest
    {
        //object result = null;
        Form1 form;

        public void Run(Form1 form1) 
        {
            form = form1;
            Globals.seleniumThread = new Thread(ActualRun);
            Globals.seleniumThread.IsBackground = true;
            Globals.seleniumThread.Start();
            //thread.Join();            
        }

        void ActualRun()
        {
            Globals.Init();

            Structs.TestPlan testPlan = TestPlanFromDataTable.GetTestPlan(ExcellReader.ReadExcellSheet(Globals.fileName));

            Test test = new Test(testPlan);
            test.Run();            

            if(Globals.killDriver)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                Globals.TearDownTest();
            }

            //result = test.GetResult();
            Globals.testResult = test.GetResult();
        }       

    }

}

