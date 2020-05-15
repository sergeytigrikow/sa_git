using CommonClassLib;
using CommonClassLib.Requests;
using CommonClassLib.Responces;

namespace ServerClasses.ServerFunctions
{
    class CommandProceeder : Proceeder
    {
        public CommandProceeder(QueueNode node)
        {
            RequestIProceed = node;
        }

        public override void Proceed()
        {
            var request = RequestIProceed.Request as CommandRequest;
            var responce = new CommandResponce();
            if (request == null) return;
            switch (request.Command)
            {
                case CommandType.Authorize:
                    {
                        Authorization(request, responce);
                        break;
                    }
            }
            Responce = responce;
            OnProceedingFinished(new ResponceEventArgs(Responce, RequestIProceed));
        }

        private void Authorization(CommandRequest req, CommandResponce responce)
        {
            //var userInfo = req.CommandInfo.Split(';');
            //bool loginSuccess = Dbprovider.Authorize(userInfo[0], userInfo[1]);
            bool loginSuccess = true;
            responce.Message = loginSuccess 
                               ? "Авторизация прошла успешно!" 
                               : "Не существует такой комбинации пользователя и пароля!";
            responce.Result = loginSuccess;
            RequestIProceed.Client.Authorized = loginSuccess;

        }
    }
}
