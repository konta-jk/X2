﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;

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

                if (row[0].ToString() == "")
                {
                    MessageBox.Show("Wiersz " + (i + 1).ToString() + 
                        " scenariusza testu jest pusty. Test zostanie przeprowadzony z wczytanymi krokami testowymi: " 
                        + i.ToString() + ".", "Uwaga", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, 
                        MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    break;
                }

                //pominięcie komentarzy
                if ((row[0].ToString()[0] == '[') && (row[0].ToString()[row[0].ToString().Length - 1] == ']'))
                {
                    Console.WriteLine("TestPlanFromDataTable.GetTestPlan(): removed comment \"" + row[0].ToString() + "\"");
                    continue; 
                }

                step = new Structs.TestStep("(" + (i + 1).ToString() + ") " + row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), row.ItemArray[2].ToString(), row.ItemArray[3].ToString());                
                testSteps.Add(step);                
            }

            Structs.TestPlan testPlan = new Structs.TestPlan(testSteps);
            return testPlan;
        }

        public static bool IsValid(DataTable dataTable)
        {
            if ((dataTable != null) && (dataTable.Rows.Count > 0) && (dataTable.Columns.Count == 4))
            {

                //Console.WriteLine("DEBUG data table contents");
                foreach(DataRow row in dataTable.Rows)
                {
                    //Console.WriteLine(row[0].ToString() + "; " + row[1].ToString() + "; " + row[2].ToString() + "; " + row[3].ToString() + "; ");
                    //if(row[0] is null)

                }



                return true;

                //rozbudować o:
                ///sprawdzenie, czy przed uzyciem zmiennej jest set variable z taką samą nazwą
                ///xpath czy obecny, tam gdzie wymagany przez operację
                ////walidację xpath zostawić selenium, rzuci błędem na liście wyników
                ///jw. tekst
            }
            else
            {
                //MessageBox.Show("This file doesn't contain a valid test plan or file read failed.");
                return false;
            }

        }
    }
}