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
using Windows.Storage;

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
            // Get the sid stored in local settings with key "PackageSid"
            var client = new NamedPipeClientStream(".", 
                $"Sessions\\{Process.GetCurrentProcess().SessionId}\\AppContainerNamedObjects\\{ApplicationData.Current.LocalSettings.Values["PackageSid"]}\\mypipe",
                PipeDirection.InOut, PipeOptions.Asynchronous);

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
