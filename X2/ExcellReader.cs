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

namespace X2
{
    static class ExcellReader
    {
        public static DataTable ReadExcellSheet(string fileName)
        {
            DataTable dataSheet = new DataTable();
            //OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0;\"");
            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;HDR=NO\"");
            
            connection.Open();

            DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            string sheetName = schema.Rows[0].Field<string>("TABLE_NAME");

            string select = "select * from [" + sheetName + "A" + Globals.minRow.ToString() + ":D" + Globals.maxRow.ToString() + "]";
            Console.WriteLine(select);

            OleDbDataAdapter sheetAdapter = new OleDbDataAdapter(select, connection);
            sheetAdapter.Fill(dataSheet);
            return dataSheet;
        }        
    }
}
