using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Data;

//inne namespace, bo X2 (właściwy QA Cat) nigdy nie będzie korzystał bezpośrednio z .side; side przyspiesza stworzenie scenariusza, ale jego nadal tworzy się ręcznie
//nie ma szans, żeby konwersja wystarczyła do stworzenia sensownego scenariusza (oczekiwanie, zapamiętywanie zmiennych, ogólny śmietnik w side)            
namespace SideConverter                       
{

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

        void SaveDataTableToCsvFile(string fileName, DataTable dataTable)
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
            dataTable.Columns.Add("Description");
            dataTable.Columns.Add("Operation");
            dataTable.Columns.Add("Text");
            dataTable.Columns.Add("XPath");
            
            DataRow row = dataTable.NewRow();

            
            row[0] = "plugin init: load website";
            row[1] = "goToUrl";
            row[2] = content.url;
            row[3] = "";
            dataTable.Rows.Add(row);
            row = dataTable.NewRow();
            row[0] = "plugin init: reload website";
            row[1] = "refresh";
            row[2] = "";
            row[3] = "";
            dataTable.Rows.Add(row);
            

            foreach (SideContent.Test t in content.tests)
            {
                foreach (SideContent.Test.Command c in t.commands)
                {
                    row = dataTable.NewRow();

                    row[0] = "plugin: " + c.command; //todo
                    row[1] = c.command;
                    row[2] = c.value ?? "";

                    if(c.targets.Count() > 0)
                    {                        
                        row[3] = (c.targets.Where(tr => tr[0].Contains("xpath") && tr[1].Contains("idRelative")).FirstOrDefault() 
                            ?? c.targets.Where(tr => tr[0].Contains("xpath") && tr[1].Contains("attributes")).FirstOrDefault()
                            ?? c.targets.Where(tr => tr[0].Contains("xpath") && tr[1].Contains("position")).FirstOrDefault()
                            ?? c.targets.Where(tr => tr[0].Contains("xpath")).FirstOrDefault()
                            ?? new string[] { "no xpath found (inner)" })[0] ?? "no xpath found (outer)"; 

                        //dla google / aol chrzani się atributes
                        //dla tarisa nie ma relative
                        //1. relative
                        //2. attributes
                        //3. position
                        //4. dowolne
                    }                    

                    dataTable.Rows.Add(row);
                }
            }

            dataTable = TranslateSideContentDT(dataTable);

            return dataTable;
        }

        //do uporządkowania!!!
        //takie operacjie na DataTable robi się beznadziejnie; na początku powinna być konwersja do string[][] i na koniec odwrotna
        //to samo w validatorze
        //i właściwie to wszystkie datatable można by wyrugować z projektu
        private DataTable TranslateSideContentDT(DataTable contentTable)
        {
            DataTable outputTable = contentTable.Copy();

            List<int> rowsForDeletion = new List<int>();

            //update-y
            foreach (DataRow row in outputTable.Rows)
            {
                if (row[3].ToString().Contains("xpath="))
                {
                    row[3] = row[3].ToString().Substring(6);                    
                }

                if (row[2].ToString().Contains("label="))
                {
                    row[2] = row[2].ToString().Substring(6);
                }

                if (row[1].ToString().Contains("sendKeys") && row[2].ToString().Contains("${KEY_ENTER}"))
                {
                    row[2] = "Enter";
                }
            }


            //select - komenda zdefiniowana w dwóch wierszach
            outputTable.AcceptChanges();
            for (int i = 0; i < outputTable.Rows.Count; i++)
            {
                if (outputTable.Rows[i][1].ToString().Contains("select") && (i > 1))
                {
                    outputTable.Rows[i][3] = outputTable.Rows[i - 1][3].ToString();
                    outputTable.Rows[i - 1].Delete();
                }
            }
            outputTable.AcceptChanges();

            //delete-y
            List<string> unwantedComands = new List<string>() { "setWindowSize", "open", "mouseDown", "mouseUp", "mouseOut", "doubleClick" };
            string[] lastRow = new string[4] { "", "", "", "" };
            outputTable.AcceptChanges(); //mogę kasować w enumeracji, bo data table jest transakcyjne
            foreach (DataRow row in outputTable.Rows)
            {
                //nieobsługiwane komendy
                string command = row[1].ToString();
                if (unwantedComands.Any(t => t.Contains(command)))
                {
                    row.Delete();
                    continue;
                }

                
                //kliknięcie nastepujące po, dodane przez plugin, zbędne
                if (row[1].ToString().Contains("click") && !lastRow[1].ToString().Contains("click") && row[3].ToString() == lastRow[3].ToString())
                {
                    row.Delete();
                    continue;
                }
                

                //duplikaty
                if ((lastRow[0] == row[0].ToString()) && (lastRow[1] == row[1].ToString()) && (lastRow[2] == row[2].ToString()) && (lastRow[3] == row[3].ToString()))
                {
                    row.Delete();
                }
                else
                {
                    //row.ItemArray.CopyTo(lastRow, 0); //ma być ostatnie w iteracji
                    lastRow[0] = row[0].ToString();
                    lastRow[1] = row[1].ToString();
                    lastRow[2] = row[2].ToString();
                    lastRow[3] = row[3].ToString();

                }



            }
            outputTable.AcceptChanges();

            
            outputTable.AcceptChanges();
            foreach (DataRow row in outputTable.Rows)
            
            outputTable.AcceptChanges();

            outputTable.AcceptChanges();
            foreach (DataRow row in outputTable.Rows)
            {

            }
            outputTable.AcceptChanges();


            return outputTable;
        }
        
        
        public void ReadSideSaveCsv(string sideFileName, string csvFileName)
        {
            SideContent content = GetSideContent(ReadSideFile(sideFileName));
            SaveDataTableToCsvFile(csvFileName, SideContentToDataTable(content));
        }

    }
}
