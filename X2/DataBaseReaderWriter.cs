using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace X2
{
    class DataBaseReaderWriter
    {
        SqlConnection connection;

        public string TryQueryToDataTable(string connectionStr, string query, bool showExcMsg, out DataTable dataTable) //true jeżeli uda się odczytać zawartość tablicy
        {
            dataTable = new DataTable();

            connection = new SqlConnection(connectionStr);

            using (connection)
            {
                try
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    using (da)
                    {
                        da.Fill(dataTable);
                    }

                    connection.Close();
                }
                catch (SqlException e)
                {
                    string err = "SqlException caught in DataBaseReaderWriter.QueryResultTable(). Query: \"" + query + "\"\r\n. Connection string: \"" + connectionStr + "\"\r\n. Exception:\r\n" + e;
                    if (showExcMsg)
                    {
                        MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Console.WriteLine(err);
                    return err;
                }
            }

            if (dataTable.Columns.Count == 0) //surogat na wypadek insert, update, delete; od porażki odróżniane boolem
            {
                dataTable.Columns.Add("c_NO_RESULT_TABLE");
                DataRow r = dataTable.NewRow();
                r[0] = "NO_RESULT_TABLE";
                dataTable.Rows.Add(r);
            }

            return "ok";            
        }
    }
}
