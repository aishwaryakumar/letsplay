using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Hackday
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaPlayer : Page, IFileOpenPickerContinuable
    {
        ObservableCollection<Song> SongCollection = new ObservableCollection<Song>();
        SenderData sd;
        public MediaPlayer()
        {
            this.InitializeComponent();
            sd = new SenderData();
            MyListView.ItemsSource = SongCollection;
            sd.ActionRequested += sd_ActionRequested;
        }

        private void sd_ActionRequested(Command cmd)
        {
            if (cmd != null)
            {
                switch (cmd.command)
                {
                    case CommandList.ADD:
                        {
                            SongData songData = cmd.SongData;
                            if(songData.SongByteArray != null)
                            {
                                SaveSong(songData.SongByteArray, songData.Name);
                            }
                            SongCollection.Add(new Song() { name = songData.Name });
                        }
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
        }

        private void add_songs(object sender, RoutedEventArgs e)
        {            
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            openPicker.FileTypeFilter.Add(".mp3");
            // openPicker.CommitButtonText = "send";
            openPicker.PickMultipleFilesAndContinue();
        }

        private void RemoveSong(object sender, TappedRoutedEventArgs e)
        {
            Button b = sender as Button;
            Song s= b.DataContext as Song;
            SongCollection.Remove(s);

        }

        private async void PlaySong(object sender, TappedRoutedEventArgs e)
        {
            Button b = sender as Button;
            Song s = b.DataContext as Song;
            string n = s.name;
            var folder = KnownFolders.MusicLibrary;
            StorageFile file =await  folder.GetFileAsync(n);
            Stream stream = await file.OpenStreamForReadAsync();
            Player.SetSource(stream.AsRandomAccessStream(), "audio/mpeg3");
            Player.Play();
        }

        private void pauseSongs(object sender, RoutedEventArgs e)
        {
            Player.Pause();
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            IReadOnlyList<StorageFile> files = args.Files;
            if (files.Count > 0)
            {
                // Application now has read/write access to the picked file(s)
                foreach (StorageFile file in files)
                {
                    SongCollection.Add(new Song() { name = file.Name });
                    if(ConnectionManager.Instance.IsMaster)
                    {
                        sd.SendActionToServer(CommandList.ADD, file.Name, -1, null);
                    }
                    else
                    {
                        Stream stream = await file.OpenStreamForReadAsync();
                        byte[] bytearray = new byte[(int)stream.Length];
                        bytearray = ConverToByteArray(stream);
                        sd.SendActionToServer(CommandList.ADD,file.Name, -1, bytearray);
                    }
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

        public async void SaveSong(byte[] songArray, string songName)
        {
            try
            {
                var musicFolder = KnownFolders.MusicLibrary;
                StorageFile sampleFile = await musicFolder.CreateFileAsync(songName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(sampleFile, songArray);
            }
            catch (Exception ex)
            {

            }
        }
    }
    public class Song
    {
        public string name { get;set; }
        public string path { get; set; }
                            
    }
}
