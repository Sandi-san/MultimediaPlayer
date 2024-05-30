using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;

namespace MultimedijskiPredvajalnik
{
    public class Multimedia
    {
        private string _path;
        private TimeSpan _duration;
        private string _genre;
        private string _album;
        private string _artist;
        private string _image;
        private string _title;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }
        //ne shranit TimeSpan, ker se shranjuje posebej
        [XmlIgnore]
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        //za shranjevanje TimeSpan v xml datoteko, serializacija
        [Browsable(false)]
        [XmlElement(DataType="duration", ElementName ="Duration")]
        public string DurationString
        {
            get { return XmlConvert.ToString(Duration); }
            set
            {
                Duration = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }

        public string Genre
        {
            get { return _genre; }
            set
            {
                _genre = value;
                OnPropertyChanged();
            }
        }
        public string Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnPropertyChanged();
            }
        }
        public string Artist
        {
            get { return _artist; }
            set
            {
                _artist = value;
                OnPropertyChanged();
            }
        }
        public string Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public Multimedia(string path, TimeSpan duration, string genre, string album, string artist, string image)
        {
            Path = path;
            Duration = duration;
            Genre = genre;
            Album = album;
            Artist = artist;
            Image = image;
            Title = TitleFromPath(path);
        }

        internal Multimedia()
        {
            Path = "";
            Duration = new TimeSpan();
            Genre = "";
            Album = "";
            Artist = "";
            Image = "";
            Title = Path;
        }

        private string TitleFromPath(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        //spremeni barvo gradnika v main playlist ko se doda nov (nepopolno, mogu bi bit tisti, ki se trenutno playa)
        public bool IsPlayingMedia
        {
            get
            {
                //var thisMedia;
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType()==typeof(MainWindow))
                    {
                        if ((window as MainWindow).Media.Source!=null)
                        {
                            return Path == (window as MainWindow).Media.Source.OriginalString;
                        }
                    }
                }
                return false;
            }
        }
        
        //ko se katerikoli od spremenljivk spremeni (add-a) klici propertyChanged - osvezi v UI
        //INotifyPropertyChanged - refresh UI when Playlist is updated (check using & : )
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
