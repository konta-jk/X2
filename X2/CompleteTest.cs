using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    static class CompleteTest
    {
        public static string Run()
        {
            Instances.Init();

            Structs.TestPlan testPlan = TestPlanFromDataTable.GetTestPlan(ExcellReader.ReadExcellSheet(Instances.fileName));

            Test test = new Test(testPlan);
            test.Run();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki
            Instances.TearDownTest();

            return test.GetResult();
        }
    }
}

