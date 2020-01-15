using Matco.Logic.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatcoListenerTest
{
    class Program
    {
        

        static void Main(string[] args)
        {
            // string path = @"C:\Users\raymond\source\repos\Matco\MatcoSender\bin\Debug\MatcoSender.exe";
            // Process.Start(path);

            var listener = new SocketListener("172.16.60.10", 60050, 1024);
            listener.dataReceivedEvent += Listener_dataReceivedEvent1;
            listener.StartListening();
        }

        private static void Listener_dataReceivedEvent1(string data)
        {
            Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", data.Length, data);
        }

        private static void Listener_dataReceivedEvent(string data)
        {
            Console.WriteLine(data);
        }
    }
}
