#define ISHOST
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using App1;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hackday
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IFileOpenPickerContinuable
    {
        private Windows.UI.Core.CoreDispatcher dispatcher;
        SenderData sd;
        public MainPage()
        {
            this.InitializeComponent();
            sd = new SenderData();
            dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            Pair();
        }

        private async void Pair()
        {
            await Task.Yield();

            // PeerFinder.Start() is used to advertise our presence so that peers can find us. 
            // It must always be called before FindAllPeersAsync.
            PeerFinder.Start();

            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;
        }

        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            try
            {
                if (ShouldConnect())
                {
                    // Go ahead and connect
                    ConnectToPeer(args.PeerInformation);
                }
            }
            catch (Exception e)
            {
                MessageDialog d = new MessageDialog(e.Message);
                d.ShowAsync();
            }

        }

        async void ConnectToPeer(PeerInformation peer)
        {
            try
            {
                appendText("connection requested");
                byte[] buf = new byte[2];
                var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                StreamSocket socket = await PeerFinder.ConnectAsync(peer);
                appendText("\nconnection established");
                using (var stream = socket.InputStream)
                {
                    await stream.ReadAsync(buffer, 2, Windows.Storage.Streams.InputStreamOptions.None);
                    appendText("\n data read \n");
                    foreach (var x in buffer.ToArray())
                    {
                        appendText("\n" + x);
                    }
                }
            }
            catch (Exception e)
            {
                MessageDialog d = new MessageDialog(e.Message);
                d.ShowAsync();
            }
        }

        private bool ShouldConnect()
        {
            // Determine whether to accept this connection request and return
            return true;
        }

        void appendText(string text)
        {
            dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                mytext.Text = mytext.Text + text;
            });
        }

        List<StreamSocket> sockets = new List<StreamSocket>(2);

        private async void con_Click(object sender, RoutedEventArgs e)
        {
            // Configure PeerFinder to search for all paired devices.
            //PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var peers = await PeerFinder.FindAllPeersAsync();

            if (peers.Count == 0)
            {
                appendText("No paired devices were found.");
            }
            else
            {
                // Attempt a connection
                byte[] buf = new byte[2];
                buf[0] = 1;
                buf[1] = 2;
                foreach (var selectedPeer in peers)
                {
                    try
                    {
                        //DeviceInformationCollection DeviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

                        //var numDevices = DeviceInfoCollection.Count();

                        //if (numDevices == 0)
                        //{
                        //    MessageDialog md = new MessageDialog("No paired devices found", "Title");
                        //    await md.ShowAsync();

                        //    return;
                        //}

                        //DeviceInformation DeviceInfo = DeviceInfoCollection[0];
                        //var service = await RfcommDeviceService.FromIdAsync(DeviceInfo.Id);

                        //StreamSocket socket = new StreamSocket();
                        //await socket.ConnectAsync(selectedPeer.HostName, "1");
                        //await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, service.ProtectionLevel);

                        StreamSocket socket = await PeerFinder.ConnectAsync(selectedPeer);
                        sockets.Add(socket);

                    }
                    catch (Exception ex)
                    {
                        appendText(ex.Message);
                    }
                }

                var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                foreach (var s in sockets)
                {
                    var x = await s.OutputStream.WriteAsync(buffer);
                }

                //using (var streamSocket = await PeerFinder.ConnectAsync(selectedPeer))
                //{
                //    var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                //    var x = await streamSocket.OutputStream.WriteAsync(buffer);
                //}
            }
        }

        private async void lst_Click(object sender, RoutedEventArgs e)
        {
            var peers = await PeerFinder.FindAllPeersAsync();
            if (peers.Count == 0)
            {
                appendText("No paired devices were found.");
            }
            else
            {
                // Select a peer. In this example, let's just pick the first peer.
                //enumerate all peers
                int i = 0;
                foreach (var peer in peers)
                {
                    appendText("Device " + i++ + " / name: " + peer.DisplayName);
                }
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            openPicker.FileTypeFilter.Add(".mp3");
            // openPicker.CommitButtonText = "send";
            openPicker.PickMultipleFilesAndContinue();
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            IReadOnlyList<StorageFile> files = args.Files;
            if (files.Count > 0)
            {
                // Application now has read/write access to the picked file(s)
                foreach (StorageFile file in files)
                {
                    Stream stream = await file.OpenStreamForReadAsync();
                    byte[] bytearray = new byte[(int)stream.Length];
                    bytearray = ConverToByteArray(stream);
                    SendSong(bytearray);
                    //play.SetSource(stream.AsRandomAccessStream(), "audio/mpeg3");
                    //play.Play();
                }
            }
            else
            {
            }
        }

        public byte[] ConverToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public void SendSong(byte[] byteArrary)
        {
            sd.SendActionToServer(SenderData.CommandList.ADD, -1, byteArrary);
        }
    }
}
