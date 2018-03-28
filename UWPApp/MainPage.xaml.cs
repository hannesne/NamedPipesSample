using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StreamReader reader;
        private StreamWriter writer;

        public MainPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //Task.Run((Action) PipeServerThread);
        }



        private static void PipeServerThread()
        {
            NamedPipeServerStream serverStream = new NamedPipeServerStream(@"LOCAL\mypipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Debug.WriteLine("Waiting for connection");
            serverStream.WaitForConnection();
            Debug.WriteLine("Connection established");

            StreamReader reader = new StreamReader(serverStream);
            StreamWriter writer = new StreamWriter(serverStream);
            while (true)
            {
                var line = reader.ReadLine();
                Debug.WriteLine($"Message: {line}");

                writer.WriteLine(String.Join("", line.Reverse()));
                writer.Flush();
            }

        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            DateTime pipeRequest = DateTime.Now;
            writer.WriteLine(Message.Text);
            writer.Flush();
            reader.ReadLine();
            DateTime pipeResponse = DateTime.Now;
            TimeSpan pipeDuration = pipeResponse - pipeRequest;
            ReceivedMessages.Text +=
                $"Received response from pipe: {pipeResponse} after {pipeDuration.TotalMilliseconds}ms";
        }

        private async void LaunchConsoleApp_OnClick(object sender, RoutedEventArgs e)
        {
            await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            var client = new NamedPipeClientStream(".",@"LOCAL\mypipe", PipeDirection.InOut, PipeOptions.Asynchronous);

            client.Connect(5000);

            reader = new StreamReader(client);
            writer = new StreamWriter(client);

           
        }
    }
}
