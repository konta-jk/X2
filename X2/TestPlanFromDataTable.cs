using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace X2
{
    class TestPlanFromDataTable
    {
        public static Structs.TestPlan GetTestPlan(DataTable testPlanTable)
        {
            Structs.Operation op;
            Structs.TestStep step;
            List<Structs.TestStep> testSteps = new List<Structs.TestStep>();

            foreach (DataRow row in testPlanTable.Rows)
            {
                op = new Structs.Operation(row.ItemArray[1].ToString(), row.ItemArray[2].ToString(), -1); //wait nie jest używany
                step = new Structs.TestStep(row.ItemArray[3].ToString(), op, row.ItemArray[0].ToString());
                testSteps.Add(step);
            }

            Structs.TestPlan testPlan = new Structs.TestPlan(testSteps); //docelowo z argumentami
            return testPlan;
        }
    }
}