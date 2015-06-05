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
    public sealed partial class MainPage : Page
    {
        private Windows.UI.Core.CoreDispatcher dispatcher;
        public MainPage()
        {
           
            this.InitializeComponent();
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
            ConnectionManager.Instance.Init();
            ConnectionManager.Instance.OnMasterDataReceived += Instance_OnMasterDataReceived;
            ConnectionManager.Instance.OnSlaveDataReceived += Instance_OnSlaveDataReceived;
        }

        void Instance_OnSlaveDataReceived(string data)
        {
            appendText("slave:" + data);
        }

        void Instance_OnMasterDataReceived(string data)
        {
            appendText("master:" + data);
        }


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
