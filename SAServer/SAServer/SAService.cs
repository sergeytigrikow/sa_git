using ServerClasses.ServerFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SAServer
{
    public partial class SAService : ServiceBase
    {
        private ServerConnectionManager server;
        public SAService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            server = new ServerConnectionManager();
            server.StartServer();
        }

        protected override void OnStop()
        {
            server.ShotDownServer();
        }
    }
}
