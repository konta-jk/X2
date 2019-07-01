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
            Structs.TestStep step;
            List<Structs.TestStep> testSteps = new List<Structs.TestStep>();

            for (int i = 0; i < testPlanTable.Rows.Count; i++)
            {
                DataRow row = testPlanTable.Rows[i];
                step = new Structs.TestStep((i + 1).ToString() + ") " + row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), row.ItemArray[2].ToString(), row.ItemArray[3].ToString());                
                testSteps.Add(step);                
            }

            Structs.TestPlan testPlan = new Structs.TestPlan(testSteps);
            return testPlan;
        }
    }
}