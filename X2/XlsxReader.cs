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
    static class XlsxReader
    {
        public static DataTable ReadExcellSheet(QATestSetup testSetup)
        {
            try
            {
                DataTable dataSheet = new DataTable();
                //OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0;HDR=NO\"");
                OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + testSetup.fileName + ";Extended Properties=\"Excel 12.0;HDR=NO\"");

                connection.Open();

                DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string sheetName = schema.Rows[0].Field<string>("TABLE_NAME");

                string select = "select * from [" + sheetName + "A" + testSetup.minRow.ToString() + ":D" + testSetup.maxRow.ToString() + "]";

                OleDbDataAdapter sheetAdapter = new OleDbDataAdapter(select, connection);
                sheetAdapter.Fill(dataSheet);

                connection.Close();

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
