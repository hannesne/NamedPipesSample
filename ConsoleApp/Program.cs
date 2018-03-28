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
            Thread pipeServerThread = new Thread(new ThreadStart(PipeServerThread));
            pipeServerThread.Start();
            Console.ReadLine();
        }

        private static void PipeServerThread()
        {
            NamedPipeServerStream serverStream = new NamedPipeServerStream(@"LOCAL\mypipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Console.WriteLine("Waiting for connection");
            serverStream.WaitForConnection();
            Console.WriteLine("Connection established");

            StreamReader reader = new StreamReader(serverStream);
            StreamWriter writer = new StreamWriter(serverStream);
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
