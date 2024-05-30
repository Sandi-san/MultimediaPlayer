using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MultimedijskiPredvajalnik
{
    public partial class MultimedWindow : Window
    {
        public MultimedWindow(Multimedia editMedia) //get current Multimedia item/class
        {
            InitializeComponent();
            fillBoxes(editMedia);
        }

        //fill textboxes with values from Multimedia  item
        private void fillBoxes(Multimedia editMedia)
        {
            this.Title = editMedia.Title;

            TextBoxTitle.Text = editMedia.Title;
            TextBoxArtist.Text = editMedia.Artist;
            TextBoxAlbum.Text = editMedia.Album;
            DurationHour.Text = editMedia.Duration.Hours.ToString();
            DurationMinute.Text = editMedia.Duration.Minutes.ToString();
            DurationSecond.Text = editMedia.Duration.Seconds.ToString();

            if (ComboBoxGenre.Items.Count > 0 )
                ComboBoxGenre.Items.Clear();

            foreach (var setting in Properties.Settings.Default.Genre)
                ComboBoxGenre.Items.Add(setting.ToString());

            if(ComboBoxGenre.Items.Count > 0) ComboBoxGenre.SelectedItem=editMedia.Genre;

            //ButtonImage.Source = new BitmapImage(new Uri(editMedia.Image, UriKind.Absolute));
        }

        //open image and show in Image gradnik
        private void ButtonImage_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog fileDial = new OpenFileDialog();
            fileDial.Filter = "PNG (*.png)|*.png|JPG (*.jpg,*.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*";
            if (fileDial.ShowDialog() == true)
            {
                ButtonImage.Source = new BitmapImage(new Uri(fileDial.FileName));
            }
        }

        //TODO: shrani spremenjene vrednosti v prvotni Multimedia item in poslji nazaj v main menu
        private void ButtonCommit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
