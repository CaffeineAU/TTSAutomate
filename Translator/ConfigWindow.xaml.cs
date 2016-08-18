using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window, INotifyPropertyChanged
    {

        
        public BitmapImage HeaderImage { get; private set; }

        private Boolean reOpenPSV;

        public Boolean ReOpenPSV
        {
            get { return reOpenPSV; }
            set { reOpenPSV = value; }
        }

        private Boolean setOutputDirectory;

        public Boolean SetOutputDirectory
        {
            get { return setOutputDirectory; }
            set { setOutputDirectory = value; }
        }

        private Boolean encodeToWav;

        public Boolean EncodeToWav
        {
            get { return encodeToWav; }
            set { encodeToWav = value; }
        }

        private Boolean rememberLanguageSettings;

        public Boolean RememberLanguageSettings
        {
            get { return rememberLanguageSettings; }
            set { rememberLanguageSettings = value; }
        }

        private IvonaRegion selectedIvonaRegion;

        public IvonaRegion SelectedIvonaRegion
        {
            get { return selectedIvonaRegion; }
            set { selectedIvonaRegion = value; }
        }

        private List<IvonaRegion> ivonaRegions = new List<IvonaRegion>();

        public List<IvonaRegion> IvonaRegions
        {
            get { return ivonaRegions; }
            set { ivonaRegions = value; }
        }

        public Boolean Result { get; private set; }

        public ConfigWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            HeaderImage = MainWindow.LoadImage("settings.png");

            IvonaRegions.Add(new IvonaRegion { RegionName = "eu-west-1", Description = "EU, Dublin" });
            IvonaRegions.Add(new IvonaRegion { RegionName = "us-east-1", Description = "US East, N. Virginia" });
            IvonaRegions.Add(new IvonaRegion { RegionName = "us-west-2", Description = "US West, Oregon" });
            Result = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }

    
}
