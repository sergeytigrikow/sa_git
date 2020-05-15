using CommonClassLib.Responces;
using System;

namespace ServerClasses.ServerFunctions
{
    public abstract class Proceeder
    {
        protected static readonly DbServerWorkProvider Dbprovider =
            new DbServerWorkProvider(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\ServerDB.accdb;Persist Security Info=True");

        public QueueNode RequestIProceed;
        public Responce Responce;

        public event EventHandler ProceedingFinished;

        protected void OnProceedingFinished(ResponceEventArgs e)
        {
            EventHandler proceedingFinished = ProceedingFinished;
            if (proceedingFinished != null)
                proceedingFinished(this, e);
        }

        public abstract void Proceed();
    }
}
