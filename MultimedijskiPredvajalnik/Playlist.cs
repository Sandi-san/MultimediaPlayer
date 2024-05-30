using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultimedijskiPredvajalnik
{
    internal class Playlist : INotifyPropertyChanged
    {
        public string PlaylistName { get; set; }
        //ObservableCollection : update UI when list of objects is changed
        private TrulyObservableCollection<Multimedia> _playlistmultimedia; //observableCollection + additional funkcionalosti (iz netta)
        public TrulyObservableCollection<Multimedia> PlaylistMultimedia
        {
            get { return _playlistmultimedia; }
            set
            {
                if (_playlistmultimedia == value) return;
                _playlistmultimedia = value;
                OnPropertyChanged();
            }
        }

        public Playlist(string name)
        {
            PlaylistName = name;
            PlaylistMultimedia = new TrulyObservableCollection<Multimedia>();
        }

        //INotifyPropertyChanged - refresh UI when Playlist is updated (check using & : )
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
