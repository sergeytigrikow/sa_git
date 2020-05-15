using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerClasses.ServerFunctions;
using CommonClassLib.Responces;

namespace ClientTests
{
    [TestClass]
    public class ConnectionTests
    {
        private ServerConnectionManager _server;

        [TestInitialize]
        public void Prepare()
        {
            _server = new ServerConnectionManager();
        }

        [TestMethod]
        public void ServerRuningTest()
        {
            _server.StartServer();
            Assert.AreEqual(_server.IsRunning, true);
            _server.ShotDownServer();
            Assert.AreEqual(_server.IsRunning, false);
            _server.StartServer();
            Assert.AreEqual(_server.IsRunning, true);
            _server.ShotDownServer();
        }


    }
}
