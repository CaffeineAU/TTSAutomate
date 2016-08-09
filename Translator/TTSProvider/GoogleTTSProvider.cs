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
    partial class GoogleTTSProvider : TTSProvider
    {
        WebClient wc = new WebClient();

        public GoogleTTSProvider()
        {
            Name = "Google Text To Speech";
            ProviderType = Provider.Google;
            ProviderClass = Class.Web;
            HasVoices = true;
            BackgroundWorker loadVoicesWorker = new BackgroundWorker();
            loadVoicesWorker.DoWork += delegate
            {
                List<CultureInfo> cultures = new List<CultureInfo>();
                cultures.AddRange(CultureInfo.GetCultures(CultureTypes.FrameworkCultures));
                cultures.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));
                AvailableVoices = cultures.Select(x => new Voice() { Name = x.DisplayName, Language = x.Name }).ToList();
                SelectedVoice = AvailableVoices[0];
            };
            loadVoicesWorker.RunWorkerAsync();
        }

        public override Boolean DownloadItem(PhraseItem item, string folder)
        {
            try
            {
                wc.DownloadFile(String.Format("http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&client=tw-ob&q={0}&tl={1}", item.Phrase, SelectedVoice.Language), String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName));
                using (Mp3FileReader mp3 = new Mp3FileReader(String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName)))
                {
                    var newFormat = new WaveFormat(16000, 1);
                    using (var resampler = new MediaFoundationResampler(mp3, newFormat))
                    {
                        resampler.ResamplerQuality = 60;
                        WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", folder, item.Folder, item.FileName), resampler);
                    }
                }
                return true;
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex);
                return false;
            }
        }

        public override Boolean DownloadAndPlay(PhraseItem item)
        {
            return true;
        }

        public override void AnnounceVoice()
        {
        }

    }

}
