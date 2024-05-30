using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MultimedijskiPredvajalnik
{
    public partial class MainWindow : Window
    {
        //ObservableCollection: update UI when items are changed (INotif in Playlist.cs)
        Playlist currentPlaylist = new Playlist("playlist");
        Playlist clonePlaylist = null; //for shuffle (shuffle off)

        public MainWindow()
        {
            InitializeComponent();
            PlaylistMenu.Visibility = Visibility.Collapsed;

            //placeholder items (remove)
            Multimedia media1 = new Multimedia(
                "‪C:/Users/nicks/Videos/song1.mp3", new TimeSpan(0, 1, 30), "Rock", "Album 1", "No artist",
                "‪C:/Users/nicks/Videos/song1.mp3");
            Multimedia media2 = new Multimedia(
                "‪C:/Users/nicks/Videos/song2.mp3", new TimeSpan(0, 1, 00), "Rock", "Album 1", "No artist",
                "‪C:/Users/nicks/OneDrive/Slike/image1.png");
            Multimedia media3 = new Multimedia(
                "‪C:/Users/nicks/Videos/video1.mp4", new TimeSpan(0, 10, 00), "Adventure", "Album 2", "Disney",
                "‪C:/Users/nicks/Videos/video1.mp4");
            currentPlaylist.PlaylistMultimedia.Add(media1);
            currentPlaylist.PlaylistMultimedia.Add(media2);
            currentPlaylist.PlaylistMultimedia.Add(media3);

            //listPlaylist.Add(newPlaylist);
            //PlaylistMenu.ItemsSource = listPlaylist;

            //ItemsSource : ELEMENT which is being listened for update (INotif)
            PlaylistMenu.ItemsSource = currentPlaylist.PlaylistMultimedia;
            //za iz shuffle -> unshuffle (ampak ne dela, ker se vedno vrednost nastavi na currentPlayist, tudi ko nocis)
            clonePlaylist = currentPlaylist; 

            PlaylistMenu.DataContext = Media.Source; //?

            //animation for background of Media
            MediaBackground.Loaded += new RoutedEventHandler(RectangleLoaded);

            //default postavitev
            ViewHoriz.IsChecked = true;
        }

        //postavitev
        bool viewH = true;
        bool viewV = false;

        //animation on window load
        private void RectangleLoaded(object sender, RoutedEventArgs e)
        {
            playBackgroundAnimation();
        }
        //play or stop animation Media Background
        public void playBackgroundAnimation()
        {
            if (Media.Source != null)
                StoryboardMediaBackground.Stop(this);
            else
                StoryboardMediaBackground.Begin(this, true);
        }

        //shuffle playlist: true-yes, false-no
        public void shufflePlaylist(bool way)
        {
            if (way)
            {
                Playlist shuffledPlaylist = currentPlaylist;
                //Playlist shuffledPlaylist = new Playlist(currentPlaylist.PlaylistName);

                //take playlist and shuffle indexes (deluje)
                Random rnd = new Random();
                int n = shuffledPlaylist.PlaylistMultimedia.Count;
                while (n > 1)
                {
                    n--;
                    int k = rnd.Next(n + 1);
                    var value = shuffledPlaylist.PlaylistMultimedia[k];
                    shuffledPlaylist.PlaylistMultimedia[k] = shuffledPlaylist.PlaylistMultimedia[n];
                    shuffledPlaylist.PlaylistMultimedia[n] = value;
                }

                PlaylistMenu.ItemsSource = shuffledPlaylist.PlaylistMultimedia;
            }
            else
            {
                PlaylistMenu.ItemsSource = clonePlaylist.PlaylistMultimedia; //!Playlist ne bo nikoli prvoten (problem z INotif/ItemSource?)
            }
        }

        //open xml file
        private void MenuItemImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDial = new OpenFileDialog();
            fileDial.Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*";
            if (fileDial.ShowDialog() == true)
            {
                //save into Multimedia class
                try
                {
                    using (StreamReader sr = new StreamReader(fileDial.FileName))
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(List<Multimedia>));
                        var item = (List<Multimedia>)xml.Deserialize(sr);
                        if (item != null)
                        {
                            currentPlaylist.PlaylistMultimedia.Clear();
                            currentPlaylist.PlaylistMultimedia = new TrulyObservableCollection<Multimedia>(item);
                            PlaylistMenu.ItemsSource = currentPlaylist.PlaylistMultimedia;
                        }
                    }
                }
                catch (Exception excep)
                {
                    MessageBox.Show(excep.ToString());
                }
            }

        }
        //save xml file
        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDial = new SaveFileDialog();
            fileDial.Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*";
            if (fileDial.ShowDialog() == true)
            {
                //save Multimedia class into xml
                using (StreamWriter sw = new StreamWriter(fileDial.FileName))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(List<Multimedia>));
                    xml.Serialize(sw, currentPlaylist.PlaylistMultimedia.ToList());
                }
            }
        }

        //close app
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //open settings window
        private void MenuItemToolsSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings newSettings = new Settings();
            this.Hide();
            if (!newSettings.IsActive)
            {
                newSettings.ShowDialog();
            }
            this.Show();
        }

        //open help window
        private void MenuItemToolsHelp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Multimedijski Predvajalnik\n" + 
                "Program created for FERI University project, II. year class \"Uporabniški vmesniki\".\n" +
                "Created in: 13.10.2022\n" + "Last update: 26.01.2023\n" + "Created by: Nick Srebot");
        }

        //enable edit button in menu if list has a selected index
        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (PlaylistMenu.SelectedItem != null)
                MenuItemEdit.IsEnabled = true;
            else
                MenuItemEdit.IsEnabled = false;
        }

        //add Media into playlist (file)
        private void MenuItemPlaylistAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDial = new OpenFileDialog();
            fileDial.Multiselect = true;
            fileDial.Filter = "Video files (*.mp4)|*.mp4|Audio files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (fileDial.ShowDialog() == true)
            {
                foreach (string fileName in fileDial.FileNames)
                {
                    currentPlaylist.PlaylistMultimedia.Add(new Multimedia(
                 fileName, new TimeSpan(0, 1, 30), "Rock", "Album 1", "No artist",
                 fileName));

                    //da klice IsPlaying v razredu Multimedia (oznaci Currently Playing - nepopolno pri vecih add)
                    Uri file = new Uri(fileName);
                    Media.Source = file;
                    Media.Pause();
                    clonePlaylist = currentPlaylist;
                }
            }
        }

        //edit Media item in playlist
        private void MenuItemPlaylistEdit_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistMenu.SelectedItem != null)
            {
                MultimedWindow newMultimed = new MultimedWindow(PlaylistMenu.SelectedItem as Multimedia);
                this.Hide();
                if (!newMultimed.IsActive)
                {
                    newMultimed.ShowDialog();
                }
                this.Show();
            }
            else
            {
                MessageBox.Show("No element selected!");
            }
        }

        //remove Media item in plylist
        private void MenuItemPlaylistRemove_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistMenu.SelectedItem != null)
            {
                currentPlaylist.PlaylistMultimedia.Remove(PlaylistMenu.SelectedItem as Multimedia);
                clonePlaylist = currentPlaylist;
            }
            else
            {
                MessageBox.Show("No element selected!");
            }
        }

        //play selected Media file in playlist
        private void PlaylistMenu_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistMenu.SelectedIndex >= 0)
            {
                var selectedItem = currentPlaylist.PlaylistMultimedia[PlaylistMenu.SelectedIndex];
                MessageBox.Show(selectedItem.Title.ToString());
                try
                {
                    Uri file = new Uri(selectedItem.Path);
                    Media.Source = file;

                    //for IsPlaying in Multimedia (ne deluje)
                    //selectedItem.Path = "";
                    //selectedItem.Path = file.OriginalString;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        //show or hide playlist
        private void MenuItemShowPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistMenu.Visibility == Visibility.Collapsed)
            {
                PlaylistMenu.Visibility = Visibility.Visible;
            }
            else
            {
                PlaylistMenu.Visibility = Visibility.Collapsed;
            }
        }

        //when new media in Multimedia is opened
        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            //set lables and gradniki in UserControl PlayerControls
            PlayerControls.LabelEnd.Content = Media.NaturalDuration.TimeSpan.ToString();
            PlayerControls.barVolume.Value = Media.Volume;
            playBackgroundAnimation();
        }

        //when media in Multimedia reaches end
        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("Media ended");
            PlayerControls.mediaEnded();
        }

        //volume control with mousewheel
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Media.Volume += (e.Delta > 0) ? 0.1 : -0.1;
            PlayerControls.barVolume.Value = Media.Volume;
        }

        //change between horizontal and vertical views (menuItem)
        private void ApperanceBothViews_Click(object sender, RoutedEventArgs e)
        {
            if (viewH)
            {
                ViewHoriz.IsChecked = false;
                viewH = false;
                ViewVert.IsChecked = true;
                viewV = true;
                verticalView();
            }
            else if(viewV)
            {
                ViewHoriz.IsChecked = true;
                viewH = true;
                ViewVert.IsChecked = false;
                viewV = false;
                horizontalView();
            }
        }

        //set main window in vertical view
        private void verticalView()
        {
            //video - far right
            Grid.SetColumn(Media, 2);
            Grid.SetRowSpan(Media, 3);
            Grid.SetRow(Media, 1);
            Grid.SetColumnSpan(Media,1);

            //video background - same as video
            Grid.SetColumn(MediaBackground, 2);
            Grid.SetRowSpan(MediaBackground, 3);
            Grid.SetRow(MediaBackground, 1);
            Grid.SetColumnSpan(MediaBackground, 1);

            //playlist - far left
            Grid.SetColumn(PlaylistMenu, 0);
            Grid.SetRowSpan(PlaylistMenu, 3);
            Grid.SetRow(PlaylistMenu, 1);
            Grid.SetColumnSpan(PlaylistMenu, 1);

            //grid splitter - vertical middle
            Grid.SetColumn(Splitter, 1);
            Grid.SetRowSpan(Splitter, 3);
            Grid.SetRow(Splitter, 1);
            Grid.SetColumnSpan(Splitter, 1);
            //set style (direction) for gridSplitter
            var ss = this.FindResource("styleGridSplitterVert");
            Splitter.Style = ss as Style;

        }
        //set main window in horizontal view
        private void horizontalView()
        {
            //video - bottom 
            Grid.SetRow(Media, 3);
            Grid.SetColumnSpan(Media, 3);
            Grid.SetColumn(Media, 0);
            Grid.SetRowSpan(Media, 1);

            //video background - same as video
            Grid.SetRow(MediaBackground, 3);
            Grid.SetColumnSpan(MediaBackground, 3);
            Grid.SetColumn(MediaBackground, 0);
            Grid.SetRowSpan(MediaBackground, 1);

            //playlist - top
            Grid.SetRow(PlaylistMenu, 1);
            Grid.SetColumnSpan(PlaylistMenu, 3);
            Grid.SetColumn(PlaylistMenu, 0);
            Grid.SetRowSpan(PlaylistMenu, 1);

            //grid splitter - horizontal middle
            Grid.SetRow(Splitter, 2);
            Grid.SetColumnSpan(Splitter, 3);
            Grid.SetColumn(Splitter, 0);
            Grid.SetRowSpan(Splitter, 1);
            //set style (direction) for gridSplitter
            var ss = this.FindResource("styleGridSplitterHoriz");
            Splitter.Style = ss as Style;

            //da bi spremenju stackpanel orientation, ne deluje ker je vgnezdjen
            /*
            //najdi PlaylistItem element od objekta PlaylistMenu
            object stackpanelEl = PlaylistMenu.FindName("PlaylistItem");
            var ps = this.FindResource("styleStackpanelHoriz");
            if (stackpanelEl is StackPanel)
            {
                //nastavi style za PlaylistItem
                StackPanel playlistItem = stackpanelEl as StackPanel;
                playlistItem.Style = ps as Style;
            }
            */
        }
    }
}
