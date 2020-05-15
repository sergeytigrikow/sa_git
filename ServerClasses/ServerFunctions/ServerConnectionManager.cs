using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CommonClassLib;
using CommonClassLib.Requests;
using CommonClassLib.Responces;
using System.IO;

namespace ServerClasses.ServerFunctions
{
    public class ConnectionInfo
    {
        public Socket Socket;
        public byte[] Buffer;
        public bool Authorized;
    }

    public class ServerConnectionManager : IDisposable
    {
        public bool IsRunning { get; private set; }

        public int ClientsOnline
        {
            get
            {
                lock (Connections)
                {
                    return Connections.Count;
                }
            }
        }

        private Socket _serverSocket;
        private const int Port = 9927;
        private static readonly List<ConnectionInfo> Connections = new List<ConnectionInfo>();
        private readonly RequestQueue _requests;
        private readonly RequestQueue _commands;

        public ServerConnectionManager()
        {
            
            _requests = new RequestQueue();
            _commands = new RequestQueue();
        }

        private void TranslateRequest(Request request, ConnectionInfo connection)
        {
            if ((connection.Authorized)
               || ((request is CommandRequest)
                    && (((CommandRequest)request).Command == CommandType.Authorize)))
            {
                if (request is AnalyzeRequest)
                {
                    _requests.Enqueue(request, connection);
                }
                if (request is CommandRequest)
                {
                    _commands.Enqueue(request, connection);
                }
            }
            else
            {
                SendMessage(connection, "Попытка не авторизованного доступа!");
                throw new Exception("Попытка не авторизованного доступа!");
            }
        }

        public void StartServer()
        {
            try
            {
                SetupServerSocket();
                _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
                IsRunning = true;
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
        }
        public void ShotDownServer()
        {
            try
            {
                lock (Connections)
                {
                    foreach (ConnectionInfo connection in Connections)
                    {
                        connection.Socket.Shutdown(SocketShutdown.Both);
                    }
                }
            }
            finally
            {
                lock (Connections)
                {
                    foreach (ConnectionInfo connection in Connections)
                    {
                        connection.Socket.Close();
                    }
                    Connections.Clear();
                }
                _serverSocket.Close();
                IsRunning = false;
            }


        }
        public static void SendResponce(ResponceEventArgs e)
        {
            try
            {
                Socket client = e.Node.Client.Socket;
                Responce resp = e.Responce;
                String jsonResponce = Serializator.Serialize(resp);
                byte[] toSend = Encoding.Unicode.GetBytes(jsonResponce);
                client.BeginSend(toSend, 0, toSend.Length, SocketFlags.None, SendCallback, e.Node.Client);
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
        }
        public static void SendMessage(ConnectionInfo connection, String msg)
        {
            try
            {
                byte[] toSend = Encoding.Unicode.GetBytes(msg);
                connection.Socket.Send(toSend, 0, toSend.Length, SocketFlags.None);
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
        }

        private void SetupServerSocket()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            String ip = null;
            using (var str = new StreamReader(new FileStream(@"D:\ip.dat", FileMode.Open, FileAccess.Read)))
            {
                ip = str.ReadToEnd();
            }
            IPAddress addr = IPAddress.Parse(ip);
            _serverSocket.Bind(new IPEndPoint(addr, Port));
            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
            Console.WriteLine("Начинаю слушать сокет {0}", _serverSocket.LocalEndPoint);
        }
        private void AcceptCallback(IAsyncResult result)
        {
            var connection = new ConnectionInfo();
            try
            {
                // Завершение операции Accept
                var s = (Socket)result.AsyncState;
                connection.Socket = s.EndAccept(result);
                Console.WriteLine("Подключен новый клиент: {0}", connection.Socket.RemoteEndPoint);
                connection.Buffer = new byte[512];
                connection.Authorized = false;
                lock (Connections) Connections.Add(connection);
                // Начало операции Receive и новой операции Accept
                _serverSocket.BeginAccept(AcceptCallback, result.AsyncState);
                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None,
                               ReceiveCallback, connection);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Приём соединений остановлен...");

            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " + exc.Message);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc.Message);
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            var connection = (ConnectionInfo)result.AsyncState;
            try
            {
                int bytesRead = connection.Socket.EndReceive(result);
                if (0 != bytesRead)
                {
                    Console.WriteLine("Получен запрос от {0}:", connection.Socket.RemoteEndPoint);
                    String buffer = Encoding.Unicode.GetString(connection.Buffer);
                    buffer = buffer.Substring(0, buffer.IndexOf('\0'));
                    Request req = Serializator.DeserializeRequest(buffer);
                    Console.WriteLine("Запрос поставлен в очередь на обработку...");
                    TranslateRequest(req, connection);
                    connection.Buffer = new byte[512];
                    Console.WriteLine("Продолжаю приём от {0}...", connection.Socket.RemoteEndPoint);
                    connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None,
                                                   ReceiveCallback, connection);
                }
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                if (exc is ObjectDisposedException) Console.WriteLine("Клиент отсоединен");
                else Console.WriteLine("Exception: " + exc.Message);
            }
        }
        private static void CloseConnection(ConnectionInfo ci)
        {

            try
            {
                if (ci.Socket == null) return;
                Console.WriteLine("Работа с {0} закончена.", ci.Socket.RemoteEndPoint);
                ci.Socket.Close();
                lock (Connections) Connections.Remove(ci);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Соединение закрыто.");
            }
        }
        private static void SendCallback(IAsyncResult res)
        {
            var node = (ConnectionInfo)res.AsyncState;
            int bytesSent = node.Socket.EndSend(res);
            if (!node.Authorized) CloseConnection(node);

            Console.WriteLine(bytesSent != 0 ? "Ответ отправлен!" : "Отправить ответ не удалось...");
        }

        public void Dispose()
        {
            try
            {
                lock (Connections)
                {
                    _serverSocket.Shutdown(SocketShutdown.Both);
                    foreach (var connectionInfo in Connections)
                    {
                        connectionInfo.Socket.Shutdown(SocketShutdown.Both);
                    }
                }
            }
            finally
            {
                foreach (var connectionInfo in Connections)
                {
                    connectionInfo.Socket.Close();
                }
                _serverSocket.Close();
            }
        }
    }
}
