using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MultimedijskiPredvajalnik
{
    //Settings = object of Genre
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            fillComboBoxTimer();
            initTimer();
            fillComboBox();
        }

        //timer for automatic saving (when this window open only)
        DispatcherTimer autosaveTimer = new DispatcherTimer();

        //pridobi ime iz textbox-a in shrani vrednost kot novo nastavitev (Settings)
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TextBoxAdd.Text))
            {
                var valToAdd = TextBoxAdd.Text.ToString();
                if (Properties.Settings.Default.Genre.Contains(valToAdd))
                {
                    MessageBox.Show("Genre with given name already exists!");
                }
                else
                {
                    Properties.Settings.Default.Genre.Add(valToAdd);
                    Properties.Settings.Default.Save();
                    TextBoxAdd.Text = "";
                    fillComboBox();
                }
            }
        }

        //pridobi ime nastavitve (Setting) in ji nastavi novo vrednost iz textbox-a
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TextBoxEdit.Text))
            {
                var valToChange = ComboBoxEdit.SelectedItem.ToString();
                string newValue = TextBoxEdit.Text;

                //get index of selected value
                int index = Properties.Settings.Default.Genre.IndexOf(valToChange);
                //insert new value
                Properties.Settings.Default.Genre.Insert(index, newValue);
                //delete old value (insert + removeAt = "edit")
                Properties.Settings.Default.Genre.RemoveAt(index + 1);

                Properties.Settings.Default.Save();
                TextBoxEdit.Text = "";
                fillComboBox();
            }
        }

        //odstrani nastavitev iz Setting
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var valToDelete = ComboBoxDelete.SelectedItem.ToString();
            Properties.Settings.Default.Genre.Remove(valToDelete);
            Properties.Settings.Default.Save();
            fillComboBox();
        }

        //update vrednost textBox ko izberes item v comboBox
        private void ComboBoxEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ComboBoxEdit.SelectedIndex>-1)
                TextBoxEdit.Text = ComboBoxEdit.SelectedItem.ToString();
        }

        //napolni comboBox-e z vrednostmi iz Settings
        private void fillComboBox()
        {
            if(ComboBoxEdit.Items.Count > 0 && ComboBoxDelete.Items.Count > 0)
            {
                ComboBoxEdit.Items.Clear();
                ComboBoxDelete.Items.Clear();
            }
            //get values from Genre setting in Settings (each is parsed into combobox)
            foreach (var setting in Properties.Settings.Default.Genre)
            {
                ComboBoxEdit.Items.Add(setting.ToString());
                ComboBoxDelete.Items.Add(setting.ToString());
            }
        }

        //napolni comboBox za Timer (Sekunde, Minute)
        private void fillComboBoxTimer()
        {
            ComboBoxInverval.Items.Add("Seconds");
            ComboBoxInverval.Items.Add("Minutes");
            ComboBoxInverval.SelectedIndex = 1; //minutes
        }

        //inicializiraj timer
        private void initTimer()
        {
            //ali je vrednost v textBox-u veljavna (validne stevilke)
            string box = textBoxInterval.Text;
            //check if box contains only numbers
            Regex regex = new Regex("[^0-9.-]+");

            if (!String.IsNullOrWhiteSpace(box) && int.TryParse(box, out int number)) //box is not empty & contains only numbers
            {
                if (ComboBoxInverval.SelectedIndex == 0) //seconds
                {
                    autosaveTimer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(box));
                }

                else if (ComboBoxInverval.SelectedIndex == 1) //minutes
                {
                    autosaveTimer.Interval = TimeSpan.FromMinutes(Convert.ToInt32(box));
                }
            }
            else //default, if textbox value is not valid
            {
                autosaveTimer.Interval = TimeSpan.FromMinutes(5); //5 minutes
            }
            autosaveTimer.Tick += timer_Tick;
            checkBoxCurrentSelection();
        }

        //preveri ali checkBox izbran ali ne, ce je zazeni timer 
        private void checkBoxCurrentSelection()
        {
            if (checkBoxAuto.IsChecked == true)
            {
                autosaveTimer.Start();
            }
            else
            {
                autosaveTimer.Stop();
            }
        }
        
        //sprozi ko timer pritece do nastavljene vrednosti
        void timer_Tick(object sender, EventArgs e)
        {
            saveSettings();
        }

        //shrani trenutne Settings
        private void saveSettings()
        {
            //System.Windows.Forms.MessageBox.Show("Saving");
            Properties.Settings.Default.Save();
        }

        //checkBox spremenjen
        private void checkBoxAuto_CheckChanged(object sender, RoutedEventArgs e)
        {
            checkBoxCurrentSelection();
        }

        //item v comboBox se je spremenil
        private void ComboBoxInverval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            initTimer();
        }

        //vrednost v textBox se je spremenila
        private void textBoxInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            //init component has not finished yet, comboboxInterval is null (sprozi na odprtju okna, da ne vrne error)
            if (ComboBoxInverval != null)
            {
                initTimer();
            }
        }
    }
}
