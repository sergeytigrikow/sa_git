using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CommonClassLib;
using CommonClassLib.Requests;
using CommonClassLib.Responces;


namespace ClientClasses
{
    public sealed class ClientConnectionManager : IDisposable
    {
        public event EventHandler ResponceReceived;

        private readonly ClientDbWorkProvider _dbprovider;
        private Socket _clientSocket;
        private const int Port = 9927;
        public Responce Responce;
        private byte[] _outbuffer;
        private byte[] _inbuffer;

        private void OnResponceReceived()
        {
            EventHandler handler = ResponceReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public ClientConnectionManager(ClientDbWorkProvider provider)
        {
            _dbprovider = provider;
        }

        public bool IsConnected
        {
            get
            {
                return (_clientSocket != null) && _clientSocket.Connected;
            }
        }

        public void SendRequest(Request req, bool update)
        {
            try
            {
                if (req is AnalyzeRequest) Responce = GetResponceFromDb(req as AnalyzeRequest);
                if ((Responce == null) || (update))
                {
                    String str = Serializator.Serialize(req);
                    _outbuffer = Encoding.Unicode.GetBytes(str);
                    _clientSocket.BeginSend(_outbuffer, 0, _outbuffer.Length, SocketFlags.None, SendingCallback, _clientSocket);
                } 
            }
            catch (Exception exc)
            {
                LogInfo(exc.Message);
            }
        }

        public void ConnectToServer(String username, String password, String serverIp)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(IPAddress.Parse(serverIp), Port);
                LogInfo("Установлено соединение с сервером");
                Request req = new CommandRequest();
                ((CommandRequest)req).Command = CommandType.Authorize;
                ((CommandRequest)req).CommandInfo = String.Format("{0};{1}", username, password);
                byte[] toSend = Encoding.Unicode.GetBytes(Serializator.Serialize(req));
                _clientSocket.Send(toSend, 0, toSend.Length, SocketFlags.None);
                
                StartListening();
            }
           catch (SocketException exc)
            {
                LogInfo(exc.Message);
            }
            catch (Exception exc)
            {
                LogInfo(exc.Message);
                _clientSocket.Close();
            }
        }

        public void TurnOffConnection()
        {
            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                LogInfo("Соединение с сервером разорвано;");
            }
            catch (Exception exc)
            {
                LogInfo(exc.Message);
            }
            finally
            {
                _clientSocket.Close();
            }
        }

        private AnalyzeResponce GetResponceFromDb(AnalyzeRequest toCompare)
        {
            AnalyzeResponce respFromDb = _dbprovider.GetReport(toCompare.SiteUrl);
            if (respFromDb == null) return null;
            if (toCompare.Requests.Any(item => !respFromDb.Results.ContainsKey(item)))
            {
                return null;
            }
            if ((DateTime.Now - respFromDb.LastUpdate).TotalHours <= 1) 
                return respFromDb;
            return null;
        }

        private void SendingCallback(IAsyncResult result)
        {
            int bytesSend = 0;
            try
            {
                bytesSend = ((Socket)result.AsyncState).EndSend(result);
            }
            catch (Exception exc)
            {
                LogInfo(exc.Message);
            }
            if (bytesSend != 0) LogInfo("Запрос на анализ успешно отправлен;");
        }

        private void StartListening()
        {
            try
            {
                _inbuffer = new byte[512];

                _clientSocket.BeginReceive(_inbuffer, 0, _inbuffer.Length, SocketFlags.None,
                                            ReceiveCallback, _clientSocket);
            }
            catch (ObjectDisposedException)
            {
                LogInfo("Получение данных остановлено");
            }
            catch (Exception exc)
            {
                LogInfo(exc.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int bytesReceived = ((Socket)result.AsyncState).EndReceive(result);
                if (bytesReceived != 0)
                {
                    String respStr = Encoding.Unicode.GetString(_inbuffer);
                    respStr = respStr.Substring(0, respStr.IndexOf('\0'));
                    Responce = Serializator.DeserializeResponce(respStr);

                    if (Responce is AnalyzeResponce)
                        _dbprovider.SaveReport(Responce as AnalyzeResponce);

                    OnResponceReceived();
                    _inbuffer = new byte[512];
                    _clientSocket.BeginReceive(_inbuffer, 0, _inbuffer.Length, SocketFlags.None, ReceiveCallback, _clientSocket);
                }
                else _clientSocket.Close();
            }
            catch (ObjectDisposedException)
            {
                LogInfo("Получение данных остановлено;");
            }
            catch (Exception exc)
            {
                _clientSocket.Close();
                LogInfo(exc.Message);
            }
        }

        private void LogInfo(String msg)
        {
                Console.WriteLine("{0} - {1}", DateTime.Now, msg);
        }

        public void Dispose()
        {
            TurnOffConnection();
        }
    }
}
