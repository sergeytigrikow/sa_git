using HtmlAgilityPack;
using ServerClasses.ServerFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            /*
            string file = @"D:\Dev\Korsak\resp.html";
            string sDocument = File.ReadAllText(file);

            var doc = new HtmlDocument();
            doc.Load(new StringReader(sDocument));

            string hosts = doc.DocumentNode.SelectSingleNode(@"//body/div").InnerText.Trim();

            string[] parts = hosts.Split(new string[] { "Total Visits" }, 2, StringSplitOptions.RemoveEmptyEntries);
            string totalVisits = parts.Length >= 2 ? (parts[1] ?? string.Empty) : string.Empty;

            totalVisits = totalVisits.Trim();

            parts = totalVisits.Split(new string[] { "\n" }, 2, StringSplitOptions.RemoveEmptyEntries);

            totalVisits = parts.Length >= 2 ? (parts[0] ?? string.Empty) : string.Empty;
            */
            
            ServerConnectionManager server = new ServerConnectionManager();
            server.StartServer();
            Console.ReadLine();
        }
    }
}
