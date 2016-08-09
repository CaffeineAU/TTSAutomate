using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSAutomate
{
    public abstract class TTSProvider: INotifyPropertyChanged
    {
        // this is ugly
        public enum Provider
        {
            Microsoft,
            Google,
            Ivona,
            wwwfromtexttospeechcom
        }

        public enum Class
        {
            Local,
            Web,
        }

        private Boolean hasVoices = false;

        public Boolean HasVoices
        {
            get { return hasVoices; }
            set
            {
                hasVoices = value;
                OnPropertyChanged("HasVoices");
            }
        }

        private Boolean hasNumericSpeed = false;

        public Boolean HasNumericSpeed
        {
            get { return hasNumericSpeed; }
            set
            {
                hasNumericSpeed = value;
                OnPropertyChanged("HasNumericSpeed");
            }
        }

        private Boolean hasDiscreteSpeed= false;

        public Boolean HasDiscreteSpeed
        {
            get { return hasDiscreteSpeed; }
            set
            {
                hasDiscreteSpeed = value;
                OnPropertyChanged("HasDiscreteSpeed");
            }
        }

        private Boolean hasVolume = false;

        public Boolean HasVolume
        {
            get { return hasVolume; }
            set
            {
                hasVolume = value;
                OnPropertyChanged("HasVolume");
            }
        }


        public string Name { get; set; }

        public Provider ProviderType { get; set; }

        public Class ProviderClass { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Name, ProviderClass);
        }

        //public abstract byte[] GetVoice(string phrase);

        private List<Voice> availableVoices = new List<Voice>();

        public List<Voice> AvailableVoices
        {
            get { return availableVoices; }
            set
            {
                availableVoices = value;
                OnPropertyChanged("AvailableVoices");
            }
        }

        private Voice selectedVoice;

        public Voice SelectedVoice
        {
            get { return selectedVoice; }
            set
            {
                selectedVoice = value;
                OnPropertyChanged("SelectedVoice");
            }
        }

        private List<String> availableSpeeds = new List<string>();

        public List<String> AvailableSpeeds
        {
            get { return availableSpeeds; }
            set
            {
                availableSpeeds = value;
                OnPropertyChanged("AvailableSpeeds");
            }
        }

        private String selectedSpeed = "";

        public String SelectedSpeed
        {
            get { return selectedSpeed; }
            set
            {
                selectedSpeed = value;
                OnPropertyChanged("SelectedSpeed");
            }
        }


        public abstract void AnnounceVoice();

        public abstract Boolean DownloadItem(PhraseItem item, string folder);

        public abstract Boolean DownloadAndPlay(PhraseItem item);

    }

    public class Voice
    {
        public String Name { get; set; }
        public String Language { get; set; }
        public String Gender { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}", Name, Language, Gender);
        }
    }


}
