using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class MediaPlayer : Page
    {
        ObservableCollection<Song> SongCollection = new ObservableCollection<Song>();
        public MediaPlayer()
        {
            this.InitializeComponent();
            MyListView.ItemsSource = SongCollection;
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
            SongCollection.Add(new Song() {name="Kuch Kuch hota hai" });
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
    }
    public class Song
    {
        public string name { get;set; }
        public string path { get; set; }
                            
    }
}
