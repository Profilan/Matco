using Matco.Logic.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Matco.Service
{
    public partial class MatcoListenerService : ServiceBase
    {
        private SocketListener _socketListener;



        public MatcoListenerService()
        {
            InitializeComponent();

            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("Matco Listener"))
            {
                System.Diagnostics.EventLog.CreateEventSource("Matco Listener", "Matco Services");
            }
            eventLog1.Source = "Matco Listener";
            eventLog1.Log = "Matco Services";
        }

        protected override void OnStart(string[] args)
        {
            string ip = ConfigurationManager.AppSettings["SocketIP"];

            try
            {
                _socketListener = new SocketListener(60050, 2048);
                _socketListener.ServerListening += _socketListener_ServerListening;
                _socketListener.ClientForcefullyDisconnected += _socketListener_ClientForcefullyDisconnected;
                _socketListener.ClientConnected += _socketListener_ClientConnected;
                _socketListener.ClientDisconnected += _socketListener_ClientDisconnected;
                _socketListener.TextReceived += _socketListener_TextReceived;
                _socketListener.Start();
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        private void _socketListener_ServerListening(string message)
        {
            eventLog1.WriteEntry(message, System.Diagnostics.EventLogEntryType.Information, 1003);
        }

        protected override void OnStop()
        {
            _socketListener.Stop();
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        private void _socketListener_TextReceived(string text)
        {
            MatcoWriter.Write(text);
            eventLog1.WriteEntry("Text received: " + text, System.Diagnostics.EventLogEntryType.Information, 1000);
        }

        private void _socketListener_ClientDisconnected()
        {
            eventLog1.WriteEntry("Client disconnected", System.Diagnostics.EventLogEntryType.Information, 1002);
        }

        private void _socketListener_ClientForcefullyDisconnected()
        {
            eventLog1.WriteEntry("Client forcefully disconnected", System.Diagnostics.EventLogEntryType.Warning, 1001);
        }

        private void _socketListener_ClientConnected()
        {
            eventLog1.WriteEntry("Client connected", System.Diagnostics.EventLogEntryType.Information, 1000);
        }
    }
}
