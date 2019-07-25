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
    class ReaderCsv
    {
        public static DataTable ReadCsv(QATestStuff testStuff, string fileName)
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
            if (dataTable.Columns.Count != 4) 
            {
                return null;
            }
            while (!streamReader.EndOfStream)
            {
                string[] cells 
                    = Regex.Split(streamReader.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                if (cells.Count() > 4)
                {
                    //return null; //źródło błędów, xpath z plugina może zawierać przecinki
                                        
                    for (int i = 4; i < cells.Count(); i++)
                    {
                        cells[3] += cells[i];
                    }

                    cells = cells.Where((value, index) => index < 4).ToArray();
                }
                lineReads++;
                DataRow row = dataTable.NewRow();
                for(int i = 0; i < cells.Length; i++)
                {
                    row[i] = cells[i];
                }
                if((lineReads >= testStuff.minRow) && (lineReads <= testStuff.maxRow))
                {
                    dataTable.Rows.Add(row);
                }
            }
            streamReader.Close();
            return dataTable;
        }
    }
}
