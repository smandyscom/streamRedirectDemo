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

            multiwayOulet mo = new multiwayOulet();
            mo.Way = multiwayOulet.interfaceSelection.FILE;
            mo.Write("Output to Filestream");
            mo.Way = multiwayOulet.interfaceSelection.TCP;
            mo.Write("Output to Remote TCP Server");
            mo.Way = multiwayOulet.interfaceSelection.PIPE;
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

    class multiwayOulet
    {
        public enum interfaceSelection
        {
            PIPE,
            TCP,
            FILE,
        }

        public interfaceSelection Way
        {
            set
            {
               currentOutputWay = cachedInterfaces[value]; 
            }
        }
        private StreamWriter currentOutputWay = null;

        private Dictionary<interfaceSelection,StreamWriter> cachedInterfaces = new Dictionary<interfaceSelection, StreamWriter>();
        public void Write(String message)
        {
            Console.SetOut(currentOutputWay); // redirect
            Console.WriteLine(message); // write-in , common interface
        }

        public multiwayOulet()
        {
           TcpClient __client  = new TcpClient(AddressFamily.InterNetwork);
           NamedPipeClientStream __npcs = new NamedPipeClientStream(".","test");
           FileStream __fs = new FileStream("./test.dat",FileMode.Append);

            //connection...
            __npcs.Connect();
            Task __task = __client.ConnectAsync("127.0.0.1",5001);
            while (!__task.IsCompleted);

            //preparing corresponding stream writers
            cachedInterfaces[interfaceSelection.TCP] = new StreamWriter(__client.GetStream());
            cachedInterfaces[interfaceSelection.PIPE] = new StreamWriter(__npcs);
            cachedInterfaces[interfaceSelection.FILE] = new StreamWriter(__fs);

        }
    }

}//namespace
