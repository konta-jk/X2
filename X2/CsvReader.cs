using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;


namespace X2
{
    class CsvReader
    {
        public static DataTable ReadCsv(string fileName)
        {
            DataTable dataTable = new DataTable();
            StreamReader streamReader = new StreamReader(fileName);
            int lineReads = 0;
            string[] columns = streamReader.ReadLine().Split(',');
            lineReads++;
            foreach (string s in columns)
            {
                dataTable.Columns.Add(s);
            }
            while (!streamReader.EndOfStream)
            {
                string[] cells 
                    = Regex.Split(streamReader.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                lineReads++;
                DataRow row = dataTable.NewRow();
                for(int i = 0; i < cells.Length; i++)
                {
                    row[i] = cells[i];
                }
                if((lineReads >= Globals.minRow) && (lineReads <= Globals.maxRow))
                {
                    dataTable.Rows.Add(row);
                }
            }
            streamReader.Close();
            return dataTable;
        }
    }
}
