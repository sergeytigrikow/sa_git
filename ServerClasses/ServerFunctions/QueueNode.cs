using CommonClassLib.Requests;

namespace ServerClasses.ServerFunctions
{
    public class QueueNode
    {
        public Request Request;
        public ConnectionInfo Client;
        public QueueNode(Request req, ConnectionInfo connection)
        {
            Request = req;
            Client = connection;
        }
    }
}
