/*
 * Implementuje IQATestLaunchPoint, aby mógł przeprowadzać testy za pomocą QA Test Launchera
 * Ładuje dane testów do pamięci, bo i tak nie zdąży ich zapchać, bo testy robią się powoli i definicje testów na jedną noc powinny się zmieścić w 10 MB
 * 
 */


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    class TestManager //: IQATestLaunchPoint
    {
        private List<int> batches = new List<int>();
        private int currentBatch;
        private List<int> testPlans = new List<int>(); //dla bieżącego batcha


        //lista id batch testów do zrobienia
        //dla każdego
        //zrobić listę id testów do zrobienia
        //dla każdego
        //normalnie test jak dotąd

        

        private void GetBatches() 
        {
            string query = "select IdTestBatch from dps.dpsdynamic.QA_TEST_BATCH where IsDaily = 'YES' and IsActive = 'YES'";
            string result = new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, true, out DataTable dataTable);
            if (result == "ok")
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    batches.Add((int)row[0]);
                }
            }
            batches.Sort();
        }

        private void GetTestPlans(int batch) 
        {
            string query = "select IdTestPlan from dps.dpsdynamic.QA_TEST_PLAN where IdTestBatch = '" + batch.ToString() + "'";
            string result = new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, true, out DataTable dataTable);
            if (result == "ok")
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    testPlans.Add((int)row[0]);
                }
            }
        }

        private DataTable GetTestSteps(int testPlan) 
        {
            string query = "select Description, Command, Text, XPath from dps.dpsdynamic.QA_TEST_STEP where IdTestPlan = '" + testPlan.ToString() + "'";
            string result = new DataBaseReaderWriter().TryQueryToDataTable(Settings.connectionString, query, true, out DataTable dataTable);

            return dataTable;
        }

    }
}
