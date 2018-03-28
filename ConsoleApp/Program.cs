using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Thread pipeServerThread = new Thread(new ThreadStart(PipeClientThread));
            pipeServerThread.Start();
            Console.ReadLine();
        }

        private static void PipeClientThread()
        {
            var client = new NamedPipeClientStream(".", @"LOCAL\mypipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            Console.WriteLine("Connecting");

            client.Connect(5000);

            Console.WriteLine("Connection established");

            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);
            while (true)
            {
                var line = reader.ReadLine();
                Console.WriteLine($"Message: {line}");

                writer.WriteLine(String.Join("", line.Reverse()));
                writer.Flush();
            }

        }
    }
}
