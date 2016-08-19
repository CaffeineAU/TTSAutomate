using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TTSAutomate
{
    class FromTextToSpeechTTSProvider : TTSProvider
    {
        public FromTextToSpeechTTSProvider()
        {
            Name = "From Text To Speech";
            ProviderClass = Class.Web;
            HasVoices = true;
            AvailableVoices.Add(new Voice { Name = "Emma (UK English)", Language = "IVONA Amy22 (UK English)" });
            AvailableVoices.Add(new Voice { Name = "Harry (UK English)", Language = "IVONA Brian22 (UK English)" });
            AvailableVoices.Add(new Voice { Name = "Jade (French)", Language = "IVONA CΘline22 (French)" });
            AvailableVoices.Add(new Voice { Name = "Gabriel (French)", Language = "IVONA Mathieu22 (French)" });
            AvailableVoices.Add(new Voice { Name = "Nadine (German)", Language = "IVONA Marlene22 (German)" });
            AvailableVoices.Add(new Voice { Name = "Michael (German)", Language = "IVONA Hans22 (German)" });
            AvailableVoices.Add(new Voice { Name = "Valentina (Russian)", Language = "IVONA Tatyana22 (Russian)" });
            AvailableVoices.Add(new Voice { Name = "John (US English)", Language = "IVONA Eric22" });
            AvailableVoices.Add(new Voice { Name = "Jenna (US English)", Language = "IVONA Jennifer22" });
            AvailableVoices.Add(new Voice { Name = "George (US English)", Language = "IVONA Joey22" });
            AvailableVoices.Add(new Voice { Name = "Alice (US English)", Language = "IVONA Kimberly22" });
            AvailableVoices.Add(new Voice { Name = "Daisy (US English)", Language = "IVONA Salli22" });
            AvailableVoices.Add(new Voice { Name = "Alessandra (Italian)", Language = "IVONA Carla22 (Italian)" });
            AvailableVoices.Add(new Voice { Name = "Giovanni (Italian)", Language = "IVONA Giorgio22 (Italian)" });
            AvailableVoices.Add(new Voice { Name = "Isabella (Spanish [Modern])", Language = "IVONA Conchita22 (Spanish [Modern])" });
            AvailableVoices.Add(new Voice { Name = "Mateo (Spanish [Modern])", Language = "IVONA Enrique22 (Spanish [Modern])" });
            AvailableVoices.Add(new Voice { Name = "Rodrigo (Portuguese)", Language = "IVONA Cristiano22 (Portuguese)" });
            SelectedVoice = AvailableVoices[0];

            if (Properties.Settings.Default.RememberLanguageSettings && this.Name == Properties.Settings.Default.LastTTSProvider)
            {
                SelectedVoice = AvailableVoices.Find(n => n.Name == Properties.Settings.Default.LastTTSVoice);
            }
            else
            {
                SelectedVoice = AvailableVoices[0];
            }

            HasDiscreteSpeed = true;
            AvailableSpeeds.AddRange(new String[] { "slow", "medium", "fast", "very fast" });
            SelectedDiscreteSpeed = "medium";
        }

        public override void DownloadItem(PhraseItem item, string folder, Boolean? convertToWav)
        {
            try
            {
                new Task(() =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(GetDownloadURL(item.Phrase), String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName));
                    }
                    if (convertToWav.Value == true)
                    {
                        using (Mp3FileReader mp3 = new Mp3FileReader(String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName)))
                        {
                            var newFormat = new WaveFormat(16000, 1);
                            using (var resampler = new MediaFoundationResampler(mp3, newFormat))
                            {
                                resampler.ResamplerQuality = 60;
                                WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", folder, item.Folder, item.FileName), resampler);
                            }
                        }
                    }
                    item.DownloadComplete = true;
                }).Start();
            }
            catch(Exception Ex)
            {
                Logger.Log(Ex.ToString());
                item.DownloadComplete = false;
            }
        }

        private int DiscreteSpeedToInt(string speed)
        {
            switch (speed)
            {
                case "slow":
                    return -1;
                case "medium":
                    return 0;
                case "fast":
                    return 1;
                case "very fast":
                    return 2;
                default:
                    return 0;            }
        }

        public override void DownloadAndPlayItem(PhraseItem item, string folder, Boolean? convertToWav)
        {
            try
            {
                new Task(() =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(GetDownloadURL(item.Phrase), String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName));
                    }
                    if (convertToWav.Value == true)
                    {
                        using (Mp3FileReader mp3 = new Mp3FileReader(String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName)))
                        {
                            var newFormat = new WaveFormat(16000, 1);
                            using (var resampler = new MediaFoundationResampler(mp3, newFormat))
                            {
                                resampler.ResamplerQuality = 60;
                                WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", folder, item.Folder, item.FileName), resampler);
                            }
                        }
                    }
                    item.DownloadComplete = true;
                    MainWindow.PlayAudioFullPath(String.Format("{0}\\wav\\{1}\\{2}.wav", folder, item.Folder, item.FileName));
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
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(GetDownloadURL(item.Phrase), String.Format("{0}\\mp3\\{1}\\{2}.mp3", Path.GetTempPath(), item.Folder, item.FileName));
                MainWindow.PlayAudioFullPath(String.Format("{0}\\mp3\\{1}\\{2}.mp3", Path.GetTempPath(), item.Folder, item.FileName));
            }
        }

        private string GetDownloadURL(string Phrase)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://www.fromtexttospeech.com/");

            int start = SelectedVoice.Name.IndexOf("(")+1;
            int end = SelectedVoice.Name.IndexOf(")");


            var postData = String.Format("input_text={0}&language={3}&voice={1}&speed={2}&action=process_text", Phrase, SelectedVoice.Language, DiscreteSpeedToInt(SelectedDiscreteSpeed), SelectedVoice.Name.Substring(start, end-start));
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            Regex r = new Regex(@"flashvars\=\'file\=\/output\/([0-9]*\/[0-9]*\.mp3)\'");

            Match m = r.Match(responseString);

            return "http://www.fromtexttospeech.com/output/" + m.Groups[1].Value;

        }


    }

}
