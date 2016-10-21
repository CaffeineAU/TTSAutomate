using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for AudioEditor.xaml
    /// </summary>
    public partial class AudioEditor : Window
    {
        StringBuilder sb = new StringBuilder();
        WaveChannel32 sound0;
        WaveChannel32 sound1;

        float max = 0;
        float min = 1000;


        int bufferSize = 1024;

        public AudioEditor()
        {
            InitializeComponent();
            sound0 = new WaveChannel32(new WaveFileReader(@"C:\temp\wav\system\CAP_WARN.wav"));
            sound1 = new WaveChannel32(new WaveFileReader(@"C:\temp\wav\system\CAP_Warn.wav"));
            //sound1 = new WaveChannel32(new WaveFileReader(@"c:\temp\trimmed.wav"));
            pwfc.AddNewWaveForm(Color.FromRgb(67,217,150), sound0.TotalTime);
           // pwfc.AddNewWaveForm(Color.FromArgb(64, 255, 0, 0), sound1.TotalTime);

        }

        private void LoadSound(WaveChannel32 sound, int index)
        {
            int count = 0;
            byte[] buffer = new byte[bufferSize];
            int read = 0;
            sound.Sample += Sound0_Sample;


            while (sound.Position < sound.Length)
            {
                max =-1;
                min = 1;

                read = sound.Read(buffer, 0, bufferSize);
                sb.AppendFormat("{1}\t{2}\t{3}\t{0}\r\n", NAudio.Utils.Decibels.LinearToDecibels(max), index, count, max);
                pwfc.waveForms[index].AddValue(max, min);
                count++;
            }

            sound.Close();
            Console.WriteLine("Sound is " + sound.TotalTime.TotalMilliseconds + "ms long");
            Console.WriteLine("Sound is " + sound.Length + " bytes");
            Console.WriteLine("Called addvalue " + count + " times");
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

        private void pwfc_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void pwfc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var file = new AudioFileReader(@"c:\temp\wav\system\CAP_WARN.wav");
            var trimmed = new OffsetSampleProvider(file);
            trimmed.SkipOver = pwfc.SelectionStart < pwfc.SelectionEnd? pwfc.SelectionStart: pwfc.SelectionEnd;
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
    }



}
