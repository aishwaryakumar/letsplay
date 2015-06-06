using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Hackday
{
    public class ConnectionManager
    {
        public delegate void OnData(string data);
        public event OnData OnMasterDataReceived;
        public event OnData OnSlaveDataReceived;

        private DataListener listener;
        public bool IsMaster = false;
        private Dictionary<string, StreamSocket> slaves = new Dictionary<string, StreamSocket>();
        StreamSocket master;
        private static ConnectionManager _instance;
        private ConnectionManager()
        {
        }

        public void SetDataListener(DataListener listener)
        {
            this.listener = listener;
        }

        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConnectionManager();
                }
                return _instance;
            }
        }

        public async void Init(CoreDispatcher disp)
        {
            this.dispatcher = disp;
            await Task.Yield();
            // PeerFinder.Start() is used to advertise our presence so that peers can find us. 
            // It must always be called before FindAllPeersAsync.
            PeerFinder.Start();
            PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;
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
                //appendText(e.Message);
            }

        }

        async void ConnectToPeer(PeerInformation peer)
        {
            try
            {
                appendText("\nconnection requested");
                //byte[] buf = new byte[2];
                //var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                StreamSocket socket = await PeerFinder.ConnectAsync(peer);
                //slaves.Add(peer.Id, socket);
                IsMaster = false;
                PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;
                master = socket;
                appendText("\n connection complete");

                GetDataFromMaster();
                //await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                //{
                //    var dispatcherTimer = new DispatcherTimer();
                //    dispatcherTimer.Tick += new EventHandler<object>(getDataFromMaster);
                //    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                //    dispatcherTimer.Start();

                //});

                //appendText("\nconnection established");
                //using (var stream = socket.InputStream)
                //{
                //    await stream.ReadAsync(buffer, 2, Windows.Storage.Streams.InputStreamOptions.None);
                //    appendText("\n data read \n");
                //    foreach (var x in buffer.ToArray())
                //    {
                //        appendText("\n" + x);
                //    }
                //}
            }
            catch (Exception e)
            {
                //appendText(e.Message);
            }
        }
        const int CONTROL_SIZE = 10;
        private CoreDispatcher dispatcher;
        bool isReadingMaster = false;
        private async void GetDataFromMaster()
        {
            try
            {
                string inp = "";
                if (isReadingMaster)
                    return;
                if (master != null)
                {
                    bool received = false;
                    do
                    {
                        received = false;

                        var len = await ReadData(master, CONTROL_SIZE);
                        uint length = 0;
                        if (uint.TryParse(len, out length))
                        {
                            isReadingMaster = true;
                            if (length > 0)
                            {
                                inp = await ReadData(master, length);
                                if (listener != null)
                                {
                                    listener.OnDataFromMaster(inp);
                                }
                                if (OnMasterDataReceived != null)
                                {
                                    OnMasterDataReceived(inp);
                                }
                                received = true;
                            }
                        }
                    }
                    while (true);
                    isReadingMaster = false;
                    GetDataFromMaster();
                }
            }
            catch (Exception ex)
            {
                isReadingMaster = false;
                GetDataFromMaster();
                //appendText(ex.Message);
            }
        }


        private bool ShouldConnect()
        {
            // Determine whether to accept this connection request and return
            return true;
        }

        public async Task<string> StartPairing()
        {
            var peers = await PeerFinder.FindAllPeersAsync();

            if (peers.Count == 0)
            {
                return appendText("No paired devices were found.\n");
            }
            else
            {
                string names = "";
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
                        slaves.Add(selectedPeer.Id, socket);
                        names += selectedPeer.ServiceName + "\n";
                    }
                    catch (Exception ex)
                    {
                        //return appendText(ex.Message);
                    }
                }
                isReadingFromSlave = new List<bool>(slaves.Count);

                PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;
                IsMaster = true;
                GetDataFromSlaves();
                //var dispatcherTimer = new DispatcherTimer();
                //dispatcherTimer.Tick += new EventHandler<object>(getDataFromSlaves);
                //dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                //dispatcherTimer.Start();
                return names;
                //var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                //foreach (var s in sockets)
                //{
                //    var x = await s.OutputStream.WriteAsync(buffer);
                //}

                //using (var streamSocket = await PeerFinder.ConnectAsync(selectedPeer))
                //{
                //    var buffer = WindowsRuntimeBufferExtensions.AsBuffer(buf);
                //    var x = await streamSocket.OutputStream.WriteAsync(buffer);
                //}
            }
        }
        List<bool> isReadingFromSlave;

        private void GetDataFromSlaves()
        {
            try
            {
                if (slaves != null && slaves.Count > 0)
                {
                    foreach (var pair in slaves)
                    {
                        var sock = pair.Value;
                        GetDataFromASlave(sock);
                    }
                }
            }
            catch (Exception ex)
            {
                //appendText(ex.Message);
            }
        }

        private void GetDataFromASlave(StreamSocket sock)
        {
            Task.Factory.StartNew(async () =>
            {
                bool received = false;

                string inp = "";
                do
                {
                    received = false;
                    var len = await ReadData(sock, CONTROL_SIZE);
                    uint length = 0;
                    if (uint.TryParse(len, out length))
                    {
                        if (length > 0)
                        {
                            inp = await ReadData(sock, length);
                            if (listener != null)
                            {
                                listener.OnDataFromSlaves(inp);
                            }
                            if (OnSlaveDataReceived != null)
                            {
                                OnSlaveDataReceived(inp);
                            }
                            received = true;
                        }
                    }
                }
                while (true);
            });
        }

        private async Task<string> ReadData(StreamSocket sock, uint numchars)
        {
            try
            {
                uint size = numchars * 2;
                byte[] buf = new byte[size];
                if (size >= 1024)
                {
                    uint bytesDone = 0;
                    uint bytesLeft = size;
                    uint toread = 1024;
                    while (bytesLeft > 0)
                    {
                        byte[] temp = new byte[toread];
                        var buffer = temp.AsBuffer();
                        await sock.InputStream.ReadAsync(buffer, toread, InputStreamOptions.None);
                        buffer.CopyTo(0, buf, (int)bytesDone, (int)buffer.Length);
                        bytesDone += buffer.Length;
                        bytesLeft -= buffer.Length;
                        toread = bytesLeft > 1024 ? 1024 : bytesLeft;
                        //var data = buffer.ToArray();
                        //CopyTo(data, buf, bytesDone);
                    }
                    return GetString(buf);
                }
                else
                {
                    var buffer = buf.AsBuffer();
                    await sock.InputStream.ReadAsync(buffer, size, InputStreamOptions.None);
                    return GetString(buffer.ToArray());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return "";
        }

        void CopyTo(byte[] source, byte[] dest, uint startIndex)
        {
            for (int i = 0; i < source.Length; i++)
            {
                dest[startIndex + i] = source[i];
            }
        }

        public async void SendData(StreamSocket socket, byte[] data)
        {
            //if (slaves.Count > 0)
            //{
            //    var buffer = WindowsRuntimeBufferExtensions.AsBuffer(data);
            //    foreach (var pair in slaves)
            //    {
            //        var name = pair.Key;
            //        var sock = pair.Value;
            //        await sock.OutputStream.WriteAsync(buffer);
            //    }
            //}
            var buffer = WindowsRuntimeBufferExtensions.AsBuffer(data);
            await socket.OutputStream.WriteAsync(buffer);
            await socket.OutputStream.FlushAsync();
        }

        public void SendData(string data)
        {
            //SendData(GetBytes(data));
            if (IsMaster)
            {
                if (slaves.Count > 0)
                {
                    //var buffer = WindowsRuntimeBufferExtensions.AsBuffer(data);
                    foreach (var pair in slaves)
                    {
                        var name = pair.Key;
                        var sock = pair.Value;
                        writedata(sock, data);
                    }
                }
            }
            else
            {
                writedata(master, data);
            }
        }

        private void writedata(StreamSocket socket, string data)
        {
            SendData(socket, GetBytes(data));
            //DataWriter writer = new DataWriter(socket.OutputStream);
            Debug.WriteLine(data.Substring(0, 10));
            //writer.WriteString(data);
            //await writer.StoreAsync();
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private string appendText(string text)
        {
            Debug.WriteLine(text);
            if (OnSlaveDataReceived != null)
            {
                OnSlaveDataReceived(text);
            }
            if (OnMasterDataReceived != null)
            {
                OnMasterDataReceived(text);
            }
            return text;
        }

    }
}
