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


        private List<String> ivonaRegions = new List<String>();

        public List<String> IvonaRegions
        {
            get { return ivonaRegions; }
            set { ivonaRegions = value; }
        }

        public ConfigWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            HeaderImage = MainWindow.LoadImage("settings.png");

            IvonaRegions.Add("eu-west-1");
            IvonaRegions.Add("us-east-1");
            IvonaRegions.Add("us-west-2");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    
}
