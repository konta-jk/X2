/*
 * czyta arkusz excella
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace X2
{
    static class ReaderXlsx
    {
        public static DataTable ReadExcellSheet(QATestStuff testStuff, string fileName)
        {
            try
            {
                DataTable dataSheet = new DataTable();
                //OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0;HDR=NO\"");
                OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;HDR=NO\"");

                connection.Open();

                DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string sheetName = schema.Rows[0].Field<string>("TABLE_NAME");

                string select = "select * from [" + sheetName + "A" + testStuff.minRow.ToString() + ":D" + testStuff.maxRow.ToString() + "]";

                



                OleDbDataAdapter sheetAdapter = new OleDbDataAdapter(select, connection);
                sheetAdapter.Fill(dataSheet);

                connection.Close();

                //usunięcie komentarzy
                //przeniesione do test plan from data table
                /*
                dataSheet.AcceptChanges();
                foreach (DataRow row in dataSheet.Rows)
                {
                    string s = row[0].ToString();
                    if (s[0] == '[' && s[s.Length - 1] == ']')
                    {
                        row.Delete();
                    }
                }
                dataSheet.AcceptChanges();
                */

                return dataSheet;
            }
            catch (Exception e)
            {
                MessageBox.Show("Can't read this file. Try using *.csv file. Exception:\r\n" + e.ToString());
                return null;
            }            
        }        
    }
}
