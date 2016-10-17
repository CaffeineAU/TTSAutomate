using NAudio.Wave;
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

        public AudioEditor()
        {
            InitializeComponent();
            sound0 = new WaveChannel32(new WaveFileReader(@"C:\temp\wav\system\CAP_WARN.wav"));
            sound1 = new WaveChannel32(new WaveFileReader(@"C:\temp\wav\system\ALT_WARN.wav"));
            pwfc.AddNewWaveForm(Color.FromArgb(64, 0, 0, 255), sound0.TotalTime);
            pwfc.AddNewWaveForm(Color.FromArgb(64, 255, 0, 0), sound1.TotalTime);

        }

        private void LoadSound(WaveChannel32 sound, int index)
        {
            int count = 0;
            byte[] buffer = new byte[16384];
            int read = 0;
            
            while (sound.Position < sound.Length)
            {
                float max = 0;
                float min = 1000;
                read = sound.Read(buffer, 0, 1024);

                for (int i = 0; i < read / 4; i++)
                {
                    max = Math.Max(max, BitConverter.ToSingle(buffer, i * 4));
                    min = Math.Min(min, BitConverter.ToSingle(buffer, i * 4));
                }
                pwfc.waveForms[index].AddValue(max, min);
                count++;
            }
            Console.WriteLine("Sound is " + sound.TotalTime.TotalMilliseconds + "ms long");
            Console.WriteLine("Sound is " + sound.Length + " bytes");
            Console.WriteLine("Called addvalue " + count + " times");
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSound(sound0, 0);
            LoadSound(sound1, 1 );
            Clipboard.SetText(pwfc.waveForms[0].sb.ToString());
        }

        private void pwfc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("{0}, {1}", pwfc.waveForms[0].Points, pwfc.waveForms[1].Points);
        }
    }



}
