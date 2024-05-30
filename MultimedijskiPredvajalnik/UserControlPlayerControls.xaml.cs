using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MultimedijskiPredvajalnik
{
    //controls for Media/video playback
    public partial class UserControlPlayerControls : UserControl
    {
        MediaElement media = null; //main MediaElement in MainWindow
        string mediaState = null; //current state of media
        ListView playlist = null; //main ListView in MainWindow
        Window parent = null; //MainWindow

        //slider
        bool isUserDraggingSlider = false;
        DispatcherTimer timerSlider = new DispatcherTimer();
        bool hasMediaEnded = false;

        //buttons
        private ImageSource _playImageSource; //icon for playButton
        bool isRepeating = false; //repeatButton
        bool isShuffling = false; //shuffleButton
        private ImageSource _repeatImageSource; //icon for repeatButton
        private ImageSource _shuffleImageSource; //icon for shuffleButton

        public UserControlPlayerControls()
        {
            InitializeComponent();
            this.DataContext = this;

            //init timer for slider
            timerSlider.Interval = TimeSpan.FromSeconds(1);
            timerSlider.Tick += TimerSlider_Tick;
            timerSlider.Start();

            //set default images for buttons play, repeat and shuffle (which can change images during runtime)
            ButtonPlay.DataContext = ButtonPlayImage.Source;
            ButtonPlayImage.Source = new BitmapImage(new Uri(@"/Resources/icon_play.png", UriKind.Relative));
            ButtonRepeatImage.Source = new BitmapImage(new Uri(@"/Resources/icon_repeat.png", UriKind.Relative));
            ButtonShuffleImage.Source = new BitmapImage(new Uri(@"/Resources/icon_shuffle.png", UriKind.Relative));
        }

        //imageSource classes for images of buttons that change (note: OnPropertyChanged for UI update)
        public ImageSource playImageSource
        {
            get { return _playImageSource; }
            set
            {
                _playImageSource = value;
                OnPropertyChanged();
            }
        }
        public ImageSource repeatImageSource
        {
            get { return _repeatImageSource; }
            set
            {
                _repeatImageSource = value;
                OnPropertyChanged();
            }
        }
        public ImageSource shuffleImageSource
        {
            get { return _shuffleImageSource; }
            set
            {
                _shuffleImageSource = value;
                OnPropertyChanged();
            }
        }

        //on each tick, for slider
        private void TimerSlider_Tick(object sender, EventArgs e)
        {
            //if Media is playing & has valid runtime & user isnt dragging the slider
            if (media.Source != null && media.NaturalDuration.HasTimeSpan && !isUserDraggingSlider)
            {
                //set Slider variables
                MediaSlider.Minimum = 0; //min value is 0
                MediaSlider.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds; //max value is total time of video
                MediaSlider.Value = media.Position.TotalSeconds; //set slider position at current time of video
            }
            //if user is dragging slider
            else if (isUserDraggingSlider)
            {
                double seconds = MediaSlider.Value; //where is slider currently
                media.Position = new TimeSpan(0,0,Convert.ToInt32(seconds)); //set video time at current value of slider
            }
        }

        //pridobi stanje Media elementa (za preverjanje prevajanja)
        private MediaState GetMediaState(MediaElement med)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helper = hlp.GetValue(med);
            FieldInfo state = helper.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            return (MediaState)state.GetValue(helper);
        }

        //media has ended
        public void mediaEnded()
        {
            //if buttonRepeat is not selected
            if (!isRepeating)
            {
                ButtonPlayImage.Source = new BitmapImage(new Uri(@"/Resources/icon_play.png", UriKind.Relative)); //change buttonPlay image to play symbol
                media.Pause(); //pause Media
                hasMediaEnded = true; //media has ended
            }
            else
            {
                resetMediaToStart(); //reset values to start
                media.Play(); //play Media
            }
        }

        //set media and slider values to start of video
        public void resetMediaToStart()
        {
            MediaSlider.Value = 0; //set slider to start
            media.Position = new TimeSpan(0); //set media to start
            hasMediaEnded = false; //media has not ended
        }

        //buttonPlay is clicked
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            //is Media opened
            if (media.Source!=null)
            {
                //System.Windows.Forms.MessageBox.Show("Media: " + GetMediaState(media).ToString() + "\nSource: " + media.Source.ToString());

                //if media has ended, reset to start
                if (hasMediaEnded)
                {
                    resetMediaToStart();
                }

                //get in which state media is currently
                mediaState = GetMediaState(media).ToString();
                switch (mediaState)
                {
                    //if closed, play media and set buttonPlay to pause symbol
                    case "Close":
                        media.Play();
                        ButtonPlayImage.Source = new BitmapImage(new Uri(@"/Resources/icon_pause.png", UriKind.Relative));
                        break;
                    //if playing, check if media is still playing or has reached end (counts as still playing)
                    case "Play":
                        if (MediaSlider.Value == media.NaturalDuration.TimeSpan.TotalSeconds) //media reached end
                        {
                            //set buttonPlay icon to pause symbol and play media
                            ButtonPlayImage.Source = new BitmapImage(new Uri(@"/Resources/icon_pause.png", UriKind.Relative));
                            media.Play();
                        }
                        else
                        {
                            //set buttonPlay to play symbol and pause media
                            ButtonPlayImage.Source = new BitmapImage(new Uri(@"/Resources/icon_play.png", UriKind.Relative));
                            media.Pause();
                        }
                        break;
                    //if paused, play media and set buttonPlay to pause symbol
                    case "Pause":
                        ButtonPlayImage.Source = new BitmapImage(new Uri(@"/Resources/icon_pause.png", UriKind.Relative));
                        media.Play();
                        break;
                }
            }
        }

        //buttonStop is clicked
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            //is Media opened
            if (media.Source!=null)
            {
                //get current state of media, if it is not in closed state:
                mediaState = GetMediaState(media).ToString();
                if (mediaState != "Close")
                {
                    media.Close(); //set media state to close
                    media.Source = null; //remove current source (video) from media
                    LabelEnd.Content = "00:00:00"; //reset end label
                    MediaSlider.Value = 0.0; //set slider to start
                    if (parent != null)
                    {
                        //klici metodo iz parent
                        var method = parent as MainWindow;
                        method.playBackgroundAnimation();
                    }
                }
            }
        }

        //buttonPrevious is clicked, play video which appears before currently playing video in playlist
        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (media.Source!=null)
            {
                //create placeholder playlist to access values from main playlist
                Playlist temp = new Playlist("placeholder");
                foreach (Multimedia item in playlist.Items) //elementi v PlaylistMenu lokalni objekt (za PlaylistMultimedia)
                {
                    temp.PlaylistMultimedia.Add(item);
                }

                foreach (Multimedia item in temp.PlaylistMultimedia)
                {
                    if (item.Path == media.Source.OriginalString) //najdi trenutno predvajan element
                    {
                        int index = temp.PlaylistMultimedia.IndexOf(item); //index od trenutno predvajanjega elementa (ListView -> Playlist)
                        try
                        {
                            //predvajaj multimedijo, ki je za 1 indeks manj od trenutno predvajanje
                            Multimedia prevMedia = temp.PlaylistMultimedia[index - 1];
                            Uri file = new Uri(prevMedia.Path);
                            media.Source = file;
                            break;
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.ToString());
                        }
                    }
                }
            }
        }

        //buttonNext is clicked, play video which appears after currently playing video in playlist
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (media.Source != null)
            {
                //create placeholder playlist to access values from main playlist
                Playlist temp = new Playlist("placeholder");
                foreach (Multimedia item in playlist.Items)
                {
                    temp.PlaylistMultimedia.Add(item);
                }

                foreach (Multimedia item in temp.PlaylistMultimedia)
                {
                    if (item.Path == media.Source.OriginalString)
                    {
                        int index = temp.PlaylistMultimedia.IndexOf(item);
                        try
                        {
                            //predvajaj multimedijo, ki je za 1 indeks vec od trenutno predvajanje
                            Multimedia prevMedia = temp.PlaylistMultimedia[index + 1];
                            Uri file = new Uri(prevMedia.Path);
                            media.Source = file;
                            break;
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.ToString());
                        }
                    }
                }
            }
        }

        //Pridobi MediaElement Media iz glavnega okna (parent)
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //PRIDOBI MEDIA ELEMENT in PLAYLIST
            parent = Window.GetWindow(this.Parent);  //dobi okno
            //System.Windows.Forms.MessageBox.Show(parent.ToString());
            if (parent!=null)   //brez tega se MainWindow.xaml podre
            {
                object mediaEl = parent.FindName("Media");  //dobi MediaElement z imenom Media
                object listviewEl = parent.FindName("PlaylistMenu");

                if (mediaEl is MediaElement)
                {
                    media = mediaEl as MediaElement; //shrani MediaElement v glavno spremenljivko
                }
                if (listviewEl is ListView)
                {
                    playlist = listviewEl as ListView;
                }
            }

            barVolume.Value = media.Volume;
        }

        //is user draggin the slider
        private void MediaSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isUserDraggingSlider = true;
        }

        //user no longer dragging slider
        private void MediaSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isUserDraggingSlider = false;
            media.Position = TimeSpan.FromSeconds(MediaSlider.Value); //update position of video depending on slider value
        }

        //value of slider has changed
        private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelCurrent.Content = TimeSpan.FromSeconds(MediaSlider.Value).ToString(); //update number of seconds video has elapsed
        }

        //buttonRepeat clicked
        private void ButtonRepeat_Click(object sender, RoutedEventArgs e)
        {
            //is repeating off
            if (isRepeating == false)
            {
                //change image to active and set repeating to true
                isRepeating = true;
                ButtonRepeatImage.Source = new BitmapImage(new Uri(@"/Resources/icon_repeatActive.png", UriKind.Relative));
            }
            //is repeating on
            else
            {
                //change image to not active and set repeating to falsee
                isRepeating = false;
                ButtonRepeatImage.Source = new BitmapImage(new Uri(@"/Resources/icon_repeat.png", UriKind.Relative));
            }
        }

        //buttonShuffle
        private void ButtonShuffle_Click(object sender, RoutedEventArgs e)
        {
            //is shuffling off
            if (isShuffling == false)
            {
                //change image to active and set shuffling to true
                isShuffling = true;
                ButtonShuffleImage.Source = new BitmapImage(new Uri(@"/Resources/icon_shuffleActive.png", UriKind.Relative));

                //call method shufflePlaylist in main window
                if (parent != null)
                {
                    var method = parent as MainWindow;
                    method.shufflePlaylist(true);
                }
            }
            //is shuffling off
            else
            {
                //change image to not active and set repeating to falsee
                isShuffling = false;
                ButtonShuffleImage.Source = new BitmapImage(new Uri(@"/Resources/icon_shuffle.png", UriKind.Relative));

                //call method shufflePlaylist in main window
                if (parent != null)
                {
                    var method = parent as MainWindow;
                    method.shufflePlaylist(false);
                }
            }
        }

        //INotifyPropertyChanged - refresh UI when Playlist is updated (check using & : )
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
