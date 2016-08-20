﻿using System;
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

        bool initialLoad = true;
        
        public BitmapImage HeaderImage { get; private set; }

        private int myVar;

        public int MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }


        public List<String> IvonaRegions { get; set; }

        public List<int> SampleRates { get; set; }

        public List<int> BitsPerSamples { get; set; }

        public ConfigWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            HeaderImage = MainWindow.LoadImage("settings.png");

            IvonaRegions = new List<string>();
            IvonaRegions.Add("eu-west-1");
            IvonaRegions.Add("us-east-1");
            IvonaRegions.Add("us-west-2");

            SampleRates = new List<int>();
            SampleRates.Add(8000);
            SampleRates.Add(11025);
            SampleRates.Add(16000);
            SampleRates.Add(22050);
            SampleRates.Add(44100);
            SampleRates.Add(48000);

            BitsPerSamples = new List<int>();
            BitsPerSamples.Add(8);
            BitsPerSamples.Add(16);
            BitsPerSamples.Add(24);

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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialLoad)
            {
                (Owner as MainWindow).SetItemsAsDirty();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initialLoad = false;
        }
    }

    
}
