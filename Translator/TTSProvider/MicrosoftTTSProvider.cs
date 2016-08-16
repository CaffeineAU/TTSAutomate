using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TTSAutomate
{
    class MicrosoftTTSProvider : TTSProvider
    {

        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        public MicrosoftTTSProvider()
        {
            Name = "Microsoft Windows Text To Speech";
            ProviderClass = Class.Local;
            HasVoices = true;
            HasDiscreteSpeed = true;
            HasDiscreteVolume = true;
            BackgroundWorker loadVoicesWorker = new BackgroundWorker();
            loadVoicesWorker.DoWork += delegate
            {
                foreach (var v in speechSynth.GetInstalledVoices().Select(v => v.VoiceInfo))
                {
                    AvailableVoices.Add(new Voice { Name = v.Name, Gender = v.Gender.ToString(), Language = v.Culture.DisplayName });
                }
                SelectedVoice = AvailableVoices[0];
            };
            loadVoicesWorker.RunWorkerAsync();

            for (int i = -10; i <= 10; i++)
            {
                AvailableSpeeds.Add(i.ToString());

            }
            SelectedDiscreteSpeed = "0";

            for (int i = 0; i <= 100; i++)
            {
                AvailableVolumes.Add(i.ToString());

            }
            SelectedDiscreteVolume = "100";
        }

        public override Boolean DownloadItem(PhraseItem item, string folder, Boolean? convertToWav)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var synth = new SpeechSynthesizer())
                    {
                        synth.SelectVoice(SelectedVoice.Name);
                        synth.Volume = Int32.Parse(SelectedDiscreteVolume);
                        synth.Rate = Int32.Parse(SelectedDiscreteSpeed);
                        synth.SetOutputToWaveStream(ms);//.SetOutputToWaveFile(String.Format("{0}\\wav22050\\{1}\\{2}.wav", folder, item.Folder, item.FileName));
                        synth.Speak(item.Phrase);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    using (WaveFileReader wav = new WaveFileReader(ms))// String.Format("{0}\\wav22050\\{1}\\{2}.wav", folder, item.Folder, item.FileName)))
                    {
                        FileStream fs = new FileStream(String.Format("{0}\\mp3\\{1}\\{2}.mp3", folder, item.Folder, item.FileName), FileMode.Create);
                        using (var writer = new LameMP3FileWriter(fs, wav.WaveFormat, 128))
                        {
                            wav.CopyTo(writer);
                        }
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    using (WaveFileReader wav = new WaveFileReader(ms))// String.Format("{0}\\wav22050\\{1}\\{2}.wav", folder, item.Folder, item.FileName)))
                    {
                        var newFormat = new WaveFormat(16000, 1);
                        using (var resampler = new MediaFoundationResampler(wav, newFormat))
                        {
                            resampler.ResamplerQuality = 60;
                            WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", folder, item.Folder, item.FileName), resampler);
                        }
                    }
                }
                return true;
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex.ToString());
                return false;
            }
        }

        public override Boolean DownloadAndPlay(PhraseItem item)
        {
            return true;
        }

        public override void Play(PhraseItem item)
        {
            using (var synth = new SpeechSynthesizer())
            {
                synth.SelectVoice(SelectedVoice.Name);
                synth.Volume = Int32.Parse(SelectedDiscreteVolume);
                synth.Rate = Int32.Parse(SelectedDiscreteSpeed);
                synth.SetOutputToDefaultAudioDevice();
                synth.Speak(item.Phrase);
            }
        }

    }

}
