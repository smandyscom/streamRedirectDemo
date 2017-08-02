using System;
using System.Net.Sockets;
using System.Net;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace streamRedirectionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(activateSimulationServer);// simulation/test use only

            outputSwitch mo = new outputSwitch();
            mo.Route = outputSwitch.routeSelections.FILE;
            mo.Write("Output to Filestream");
            mo.Route = outputSwitch.routeSelections.TCP;
            mo.Write("Output to Remote TCP Server");
            mo.Route = outputSwitch.routeSelections.PIPE;
            mo.Write("Output to PIPE for other process");

       }

        /// <summary>
        /// Test use 
        /// </summary>
        /// <param name="state"></param>
        static void activateSimulationServer(Object state)
        {
            TcpListener __tl = new TcpListener(IPAddress.Parse("127.0.0.1"),5001);
            __tl.Start();
            NamedPipeServerStream __npss = new NamedPipeServerStream("test");
            __npss.WaitForConnection();

            while (true)
            {
                Thread.Sleep(Timeout.Infinite);
            };// do nothing
        }
    }//program

    class outputSwitch
    {
        public enum routeSelections
        {
            PIPE,
            TCP,
            FILE,
        }

        public routeSelections Route
        {
            set
            {
               currentOutputWay = cachedInterfaces[value]; 
            }
        }
        private StreamWriter currentOutputWay = null;

        private Dictionary<routeSelections,StreamWriter> cachedInterfaces = new Dictionary<routeSelections, StreamWriter>();
        public void Write(String message)
        {
            Console.SetOut(currentOutputWay); // redirect
            Console.WriteLine(message); // write-in , common interface
        }

        public outputSwitch()
        {
           TcpClient __client  = new TcpClient(AddressFamily.InterNetwork);
           NamedPipeClientStream __npcs = new NamedPipeClientStream(".","test");
           FileStream __fs = new FileStream("./test.dat",FileMode.Append);

            //connection...
            __npcs.Connect();
            Task __task = __client.ConnectAsync("127.0.0.1",5001);
            while (!__task.IsCompleted);

            //preparing corresponding stream writers
            cachedInterfaces[routeSelections.TCP] = new StreamWriter(__client.GetStream());
            cachedInterfaces[routeSelections.PIPE] = new StreamWriter(__npcs);
            cachedInterfaces[routeSelections.FILE] = new StreamWriter(__fs);

        }
    }

}//namespace
