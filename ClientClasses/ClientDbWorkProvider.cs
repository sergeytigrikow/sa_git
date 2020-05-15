using System.Collections.ObjectModel;
using System.Data.OleDb;
using CommonClassLib;
using System;
using System.Collections.Generic;
using CommonClassLib.Responces;
using System.Linq;

namespace ClientClasses
{
    public class ClientDbWorkProvider : DbWorkProvider
    {
        private ObservableCollection<AnalyzeResponce> _reports;
        public ObservableCollection<AnalyzeResponce> Reports
        {
            get
            {
                _reports = GetReportsList();
                return _reports;
            }
        }

        public ClientDbWorkProvider(String connectionString) : base(connectionString)
        {
        }

        public ObservableCollection<AnalyzeResponce> GetReportsList()
        {
            var reports = new ObservableCollection<AnalyzeResponce>();
            OleDbDataReader infoReader = null;
            try
            {
                var findQuery = Connection.CreateCommand();
                findQuery.CommandText = "SELECT * FROM Sites";
                infoReader = findQuery.ExecuteReader();
                if (infoReader == null) throw new Exception();
                while (true)
                {
                    infoReader.Read();
                    var url = infoReader["URL"].ToString();
                    var lastUpdate = infoReader["LastUpdate"].ToString();
                    var cy = infoReader["CY"].ToString();
                    var pr = infoReader["PR"].ToString();
                    var links = infoReader["Links"].ToString();
                    var clients = infoReader["Customers"].ToString();
                    var analyzes = new Dictionary<RequestType, string>
                        {
                            {RequestType.CY, cy}, 
                            {RequestType.PR, pr}, 
                            {RequestType.Links, links}, 
                            {RequestType.Customers, clients}
                        };
                    reports.Add(new AnalyzeResponce
                    {
                        LastUpdate = DateTime.Parse(lastUpdate),
                        Url = url,
                        Result = true,
                        Results = analyzes
                    });
                }
            }
            catch (Exception)
            {
                return reports;
            }
            finally
            {
                if (infoReader != null) infoReader.Close();
            }

        }

        public void DeleteReport(AnalyzeResponce responce)
        {
                var findQuery = Connection.CreateCommand();
                findQuery.CommandText = String.Format("Delete from Sites Where URL='{0}'", responce.Url);
                int k = findQuery.ExecuteNonQuery();
                if (k == 1) _reports.Remove(responce);
        }

        public void Update()
        {
            var list = GetReportsList();
            foreach (var analyzeResponce in list)
            {
                AnalyzeResponce toReplace = _reports.FirstOrDefault(x => x.Url == analyzeResponce.Url);
                if (toReplace == null) _reports.Add(analyzeResponce);
                else if (!analyzeResponce.Equals(toReplace))
                {
                    _reports.Remove(toReplace);
                    _reports.Add(analyzeResponce);
                }
            }
            
        }
    }
}
