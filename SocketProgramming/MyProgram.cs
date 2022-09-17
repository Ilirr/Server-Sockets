using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace SocketProgramming
{
    internal class MyProgram
    {

        public Socket ServerSocket { get; set; }
        public Socket ServerHandler { get; set; }
        public Socket ServerListener { get; set; }
        public Socket ClientSocket { get; set; }
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int PORT = 10203;

        public class StateObject
        {
            public const int BUFFER_SIZE = 1024;
            public byte[] buffer = new byte[BUFFER_SIZE];
            public Socket workSocket = null;
        }
        private void SetupServer()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ipAddress, PORT);
            ServerListener = new Socket(ipAddress.AddressFamily,SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Setting up server...");  
            ServerListener.Bind(endPoint);
            ServerListener.Listen(100);
            ServerListener.BeginAccept(new AsyncCallback(ConnectCallback), ServerListener);
            Console.WriteLine("Server setup complete");
        }

       
        private void ConnectToServer()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ipAddress, PORT);
            ClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);
                    //ClientSocket.Connect(ipAddress, PORT,remoteEP);
                    ClientSocket.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), ClientSocket);
                }
                catch (SocketException)
                { 
                    
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }
        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                Socket client = (Socket)AR.AsyncState;
                client.EndConnect(AR);
                StateObject state = new StateObject();
                state.workSocket = client;
                clientSockets.Add(client);
               // client.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                Console.WriteLine("Client connected, waiting for request...");
               // client.BeginAccept(ConnectCallback, client);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }
        private static void ReceiveCallback(IAsyncResult AR)
        {
            StateObject state = (StateObject)AR.AsyncState;
            Socket client = state.workSocket;
            int received;

            try
            {
                received = client.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                client.Close();
                clientSockets.Remove(client);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(state.buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);
            client.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
        }
        public void Run()
        {
            Console.WriteLine("client1,server2");
            int a = Convert.ToInt32(Console.ReadLine());
            if (a == 1)
            {
                Client();
            }
            else if (a == 2)
            {
                Server();
            }
        }

        void Client()
        {
            Console.Title = "Client";
            ConnectToServer();
            RequestLoop();
            Exit();
        }

        void Server()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine();
            CloseAllSockets();
        }

        private void RequestLoop()
        {
            while (true)
            {
                SendRequest();
                ReceiveResponse();
            }
        }
        private void SendRequest()
        {
            Console.Write("Send a request: ");
            string request = Console.ReadLine();
            SendString(request);
        }
        private void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private void ReceiveResponse() 
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            var buffer = new byte[1024];
            int bytesRead = 
            var response = ClientSocket.BeginReceive(buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None,new AsyncCallback(ReceiveCallback),state);
            if (!response.IsCompleted) return;
            var data = new byte[response];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);
        }
        private void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            ServerSocket.Close();
        }
        private void Exit()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }
    }
}
