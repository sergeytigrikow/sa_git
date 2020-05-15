using System;
using System.Data.OleDb;
using System.Text;
using CommonClassLib.Responces;

namespace CommonClassLib
{
    public class DbWorkProvider : IDisposable
    {
        protected readonly OleDbConnection Connection;

        public DbWorkProvider(String connectionString)
        {
            Connection = new OleDbConnection(connectionString);
            Connection.Open();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Проверка запросов SQL на уязвимости безопасности")]
        public int SaveReport(AnalyzeResponce resp)
        {
            OleDbCommand findQuery = Connection.CreateCommand();
            var types = new StringBuilder(String.Empty);
            var values = new StringBuilder(String.Empty);
            foreach (var item in resp.Results)
            {
                types.AppendFormat("{0}, ", item.Key);
                values.AppendFormat("'{0}', ", item.Value);
            }
            types.Remove(types.Length - 2, 2);
            values.Remove(values.Length - 2, 2);
            String insertCommand = String.Format(@"INSERT INTO Sites (URL, LastUpdate, {0}) VALUES ('{1}', '{2}', {3})",
                types,
                resp.Url,
                resp.LastUpdate,
                values
                );

            while (true)
            {
                try
                {
                    findQuery.CommandText = insertCommand;
                    findQuery.ExecuteNonQuery();
                    return 0;
                }
                catch (OleDbException exc)
                {
                    if (exc.ErrorCode == -2147467259)
                    {
                        findQuery.CommandText = String.Format("DELETE FROM Sites WHERE URL='{0}'", resp.Url);
                        try
                        {
                            findQuery.ExecuteNonQuery();
                        }
                        catch (OleDbException)
                        {
                            return -1;
                        }
                    }
                    else return -1;
                }
            }  
        }

        public AnalyzeResponce GetReport(String url)
        {
            OleDbCommand findQuery = Connection.CreateCommand();
            findQuery.CommandText = String.Format(@"SELECT * FROM Sites Where URL = '{0}'", url);

            OleDbDataReader infoReader = null;
            try
            {
                infoReader = findQuery.ExecuteReader();
                if (infoReader != null)
                {
                    infoReader.Read();
                    var resp = new AnalyzeResponce
                    {
                        Url = infoReader["URL"].ToString(),
                        LastUpdate = DateTime.Parse(infoReader["LastUpdate"].ToString())
                    };
                    foreach (var item in Enum.GetValues(typeof(RequestType)))
                    {
                        String columnName = item.ToString();
                        String buffer = infoReader[columnName].ToString();
                        if (buffer == String.Empty) continue;
                        resp.Results.Add((RequestType)item, buffer);
                    }
                    return resp;
                }
                else return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            finally
            {
                if (infoReader != null) infoReader.Close();
            }

        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
