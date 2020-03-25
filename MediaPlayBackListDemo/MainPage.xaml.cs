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
            Debug.WriteLine($"Current playback state changed to: {sender.PlaybackState}");

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
            Debug.WriteLine($"Enabling Prev/Next Buttons");
            nextBtn.IsEnabled = true;
            previousBtn.IsEnabled = true;
            if (state == MediaPlaybackState.Playing)
            {
                playButtonSymbol.Symbol = Symbol.Pause;
                Debug.WriteLine($"Media playing");
            }
            else
            {
                playButtonSymbol.Symbol = Symbol.Play;
                Debug.WriteLine($"Media paused");
            }
        }
        
        private void skipBackBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaplayer.PlaybackSession.Position -= TimeSpan.FromSeconds(10);
        }

        private void rewindBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaplayer.PlaybackSession.PlaybackRate <= 1 && mediaplayer.PlaybackSession.PlaybackRate >= 0.1)
                mediaplayer.PlaybackSession.PlaybackRate -= 0.1;
            else mediaplayer.PlaybackSession.PlaybackRate -= 0.5;
            Debug.WriteLine($"PlaybackRate : {mediaplayer.PlaybackSession.PlaybackRate}x");
        }

        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            playbackList.MovePrevious();
        }

        private void playpauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlaybackState.Playing == mediaplayer.PlaybackSession.PlaybackState)
            {
                mediaplayer.Pause();
            }
            else if (MediaPlaybackState.Paused == mediaplayer.PlaybackSession.PlaybackState)
            {
                mediaplayer.Play();
            }
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            playbackList.MoveNext();
        }

        private void fastForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaplayer.PlaybackSession.PlaybackRate += 0.5;
            Debug.WriteLine($"PlaybackRate : {mediaplayer.PlaybackSession.PlaybackRate}x");
        }

        private void skipForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaplayer.PlaybackSession.Position += TimeSpan.FromSeconds(30);
        }

        async private void shuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            playbackList.ShuffleEnabled = !playbackList.ShuffleEnabled;
            Debug.WriteLine($"shuffle : {playbackList.ShuffleEnabled}");
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                shuffleBtn.FontWeight =
                    playbackList.ShuffleEnabled ? Windows.UI.Text.FontWeights.Bold : Windows.UI.Text.FontWeights.Light;
            });
        }

       async  private void AutoRepeatBtn_Click(object sender, RoutedEventArgs e)
        {
            playbackList.AutoRepeatEnabled = !playbackList.AutoRepeatEnabled;
            Debug.WriteLine($"AutoRepeat : {playbackList.AutoRepeatEnabled}");
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                autoRepeatBtn.FontWeight =
                    playbackList.AutoRepeatEnabled ? Windows.UI.Text.FontWeights.Bold : Windows.UI.Text.FontWeights.Light;
            });
        }

        private void aspectRatioBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (mediaplayerElement.Stretch)
            {
                case Stretch.Fill:
                    mediaplayerElement.Stretch = Stretch.None;
                    break;
                case Stretch.None:
                    mediaplayerElement.Stretch = Stretch.Uniform;
                    break;
                case Stretch.Uniform:
                    mediaplayerElement.Stretch = Stretch.UniformToFill;
                    break;
                case Stretch.UniformToFill:
                    mediaplayerElement.Stretch = Stretch.Fill;
                    break;
                default:
                    break;
            }
        }

        private void fullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaplayerElement.IsFullWindow = !mediaplayerElement.IsFullWindow;
        }

        //skipBackBtn rewindBtn previousBtn playpauseBtn nextBtn fastForwardBtn skipForwardBtn



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
