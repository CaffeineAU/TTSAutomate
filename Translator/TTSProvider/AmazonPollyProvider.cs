using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Amazon.Polly;


namespace TTSAutomate
{
    partial class AmazonPollyTTSProvider : TTSProvider
    {
        bool disabled = false;
        AmazonPollyClient polly;

        public AmazonPollyTTSProvider()
        {
            Name = "Amazon Polly";
            ProviderClass = Class.Web;
            HasVoices = true;
            HasDiscreteSpeed = false;
            HasDiscreteVolume = false;
            BackgroundWorker loadVoicesWorker = new BackgroundWorker();

            polly = new AmazonPollyClient(AccessKey, SecretKey, Amazon.RegionEndpoint.USEast2);

            loadVoicesWorker.DoWork += delegate
            {
                try
                {
                    foreach (var v in polly.DescribeVoices(new Amazon.Polly.Model.DescribeVoicesRequest()).Voices)
                    {
                        AvailableVoices.Add(new Voice { Name = v.Name, Gender = v.Gender.ToString(), Language = v.LanguageName });
                    }

                    if (this.Name == Properties.Settings.Default.LastTTSProvider)
                    {
                        if (Properties.Settings.Default.RememberLanguageSettings)
                        {
                            SelectedVoice = AvailableVoices.Find(n => n.Name == Properties.Settings.Default.LastTTSVoice);
                        }
                        else
                        {
                            SelectedVoice = AvailableVoices[0];
                        }
                    }
                }
                catch (Exception Ex)
                {
                    Logger.Log(Ex.ToString());
                    AvailableVoices.Add(new Voice { Name = "Amazon Polly Voices are Disabled", Language = "en-US", Gender = "None"});
                    SelectedVoice = AvailableVoices[0];
                    disabled = true;
                }
            };
            loadVoicesWorker.RunWorkerAsync();

        }

        public override void DownloadItem(PhraseItem item, string folder)
        {
            String SSMLText = String.Format(@"
<speak>
  {0}
</speak >
", item.Phrase.Replace("&", "&amp;"));

            try
            {
                new Task(() =>
                {
                    Amazon.Polly.Model.SynthesizeSpeechRequest ssr = new Amazon.Polly.Model.SynthesizeSpeechRequest();

                    ssr.TextType = TextType.Ssml;
                    ssr.Text = SSMLText;
                    ssr.VoiceId = polly.DescribeVoices(new Amazon.Polly.Model.DescribeVoicesRequest()).Voices.Find(n => n.Name == SelectedVoice.Name).Id;
                    ssr.OutputFormat = OutputFormat.Mp3;

                    using (FileStream output = File.Create(String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName)))
                    {
                        polly.SynthesizeSpeech(ssr).AudioStream.CopyTo(output);
                    }

                    ConvertToWav(item, folder, false, new String[] { Name, SelectedVoice.Name, SelectedDiscreteSpeed, SelectedDiscreteVolume });
                }).Start();
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex.ToString());
                item.DownloadComplete = false;
            }
        }

        public override void DownloadAndPlayItem(PhraseItem item, string folder)
        {
            String SSMLText = String.Format(@"
<speak>
  {0}
</speak >
", item.Phrase.Replace("&", "&amp;"));

            try
            {
                new Task(() =>
                {
                    Amazon.Polly.Model.SynthesizeSpeechRequest ssr = new Amazon.Polly.Model.SynthesizeSpeechRequest();

                    ssr.TextType = TextType.Ssml;
                    ssr.Text = SSMLText;
                    ssr.VoiceId = polly.DescribeVoices(new Amazon.Polly.Model.DescribeVoicesRequest()).Voices.Find(n => n.Name == SelectedVoice.Name).Id;
                    ssr.OutputFormat = OutputFormat.Mp3;
                    ssr.SampleRate = "22050";
                    

                    using (FileStream output = File.Create(String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName)))
                    {
                        polly.SynthesizeSpeech(ssr).AudioStream.CopyTo(output);
                    }

                    ConvertToWav(item, folder, true, new String[] { Name, SelectedVoice.Name, SelectedDiscreteSpeed, SelectedDiscreteVolume });
                }).Start();
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex.ToString());
                item.DownloadComplete = false;
            }
        }

        public override void Play(PhraseItem item)
        {
            if (!disabled)
            {
                Amazon.Polly.Model.SynthesizeSpeechRequest ssr = new Amazon.Polly.Model.SynthesizeSpeechRequest();

                ssr.Text = item.Phrase;
                ssr.VoiceId = polly.DescribeVoices(new Amazon.Polly.Model.DescribeVoicesRequest()).Voices.Find(n => n.Name == SelectedVoice.Name).Id;
                ssr.OutputFormat = OutputFormat.Mp3;
                byte[] mp3;

                using (MemoryStream output = new MemoryStream())
                {
                    polly.SynthesizeSpeech(ssr).AudioStream.CopyTo(output);
                    mp3 = output.ToArray();
                }

                MainWindow.PlayAudioStream(mp3);
            }
        }


    }

}
