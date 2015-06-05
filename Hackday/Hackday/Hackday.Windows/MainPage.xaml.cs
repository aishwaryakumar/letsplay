using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hackday
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //Do the bluetooth thing
            Pair();
        }

        private async void Pair()
        {
            await Task.Yield();

            // PeerFinder.Start() is used to advertise our presence so that peers can find us. 
            // It must always be called before FindAllPeersAsync.
            PeerFinder.Start();

            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;


            // Configure PeerFinder to search for all paired devices.
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var peers = await PeerFinder.FindAllPeersAsync();

            if (peers.Count == 0)
            {
                Debug.WriteLine("No paired devices were found.");
            }
            else
            {
                // Select a peer. In this example, let's just pick the first peer.
                //enumerate all peers
                var i = 0;
                foreach (var peer in peers)
                {
                    Debug.WriteLine("Device " + i++ + " / name: " + peer.DisplayName);
                }

                PeerInformation selectedPeer = peers[0];

                // Attempt a connection
                byte[] buf = new byte[2];
                buf[0] = 1;
                buf[1] = 2;
                using (var streamSocket = await PeerFinder.ConnectAsync(selectedPeer))
                {
                    var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                    var x = await streamSocket.OutputStream.WriteAsync(buffer);
                }
            }
        }

        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            if (ShouldConnect())
            {
                // Go ahead and connect
                ConnectToPeer(args.PeerInformation);
            }

        }

        async void ConnectToPeer(PeerInformation peer)
        {
            byte [] buf = new byte[2];
            var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
            StreamSocket socket = await PeerFinder.ConnectAsync(peer);
            using (var stream = socket.InputStream)
            {
                await stream.ReadAsync(buffer, 2, Windows.Storage.Streams.InputStreamOptions.None);
                foreach (var x in buffer.ToArray())
                {
                    Debug.WriteLine(x);
                }
            }

        }

        private bool ShouldConnect()
        {
            // Determine whether to accept this connection request and return
            return true;
        }
    }
}
