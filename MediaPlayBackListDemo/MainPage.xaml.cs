using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MediaPlayBackListDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlaybackItem playbackItem;
        MediaPlayer mediaplayer;
        MediaPlaybackList playbackList = new MediaPlaybackList();

        public MainPage()
        {
            this.InitializeComponent();
            InitializePlaybackList();
        }
         void InitializePlaybackList()
        {
            

            //Create the MediaPlayer:
            mediaplayer = new MediaPlayer();

            // Subscribe to MediaPlayer PlaybackState changed events
            mediaplayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            // Subscribe to MediaPlayer PlaybackRate changed events
            mediaplayer.PlaybackSession.PlaybackRateChanged += PlaybackSession_PlaybackRateChanged;

            // Subscribe to list UI changes
            playlistView.ItemClick += PlaylistView_ItemClick;

            //Attach the player to the MediaPlayerElement:
            mediaplayerElement.SetMediaPlayer(mediaplayer);

            // Set list for playback
            mediaplayer.Source = playbackList;


        }


        private async void PickMultiFile(object sender, RoutedEventArgs e)
        {

            await getMultiFile();
        }
        async private System.Threading.Tasks.Task getMultiFile()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mkv");

            //var file = await openPicker.PickSingleFileAsync();
            var pickedfiles = await openPicker.PickMultipleFilesAsync();
            if (pickedfiles.Count > 0)
            {
                int i = 1;
                foreach (Windows.Storage.StorageFile file in pickedfiles)
                {
                    i++;
                    MediaModel media = new MediaModel(file)
                    {
                        Title = file.Name,
                        ArtUri = new Uri($"ms-appx:///Assets/{i}.jpg")
                     };
                    playlistView.Items.Add(media);
                    playbackList.Items.Add(media.MediaPlaybackItem);
                }

                // Subscribe for changes
                playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;

            // Loop
            playbackList.AutoRepeatEnabled = true;
            }
                
                
                

            
        }

        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Synchronize our UI with the currently-playing item.
                playlistView.SelectedIndex = (int)sender.CurrentItemIndex;
            });
        }

        private void PlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MediaModel;
           

            // Start the background task if it wasn't running
            playbackList.MoveTo((uint)playbackList.Items.IndexOf(item.MediaPlaybackItem));

            if (MediaPlaybackState.Paused == mediaplayer.PlaybackSession.PlaybackState)
            {
                mediaplayer.Play();
            }
        }

        private void PlaybackSession_PlaybackRateChanged(MediaPlaybackSession sender, object args)
        {
            
        }

        async private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var currentState = sender.PlaybackState;

                // Update state label
                txtCurrentState.Text = currentState.ToString();

                // Update controls
                UpdateTransportControls(currentState);
            });

        }
        private void UpdateTransportControls(MediaPlaybackState state)
        {
            nextBtn.IsEnabled = true;
            previousBtn.IsEnabled = true;
            if (state == MediaPlaybackState.Playing)
            {
                playButtonSymbol.Symbol = Symbol.Pause;
            }
            else
            {
                playButtonSymbol.Symbol = Symbol.Play;
            }
        }




    }

    public class MediaModel
    {
        public MediaModel(StorageFile file)
        {
            MediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
        }

        public string Title { get; set; }
        public Uri ArtUri { get; set; }
        public MediaPlaybackItem MediaPlaybackItem { get; private set; }
    }
}
