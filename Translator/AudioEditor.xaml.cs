using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.IO;

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for AudioEditor.xaml
    /// </summary>
    public partial class AudioEditor : Window, INotifyPropertyChanged
    {
        WaveChannel32 sound0;

        float max = 0;
        float min = 1000;

        private float silenceThreshold = -48;

        public float SilenceThreshold
        {
            get { return silenceThreshold; }
            set
            {
                silenceThreshold = value;
                pwfc.SelectionStart = pwfc.XLocationToTimeSpan(pwfc.WaveFormDisplay.Values.First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key);
                pwfc.SelectionEnd = pwfc.XLocationToTimeSpan(pwfc.WaveFormDisplay.Values.Reverse().First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key);
                pwfc.DrawSelectionRect();
                OnPropertyChanged("SilenceThreshold");
            }
        }


        private int sampleRate = 0;

        public int SampleRate
        {
            get { return sampleRate; }
            set
            {
                sampleRate = value;
                OnPropertyChanged("SampleRate");
            }
        }

        private int bitsPerSample = 0;

        public int BitsPerSample
        {
            get { return bitsPerSample; }
            set
            {
                bitsPerSample = value;
                OnPropertyChanged("BitsPerSample");
            }
        }

        private int channels = 0;

        public int Channels
        {
            get { return channels; }
            set
            {
                channels = value;
                OnPropertyChanged("Channels");
            }
        }

        private String filename = "";

        public String FileName
        {
            get { return filename; }
            set
            {
                filename = value;
                //wfr = new WaveFileReader(value);
                MemoryStream ms = new MemoryStream(File.ReadAllBytes(value));
                var wfr = new WaveFileReader(ms);


                sound0 = new WaveChannel32(wfr);
                SampleRate = wfr.WaveFormat.SampleRate;
                BitsPerSample = wfr.WaveFormat.BitsPerSample;
                Duration = wfr.TotalTime;
                Channels = wfr.WaveFormat.Channels;
                pwfc.AddNewWaveForm(Color.FromRgb(67, 217, 150), SampleRate, BitsPerSample, Channels);
                LoadSound(sound0, 0);
                wfr.Close();
            }
        }

        private TimeSpan duration;

        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged("Duration");
            }
        }

        int bufferSize = 1024;

        public AudioEditor(string filename)
        {
            InitializeComponent();
            FileName = filename; 
            this.DataContext = this;

        }

        private void LoadSound(WaveChannel32 sound, int index)
        {
            int count = 0;
            int read = 0;
            sound.Sample += Sound0_Sample;
            bufferSize =1024* sampleRate * 16 / 256000*Channels ;

            byte[] buffer = new byte[bufferSize];

            while (sound.Position < sound.Length)
            {
                max =-1;
                min = 1;

                read = sound.Read(buffer, 0, bufferSize);
                pwfc.WaveFormDisplay.AddValue(max, min);
                count++;
            }

            sound.Close();
            //Debug.WriteLine("Sound is " + sound.TotalTime.TotalMilliseconds + "ms long");
            //Debug.WriteLine("Sound is " + wfr.Length + " bytes");
            //Debug.WriteLine("Called addvalue " + count + " times");
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSound(sound0, 0);
        }

        private void pwfc_MouseUp(object sender, MouseButtonEventArgs e)
        {

            var file = new AudioFileReader(FileName);
            var trimmed = new OffsetSampleProvider(file);
            trimmed.SkipOver = pwfc.SelectionStart;
            trimmed.Take = TimeSpan.FromMilliseconds(Math.Abs(pwfc.SelectionEnd.TotalMilliseconds - pwfc.SelectionStart.TotalMilliseconds));

            //WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            new Task(() =>
            {
                var player = new WaveOutEvent();
                player.Init(trimmed);
                player.Play();
                while (player.PlaybackState != PlaybackState.Stopped)
                {
                    System.Threading.Thread.Sleep(100);
                }
                file.Close();
            }).Start();

        }

        private void Sound0_Sample(object sender, SampleEventArgs e)
        {
            max = Math.Max(max, e.Left);
            min = Math.Min(min, e.Left);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var file = new AudioFileReader(FileName);
            var trimmed = new OffsetSampleProvider(file);
            trimmed.SkipOver = pwfc.SelectionStart;
            trimmed.Take = pwfc.SelectionEnd - pwfc.SelectionStart;

            WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            pwfc.ClearWaveForm();
            pwfc.AddNewWaveForm(Color.FromRgb(67, 217, 150), SampleRate, BitsPerSample, Channels);

            FileName = @"c:\temp\trimmed.wav";
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(FileName));
            var file2 = new WaveFileReader(ms);
            new Task(() =>
            {
                var player = new WaveOutEvent();
                player.Init(file2);
                player.Play();
                while (player.PlaybackState != PlaybackState.Stopped)
                {
                    System.Threading.Thread.Sleep(100);
                }
                file2.Close();
            }).Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var file = new AudioFileReader(FileName);
            var trimmed = new OffsetSampleProvider(file);
            int totrim = pwfc.WaveFormDisplay.Values.First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key;
            trimmed.SkipOver = pwfc.XLocationToTimeSpan(totrim);
            trimmed.Take = TimeSpan.Zero;

            WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            pwfc.ClearWaveForm();
            pwfc.AddNewWaveForm(Color.FromRgb(67, 217, 150), SampleRate, BitsPerSample, Channels);

            FileName = @"c:\temp\trimmed.wav";
            //LoadSound(sound0, 0);
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(FileName));
            var file2 = new WaveFileReader(ms);
            new Task(() =>
            {
                var player = new WaveOutEvent();
                player.Init(file2);
                player.Play();
                while (player.PlaybackState != PlaybackState.Stopped)
                {
                    System.Threading.Thread.Sleep(100);
                }
                file2.Close();
            }).Start();


        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var file = new AudioFileReader(FileName);
            var trimmed = new OffsetSampleProvider(file);
            int totrim = pwfc.WaveFormDisplay.Values.First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key;
            pwfc.SelectionStart = pwfc.XLocationToTimeSpan(pwfc.WaveFormDisplay.Values.First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key);
            pwfc.SelectionEnd = pwfc.XLocationToTimeSpan(pwfc.WaveFormDisplay.Values.Reverse().First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key);
            pwfc.DrawSelectionRect();

            trimmed.SkipOver = pwfc.XLocationToTimeSpan(totrim);
            trimmed.Take = pwfc.SelectionEnd - pwfc.SelectionStart;

            WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            pwfc.ClearWaveForm();
            pwfc.AddNewWaveForm(Color.FromRgb(67, 217, 150), SampleRate, BitsPerSample, Channels);

            FileName = @"c:\temp\trimmed.wav";
            //LoadSound(sound0, 0);
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(FileName));
            var file2 = new WaveFileReader(ms);
            new Task(() =>
            {
                var player = new WaveOutEvent();
                player.Init(file2);
                player.Play();
                while (player.PlaybackState != PlaybackState.Stopped)
                {
                    System.Threading.Thread.Sleep(100);
                }
                file2.Close();
            }).Start();


        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var file = new AutoDisposeFileReader( new AudioFileReader(FileName));
            var trimmed = new OffsetSampleProvider(file);
            int totrim = pwfc.WaveFormDisplay.Values.First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key;
            pwfc.SelectionStart = pwfc.XLocationToTimeSpan(pwfc.WaveFormDisplay.Values.First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key);
            pwfc.SelectionEnd = pwfc.XLocationToTimeSpan(pwfc.WaveFormDisplay.Values.Reverse().First(n => n.Value.Item1 > NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold) && n.Value.Item2 < NAudio.Utils.Decibels.DecibelsToLinear(SilenceThreshold)).Key);
            pwfc.DrawSelectionRect();

            trimmed.SkipOver = TimeSpan.Zero; 
            trimmed.Take = pwfc.SelectionEnd ;

            WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            pwfc.ClearWaveForm();
            pwfc.AddNewWaveForm(Color.FromRgb(67, 217, 150), SampleRate, BitsPerSample, Channels);

            FileName = @"c:\temp\trimmed.wav";

            MemoryStream ms = new MemoryStream(File.ReadAllBytes(FileName));
            var file2 = new WaveFileReader(ms);
            new Task(() =>
            {
                var player = new WaveOutEvent();
                player.Init(file2);
                player.Play();
                while (player.PlaybackState != PlaybackState.Stopped)
                {
                    System.Threading.Thread.Sleep(100);
                }
                file2.Close();
            }).Start();

        }
    }

    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
