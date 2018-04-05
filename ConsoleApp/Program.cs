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
        static StreamReader reader;
        static StreamWriter writer;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Thread pipeServerThread = new Thread(new ThreadStart(PipeClientThread));
            pipeServerThread.Start();

            while (true)
            {
                string sendMessage = Console.ReadLine();
                writer.WriteLine(sendMessage);
                writer.Flush();

            }
        }


        private static void PipeClientThread()
        {
            //get the pipename in powershell by running up the app, and then executing  [System.IO.Directory]::GetFiles("\\.\\pipe\\") | where {$_ -like '*mypipe*'} in powershell.
            var client = new NamedPipeClientStream(".", @"Sessions\1\AppContainerNamedObjects\S-1-15-2-753128950-1760965839-196781726-1165193043-2994346047-4209839368-3121518441\mypipe", PipeDirection.InOut, PipeOptions.Asynchronous);

            client.Connect(5000);

            Console.WriteLine("Connection established");

            reader = new StreamReader(client);
            writer = new StreamWriter(client);
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
