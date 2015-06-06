using Newtonsoft.Json;
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
using Windows.UI.Core;
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
        Song CurrentSong;
        CoreDispatcher dispatcher;
        ObservableCollection<Song> SongCollection = new ObservableCollection<Song>();
        SenderData sd;
        public MediaPlayer()
        {
            this.InitializeComponent();
            sd = new SenderData();
            MyListView.ItemsSource = SongCollection;
            sd.ActionRequested += sd_ActionRequested;
            Player.MediaEnded -= next_song;
            Player.MediaEnded += next_song;

            ConnectionManager.Instance.OnMasterDataReceived += DataReceived;
            ConnectionManager.Instance.OnSlaveDataReceived += DataReceived;
            this.dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private void DataReceived(string data)
        {
            Command cmd = JsonConvert.DeserializeObject<Command>(data);
            if (cmd != null)
            {
                sd_ActionRequested(cmd);
            }
        }

        private async void sd_ActionRequested(Command cmd)
        {
            if (cmd != null)
            {
                switch (cmd.command)
                {
                    case CommandList.ADD:
                        {
                            SongData songData = cmd.SongData;

                            if (ConnectionManager.Instance.IsMaster)
                            {
                                if (songData.SongByteArray != null)
                                {
                                    SaveSong(songData.SongByteArray, songData.Name);
                                    await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                    {
                                        SongCollection.Add(new Song() { name = songData.Name });
                                    });
                                    sd.SendActionToServer(CommandList.LISTUPDATE, "", -1, null, SongCollection.ToList());
                                }
                            }
                        }
                        break;
                    case CommandList.REMOVE:
                        {
                            int index = cmd.songIndex;
                            if (ConnectionManager.Instance.IsMaster)
                            {
                                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                {
                                    SongCollection.RemoveAt(index);
                                });
                                sd.SendActionToServer(CommandList.LISTUPDATE, "", -1, null, SongCollection.ToList());
                            }
                        }
                        break;
                    case CommandList.NEXT:
                        {
                            int index = cmd.songIndex;
                            CurrentSong = SongCollection.ElementAt(index);
                            if (ConnectionManager.Instance.IsMaster)
                            {
                                PlayMusic(CurrentSong);
                            }
                        }
                        break;
                    case CommandList.PREVIOUS:
                        {
                            int index = cmd.songIndex;
                            CurrentSong = SongCollection.ElementAt(index);
                            if (ConnectionManager.Instance.IsMaster)
                            {
                                PlayMusic(CurrentSong);
                            }
                        }
                        break;
                    case CommandList.TOGGLEPLAYSTATE:
                        {
                            int index = cmd.songIndex;
                            if (index != -1)
                            {
                                CurrentSong = SongCollection.ElementAt(index);
                                if (ConnectionManager.Instance.IsMaster)
                                {
                                    PlayMusic(CurrentSong);
                                }
                            }
                            else
                            {
                                MediaElementState m = MediaElementState.Closed;
                                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                {
                                   m= Player.CurrentState;
                                });
                                if (m == MediaElementState.Playing)
                                {
                                    if (ConnectionManager.Instance.IsMaster)
                                    {
                                        await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                        {
                                            Player.Pause();
                                            PauseOrPlay.Content = "Play";
                                            PauseOrPlay.Click -= pauseSongs;
                                            PauseOrPlay.Click += playSongs;
                                        });
                                    }
                                    
                                }
                                else
                                {
                                    if (ConnectionManager.Instance.IsMaster)
                                    {
                                        await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                        {
                                            Player.Play();
                                            PauseOrPlay.Content = "Pause";
                                            PauseOrPlay.Click -= playSongs;
                                            PauseOrPlay.Click += pauseSongs;
                                        });
                                    }
                                    
                                }
                            }
                        }
                        break;
                    case CommandList.LISTUPDATE:
                        if (!ConnectionManager.Instance.IsMaster)
                        {
                            if (cmd.SongList == null || cmd.SongList.Count < 1) return;
                            await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                            {
                                SongCollection.Clear();
                                foreach (var item in cmd.SongList)
                                    SongCollection.Add(item);
                            });
                        }
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
            Song s = b.DataContext as Song;
            int index = SongCollection.IndexOf(s);
            SongCollection.Remove(s);
            sd.SendActionToServer(CommandList.REMOVE, s.name, index, null);

        }

        private void PlaySongFromList(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Song s = b.DataContext as Song;
            CurrentSong = s;

            PlayMusic(CurrentSong);

            sd.SendActionToServer(CommandList.TOGGLEPLAYSTATE, s.name, SongCollection.IndexOf(s), null);
        }

        private async void pauseSongs(object sender, RoutedEventArgs e)
        {
            if (ConnectionManager.Instance.IsMaster)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    Player.Pause();
                });
            }
            PauseOrPlay.Content = "Play";
            PauseOrPlay.Click -= pauseSongs;
            PauseOrPlay.Click += playSongs;
            sd.SendActionToServer(CommandList.TOGGLEPLAYSTATE, CurrentSong.name, -1, null);

        }

        private async void playSongs(object sender, RoutedEventArgs e)
        {
            if (ConnectionManager.Instance.IsMaster)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    Player.Play();
                });
            }
            PauseOrPlay.Content = "Pause";
            PauseOrPlay.Click -= playSongs;
            PauseOrPlay.Click += pauseSongs;
            sd.SendActionToServer(CommandList.TOGGLEPLAYSTATE, CurrentSong.name, -1, null);
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            IReadOnlyList<StorageFile> files = args.Files;
            if (files.Count > 0)
            {
                // Application now has read/write access to the picked file(s)
                foreach (StorageFile file in files)
                {
                    if (ConnectionManager.Instance.IsMaster)
                        SongCollection.Add(new Song() { name = file.Name });
                    if (ConnectionManager.Instance.IsMaster)
                    {
                        sd.SendActionToServer(CommandList.ADD, file.Name, -1, null);
                    }
                    else
                    {
                        Stream stream = await file.OpenStreamForReadAsync();
                        byte[] bytearray = new byte[(int)stream.Length];
                        bytearray = ConverToByteArray(stream);
                        sd.SendActionToServer(CommandList.ADD, file.Name, -1, bytearray);
                    }
                }
            }
            else
            {
            }
        }

        public async void PlayMusic(Song song)
        {
            var folder = KnownFolders.MusicLibrary;
            StorageFile file = await folder.GetFileAsync(song.name);
            Stream stream = await file.OpenStreamForReadAsync();

            if (ConnectionManager.Instance.IsMaster)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        Player.SetSource(stream.AsRandomAccessStream(), "audio/mpeg3");
                        Player.Play();
                    });
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

        private void next_song(object sender, RoutedEventArgs e)
        {
            int currentIndex = SongCollection.IndexOf(CurrentSong);
            if (currentIndex != SongCollection.Count - 1)
            {
                CurrentSong = SongCollection.ElementAt(++currentIndex);
                if (ConnectionManager.Instance.IsMaster)
                {
                    PlayMusic(CurrentSong);
                }
                sd.SendActionToServer(CommandList.NEXT, CurrentSong.name, currentIndex, null);
            }
        }

        private void prev_song(object sender, RoutedEventArgs e)
        {
            int currentIndex = SongCollection.IndexOf(CurrentSong);
            if (currentIndex != 0)
            {
                CurrentSong = SongCollection.ElementAt(--currentIndex);
                if (ConnectionManager.Instance.IsMaster)
                {
                    PlayMusic(CurrentSong);
                }
                sd.SendActionToServer(CommandList.PREVIOUS, CurrentSong.name, currentIndex, null);
            }
        }
    }
    public class Song
    {
        public string name { get; set; }
        public string path { get; set; }

    }
}
