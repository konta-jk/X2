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
        object result = null;

        public string Run() 
        {
            Thread thread = new Thread(ActualRun);
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            return result.ToString();
        }

        void ActualRun()
        {
            Globals.Init();

            Structs.TestPlan testPlan = TestPlanFromDataTable.GetTestPlan(ExcellReader.ReadExcellSheet(Globals.fileName));

            Test test = new Test(testPlan);
            test.Run();            

            if(Globals.killDriver)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki
                Globals.TearDownTest();
            }            

            result = test.GetResult();
        }       

    }

}

