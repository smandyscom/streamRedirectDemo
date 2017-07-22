using System;
using System.Net.Sockets;
using System.Net;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
namespace streamRedirectionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(activateServer);//

            TcpClient __client = new TcpClient(AddressFamily.InterNetwork);
            NamedPipeClientStream __npcs = new NamedPipeClientStream(".","test");
            FileStream __fs = new FileStream("./test.dat",FileMode.Append);

            //connection...
            __npcs.Connect();
            Task __task = __client.ConnectAsync("127.0.0.1",5001);
            while (!__task.IsCompleted);

            WriteRoutine(__client.GetStream());//re-direct output to some socket
            WriteRoutine(__npcs);//re-direct output to pipe
            WriteRoutine(__fs);//re-direct output to file
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="__stream"></param>
        static void WriteRoutine(Stream __stream)
        {
            using(StreamWriter sw = new StreamWriter(__stream))
            {
                Console.SetOut(sw); // redirect
                Console.WriteLine(DateTime.Now.ToString()); // write-in , common interface
            }
        }

        static void activateServer(Object state)
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
    }
}
