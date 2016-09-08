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
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TTSAutomate
{
    class BingTTSProvider : TTSProvider
    {
        public BingTTSProvider()
        {
            Name = "Bing Text To Speech";
            ProviderClass = Class.Web;
            HasVoices = true;
            BackgroundWorker loadVoicesWorker = new BackgroundWorker();
            loadVoicesWorker.DoWork += delegate
            {
                List<CultureInfo> cultures = new List<CultureInfo>();
                cultures.AddRange(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
                cultures.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));
                AvailableVoices = cultures.Select(x => new Voice() { Name = x.DisplayName + " (Male)", Language = x.Name, Gender = "male" }).ToList();
                AvailableVoices.AddRange(cultures.Select(x => new Voice() { Name = x.DisplayName + " (Female)", Language = x.Name, Gender = "female" }).ToList());
                AvailableVoices.Sort((x, y) => x.Name.CompareTo(y.Name));
                SelectedVoice = AvailableVoices[0];
                if (Properties.Settings.Default.RememberLanguageSettings && this.Name == Properties.Settings.Default.LastTTSProvider)
                {
                    SelectedVoice = AvailableVoices.Find(n => n.Name == Properties.Settings.Default.LastTTSVoice);
                }
                else
                {
                    SelectedVoice = AvailableVoices[0];
                }

            };
            loadVoicesWorker.RunWorkerAsync();
        }

        public override void DownloadItem(PhraseItem item, string folder)
        {
            try
            {
                new Task(() =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(HttpRequestHeader.Cookie, Properties.Settings.Default.BingHeaderString);
                        wc.DownloadFile(String.Format("http://www.bing.com/translator/api/language/Speak?locale={1}&gender={2}&media=audio/mp3&text={0}", item.Phrase, SelectedVoice.Language, SelectedVoice.Gender), String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName));
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
            try
            {
                new Task(() =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(HttpRequestHeader.Cookie, Properties.Settings.Default.BingHeaderString);
                        wc.DownloadFile(String.Format("http://www.bing.com/translator/api/language/Speak?locale={1}&gender={2}&media=audio/mp3&text={0}", item.Phrase, SelectedVoice.Language, SelectedVoice.Gender), String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName));
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
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, Properties.Settings.Default.BingHeaderString);
                    MainWindow.PlayAudioStream(wc.DownloadData(String.Format("http://www.bing.com/translator/api/language/Speak?locale={1}&gender={2}&media=audio/mp3&text={0}", item.Phrase, SelectedVoice.Language, SelectedVoice.Gender)));
                }
                catch
                {

                }
            }
        }
    }

}
