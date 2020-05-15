using CommonClassLib;
using System;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.Text;

namespace ServerClasses.ServerFunctions
{
    public class DbServerWorkProvider : DbWorkProvider
    {
        public DbServerWorkProvider(string dbPath) : base(dbPath){}

        public bool Authorize(String username, String password)
        {
            var md5Provider = new MD5CryptoServiceProvider();
            String md5Pass = Encoding.Unicode.GetString(md5Provider.ComputeHash(Encoding.Unicode.GetBytes(password)));

            OleDbCommand authQuery = Connection.CreateCommand();
            authQuery.CommandText = String.Format("SELECT Password FROM Users WHERE User='{0}'", username);

            OleDbDataReader infoReader = null;
            try
            {
                infoReader = authQuery.ExecuteReader();
                if (infoReader != null)
                {
                    infoReader.Read();
                    var buffer = infoReader[0].ToString();
                    if (md5Pass == buffer) return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (infoReader != null) infoReader.Close();
            }
        }
    }
}
