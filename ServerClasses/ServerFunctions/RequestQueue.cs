using System;
using System.Collections.Generic;
using System.Threading;
using CommonClassLib.Requests;

namespace ServerClasses.ServerFunctions
{
    class RequestQueue
    {

        private readonly Queue<QueueNode> _requests = new Queue<QueueNode>();
        private readonly List<Proceeder> _proceeders = new List<Proceeder>();
        private readonly List<Thread> _threads = new List<Thread>();
        private Thread _mainQueueThread;
        private bool _isRunning;

        public RequestQueue()
        {
            StartProceeding();
        }

        public void StartProceeding()
        {
            _mainQueueThread = new Thread(Proceed) {IsBackground = true};
            _mainQueueThread.Start();
        }

        public void StopProceeding()
        {
            foreach (Thread item in _threads)
            {
                item.Abort();
            }
            _mainQueueThread.Abort();
            _threads.Clear();
            _mainQueueThread = null;
        }

        public void Enqueue(Request req, ConnectionInfo connection)
        {
            _requests.Enqueue(new QueueNode(req, connection));
        }
        private void ProceedNext()
        {
            QueueNode node = _requests.Dequeue();
            Proceeder proceeder;
            if (node.Request is AnalyzeRequest) proceeder = new AnalyzeProceeder(node);
            else proceeder = new CommandProceeder(node);
            
            _proceeders.Add(proceeder);
            proceeder.ProceedingFinished += reporter_AnalyzeFinished;
            var thread = new Thread(proceeder.Proceed) {IsBackground = true};
            _threads.Add(thread);
            thread.Start();
        }
        private void reporter_AnalyzeFinished(object sender, EventArgs e)
        {
            var args = e as ResponceEventArgs;
            ServerConnectionManager.SendResponce(args);
        }
        private void Proceed()
        {
            _isRunning = true;
            while (_isRunning)
            {
                if (_requests.Count != 0)
                {
                    ProceedNext();
                }
                else Thread.Sleep(1000);
            }

        }


    }
}
