using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Matco.Logic.Services
{
    public class StateObject
    {
        // Client socket
        public Socket WorkSocket { get; set; }

        // Size of receive buffer
        public int BufferSize { get; private set; }

        // Receive buffer
        public byte[] Buffer { get; set; }

        // Received data string
        public StringBuilder Data { get; set; }

        public StateObject(int bufferSize)
        {
            BufferSize = bufferSize; ;
            Buffer = new byte[bufferSize];
            Data = new StringBuilder();
        }
    }

    public class SocketListener
    {
        #region Properties

        public Socket ServerSocket { get; private set; }
        public List<Socket> ClientSockets { get; private set; }
        public int Port { get; private set; }
        public int BufferSize { get; private set; }
        public byte[] Buffer { get; private set; }

        #endregion

        #region Events

        public delegate void ClientConnectedHandler();
        public event ClientConnectedHandler ClientConnected;

        public delegate void ClientForcefullyDisconnectedHandler();
        public event ClientForcefullyDisconnectedHandler ClientForcefullyDisconnected;

        public delegate void ClientDisconnectedHandler();
        public event ClientConnectedHandler ClientDisconnected;

        public delegate void TextReceivedHandler(string text);
        public event TextReceivedHandler TextReceived;

        public delegate void ServerListeningHandler(string message);
        public event ServerListeningHandler ServerListening;

        #endregion


        public SocketListener(int port, int bufferSize)
        {
            Port = port;
            BufferSize = bufferSize;
            Buffer = new byte[bufferSize];

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSockets = new List<Socket>();
        }

        public void Start()
        {
            SetupServer();
        }

        public void Stop()
        {
            CloseAllSockets();
        }

        private void SetupServer()
        {
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(0);
            ServerSocket.BeginAccept(AcceptCallback, null);
            ServerListening?.Invoke("Server listening on port " + Port);
        }

        private void CloseAllSockets()
        {
            foreach (Socket socket in ClientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            ServerSocket.Close();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket;

            try
            {
                socket = ServerSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            ClientSockets.Add(socket);
            socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, socket);
            // Console.WriteLine("Client connected, waiting for request...");
            ClientConnected?.Invoke();
            ServerSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket current = (Socket)ar.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(ar);
            }
            catch (SocketException)
            {
                // Console.WriteLine("Client forcefully disconnected");
                ClientForcefullyDisconnected?.Invoke();
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                ClientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(Buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            // Console.WriteLine("Received Text: " + text);

            if (text.ToLower() == "")
            {
                ClientDisconnected?.Invoke();
                return;
            }
            else
            {
                byte[] data = Encoding.ASCII.GetBytes("Received");
                current.Send(data);
                TextReceived?.Invoke(text);
            }

            current.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, current);

        }
    }
}
