using System;
using CommonClassLib.Responces;

namespace ServerClasses.ServerFunctions
{
    public class ResponceEventArgs : EventArgs
    {
        public Responce Responce { get; private set; }
        public QueueNode Node { get; private set; }
        public ResponceEventArgs(Responce res, QueueNode req)
        {
            Responce = res;
            Node = req;
        }
    }
}
