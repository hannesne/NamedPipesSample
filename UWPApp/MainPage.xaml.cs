using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Win32.SafeHandles;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static StreamReader reader;
        private static StreamWriter writer;
        private DateTime lastSentMessage;
        private bool waitingForReply;

        public MainPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            string objectPath = GetPipePath();
            Debug.WriteLine(objectPath);

            Task.Run((Action) PipeServerThread);
        }

        private static string GetPipePath()
        {
            
            uint pathLength;
            bool result =
                GetAppContainerNamedObjectPath(IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, out pathLength);
            
            string objectPath = string.Empty;
            IntPtr pathPointer = Marshal.AllocHGlobal((int) (pathLength * 2));
            if (!result && Marshal.GetLastWin32Error() == 122)
            {
                result =
                    GetAppContainerNamedObjectPath(IntPtr.Zero, IntPtr.Zero, pathLength, pathPointer, out pathLength);
            }

            if (result)
            {
                objectPath = Marshal.PtrToStringUni(pathPointer);
                Marshal.FreeHGlobal(pathPointer);
            }
            else
            {
                throw new Exception("Couldn't figure out the named object path for the app container.");
            }

            return objectPath;
        }

        [DllImport("Kernel32.dll",CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetAppContainerNamedObjectPath(IntPtr Token, IntPtr AppContainerSid, UInt32 ObjectPathLength, IntPtr ObjectPath,out UInt32 ReturnLength);
        
        private void PipeServerThread()
        {
            NamedPipeServerStream serverStream = new NamedPipeServerStream(@"LOCAL\mypipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            Debug.WriteLine("Waiting for connection");
            
            serverStream.WaitForConnection();
            Debug.WriteLine("Connection established");

            reader = new StreamReader(serverStream);
            writer = new StreamWriter(serverStream);

            while (true)
            {
                var responseMessage = reader.ReadLine();

                string appendix;
                if (waitingForReply)
                {
                    waitingForReply = false;
                    TimeSpan duration = DateTime.Now - lastSentMessage;
                    appendix = $" rountrip duration: {duration.TotalMilliseconds}ms";
                }
                else
                {
                    appendix = string.Empty;
                }

                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReceivedMessages.Text +=
                    $"Received '{responseMessage}' from pipe {appendix} {Environment.NewLine}").AsTask().Wait();


               }

        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            if (writer == null)
            {
                ReceivedMessages.Text = "Pipe not open?";
                return;
            }
            lastSentMessage = DateTime.Now;
            waitingForReply = true;
            writer.WriteLine(Message.Text);
            writer.Flush();
            
        }

        private async void LaunchConsoleApp_OnClick(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            
        }

    }
}
