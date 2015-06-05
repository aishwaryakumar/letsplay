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
using Hackday;

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
            sd.ActionRequested += sd_ActionRequested;
            dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            this.NavigationCacheMode = NavigationCacheMode.Required;
           
        }

        private void sd_ActionRequested(SenderData.Command cmd)
        {
            if (cmd != null)
            {
                switch (cmd.command)
                {
                    case CommandList.ADD:
                        
                        break;
                    case CommandList.REMOVE:
                        break;
                    case CommandList.NEXT:
                        break;
                    case CommandList.PREVIOUS:
                        break;
                    case CommandList.TOGGLEPLAYSTATE:
                        break;
                    default:
                        break;
                }
            }
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
            ConnectionManager.Instance.Init();
        //    ConnectionManager.Instance.OnMasterDataReceived += Instance_OnMasterDataReceived;
        //    ConnectionManager.Instance.OnSlaveDataReceived += Instance_OnSlaveDataReceived;
        }

        //void Instance_OnSlaveDataReceived(string data)
        //{
        //    appendText("slave:" + data);
        //}

        //void Instance_OnMasterDataReceived(string data)
        //{
        //    appendText("master:" + data);
        //}


        void appendText(string text)
        {
            dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                mytext.Text = mytext.Text + text;
            });
        }

        private async void con_Click(object sender, RoutedEventArgs e)
        {
            // Configure PeerFinder to search for all paired devices.
            //PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var connectedpeers = await ConnectionManager.Instance.StartPairing();
            appendText(connectedpeers);
        }

        private async void lst_Click(object sender, RoutedEventArgs e)
        {
            var peers = await PeerFinder.FindAllPeersAsync();
            if (peers.Count == 0)
            {
                appendText("No paired devices were found.\n");
            }
            else
            {
                // Select a peer. In this example, let's just pick the first peer.
                //enumerate all peers
                int i = 0;
                foreach (var peer in peers)
                {
                    appendText("Device " + i++ + " / name: " + peer.DisplayName + "\n");
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
            sd.SendActionToServer(CommandList.ADD, -1, byteArrary);
		}
		
        private void data_Click(object sender, RoutedEventArgs e)
        {
            ConnectionManager.Instance.SendData("" + DateTime.Now);
        }
         private void go_media(object sender, RoutedEventArgs e)
        {
            Frame f = Window.Current.Content as Frame;
            f.Navigate(typeof(MediaPlayer));
        }
    }
}
