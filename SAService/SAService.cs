using ServerClasses.ServerFunctions;
using System.ServiceProcess;

namespace SAService
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
