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

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for AudioEditor.xaml
    /// </summary>
    public partial class AudioEditor : Window, INotifyPropertyChanged
    {
        WaveFileReader wfr;
        WaveChannel32 sound0;

        float max = 0;
        float min = 1000;

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
                wfr = new WaveFileReader(value);

                sound0 = new WaveChannel32(wfr);
                SampleRate = wfr.WaveFormat.SampleRate;
                BitsPerSample = wfr.WaveFormat.BitsPerSample;
                Duration = wfr.TotalTime;
                Channels = wfr.WaveFormat.Channels;
                pwfc.AddNewWaveForm(Color.FromRgb(67, 217, 150), SampleRate, BitsPerSample, Channels);
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
            FileName = filename; //@"J:\Videos\StopMotion\1SecondHum.wav";
            //FileName = @"J:\Videos\StopMotion\Sawtooth.wav";
            //FileName = @"J:\Videos\StopMotion\Sawtooth24.wav";
            //FileName = @"J:\Videos\StopMotion\Sawtooth32.wav";
            //FileName = @"J:\Videos\StopMotion\SawtoothMono.wav";
            //FileName = @"J:\Videos\StopMotion\331980__dmunk__shuffling.wav";
            //FileName = @"C:\Users\Liam O\Downloads\22267__zeuss__the-chime.wav";
            //FileName = @"C:\Users\Liam O\Downloads\345228__v0idation__kids-counting-1-to-20-mono-44khz.wav";
            //FileName = @"C:\Users\Liam O\Downloads\223452__achim-bornhoeft__1-4.wav";
            //FileName = @"C:\temp\wav\system\CAP_Warn.wav";
            //FileName = @"C:\Users\liamo\Downloads\177269__sergeeo__numbers-in-french.wav";
            //FileName = @"C:\Users\liamo\Downloads\26903__vexst__snare-4.wav";
            //FileName = @"C:\Users\liamo\Downloads\363118__fractalstudios__waves-001.wav";
            //FileName = @"C:\Users\liamo\Downloads\364296__mickmon__justa-hick-burl.wav";
            //FileName = @"C:\Users\liamo\Downloads\1kHz_44100Hz_16bit_05sec.wav";
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
                pwfc.waveForm.AddValue(max, min);
                count++;
            }

            sound.Close();
            wfr.Close();
            Debug.WriteLine("Sound is " + sound.TotalTime.TotalMilliseconds + "ms long");
            Debug.WriteLine("Sound is " + wfr.Length + " bytes");
            Debug.WriteLine("Called addvalue " + count + " times");
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSound(sound0, 0);
            //LoadSound(sound1,1 );
            //Clipboard.SetText(sb.ToString());

            //var file = new AudioFileReader(@"C:\temp\wav\system\CAP_WARN.wav");
            //var trimmed = new OffsetSampleProvider(file);
            //trimmed.SkipOver = TimeSpan.FromMilliseconds(200);
            //trimmed.Take = TimeSpan.FromMilliseconds(1460);

            //WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            //var player = new WaveOutEvent();
            //player.Init(trimmed);
            //player.Play();
        }

        private void pwfc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var file = new AudioFileReader(FileName);
            var trimmed = new OffsetSampleProvider(file);
            trimmed.SkipOver =pwfc.SelectionStart;
            trimmed.Take = TimeSpan.FromMilliseconds(Math.Abs(pwfc.SelectionEnd.TotalMilliseconds - pwfc.SelectionStart.TotalMilliseconds));

            //WaveFileWriter.CreateWaveFile(@"c:\temp\trimmed.wav", new SampleToWaveProvider(trimmed));
            var player = new WaveOutEvent();
            player.Init(trimmed);
            player.Play();
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
    }
}
