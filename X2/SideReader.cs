using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Data;

namespace X2
{
    //przenieść z X2/QA Cat do osobnego narzędzia QA Hog; nie ma szans żeby konwersja wystarczyła do stworzenia sensownego scenariusza (oczekiwanie, zapamiętywanie zmiennych itp)
    //dokończyć - ładne wybieranie xpatha, może da się coś pozyskać do kolumny text w test step....
    //może jeszcze dodać funkcję pośredniczącą między tworzeniem a zapisem data table, która przetłumaczy z seleniowego na moje
    //docelowo to osobne narzędzie/projekt/moduł będzie też musiał mieć kontrolki do wyboru pliku wejściowego i wyjściowego

    //klasa odzwierciedlająca jsona
    class SideReader
    {
        [DataContract]
        public class SideContent
        {
            [DataMember] public string name;
            [DataMember] public string url;
            [DataMember] public List<Test> tests;

            [DataContract]
            public class Test
            {
                [DataMember] public string name;
                [DataMember] public List<Command> commands;

                [DataContract]
                public class Command
                {
                    [DataMember] public string command;
                    [DataMember] public string[][] targets;
                    [DataMember] public string value;
                }
            }
        }

        public SideContent GetSideContent(string json)
        {
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));

            using (ms) //skrót od try, finally dispose
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(SideContent));
                return (SideContent)deserializer.ReadObject(ms);
            }   
        }


        string ReadSideFile(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                return streamReader.ReadToEnd();
            }
        }

        void DataTableToCsvFile(string fileName, DataTable dataTable)
        {
            StringBuilder sb = new StringBuilder();
            List<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field =>
                  string.Concat("", field.ToString().Replace("\"", "\"\""), ""));
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        //work in progress, dokończyć
        public DataTable SideContentToDataTable(SideContent content)
        {
            DataTable dataTable = new DataTable();
            for (int i = 0; i < 4; i++)
            {
                dataTable.Columns.Add("Column " + i.ToString());
            }
            DataRow row = dataTable.NewRow();

            row[0] = "Load website";
            row[1] = "GoToUrl";
            row[2] = content.url;
            row[3] = "";

            dataTable.Rows.Add(row);

            row = dataTable.NewRow();

            row[0] = "Reload website";
            row[1] = "Refresh";
            row[2] = "";
            row[3] = "";

            dataTable.Rows.Add(row);

            foreach (SideContent.Test t in content.tests)
            {
                foreach (SideContent.Test.Command c in t.commands)
                {
                    row = dataTable.NewRow();

                    row[0] = "..."; //todo
                    row[1] = c.command;
                    row[2] = ""; //todo
                    row[3] = ""; //todo

                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }





        //do wywalenia po testach ----------------------------------------
        public void TestThisShit()
        {
            //SideContent syf = GetSideFile(ReadSideFile(@"C:\Users\jaroslawk\Downloads\SideRipper1.side"));
            SideContent syf = GetSideContent(ReadSideFile(@"C:\Users\jaroslawk\Downloads\BigBrute666.side"));



            Console.WriteLine(syf.name);
            Console.WriteLine(syf.tests[0].commands[2].targets[0][0]);

            DataTableToCsvFile(@"C:\Users\jaroslawk\Downloads\BigBrute667.csv", SideContentToDataTable(syf));
        }

    }
}
