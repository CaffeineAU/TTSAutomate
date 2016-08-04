using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using NAudio.Wave;
using System.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Speech;
using System.Collections.Specialized;

namespace TTSTranslate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Boolean filenameSelected = false;
        System.Speech.Synthesis.SpeechSynthesizer ssss = new System.Speech.Synthesis.SpeechSynthesizer();
        WebClient wc;

        private ObservableCollection<TTSVoice> ttsVoices = new ObservableCollection<TTSVoice>();

        public ObservableCollection<TTSVoice> TextToSpeechVoices
        {
            get { return ttsVoices; }
            set { ttsVoices = value; }
        }

        private TTSVoice selectedVoice;

        public TTSVoice SelectedVoice
        {
            get { return selectedVoice; }
            set
            {
                selectedVoice = value;
                OnPropertyChanged("SelectedVoice");
            }
        }

        private Voice selectedIvonaVoice;

        public Voice SelectedIvonaVoice
        {
            get { return selectedIvonaVoice; }
            set
            {
                selectedIvonaVoice = value;
                OnPropertyChanged("SelectedIvonaVoice");
            }
        }

        private SupportedVoices supportedVoices;// = new List<Voice>();

        public SupportedVoices IvonaSupportedVoices
        {
            get { return supportedVoices; }
            set
            {
                supportedVoices = value;
                OnPropertyChanged("IvonaSupportedVoices");
            }
        }


        private int engineIndex;

        public int EngineIndex
        {
            get { return TTSEngines.IndexOf(SelectedEngine); }
            set { engineIndex = value; }
        }


        private Boolean needToSave = true;

        public Boolean NeedToSave
        {
            get { return needToSave; }
            set
            {
                needToSave = value;
                OnPropertyChanged("NeedToSave");
                Title = String.Format("TTSAutomate {2} - {0} {1}", PhraseFileName, NeedToSave ? "(Unsaved)" : "", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
        }

        private List<VoiceProvider> providers = new List<VoiceProvider>();

        public List<VoiceProvider> TTSEngines
        {
            get { return providers; }
            set
            {
                providers = value;
                OnPropertyChanged("TTSEngines");
            }
        }

        private VoiceProvider selectedEngine;

        public VoiceProvider SelectedEngine
        {
            get { return selectedEngine; }
            set
            {
                selectedEngine = value;
                OnPropertyChanged("SelectedEngine");
                OnPropertyChanged("UseWeb");
                OnPropertyChanged("UseLocal");
                OnPropertyChanged("EngineIndex");
            }
        }


        private int selectedRowCount;

        public int SelectedRowCount
        {
            get { return selectedRowCount; }
            set { selectedRowCount = value;
                OnPropertyChanged("SelectedRowCount");
            }
        }


        private int speechRate;

        public int SpeechRate
        {
            get { return speechRate; }
            set
            {
                speechRate = value;
                OnPropertyChanged("SpeechRate");
            }
        }

        private int volume = 100;

        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                OnPropertyChanged("Volume");
            }
        }


        public Visibility UseWeb
        {
            get
            {
                return (SelectedEngine.ProviderClass == VoiceProvider.Class.Web) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility UseLocal
        {
            get
            {
                return (SelectedEngine.ProviderClass != VoiceProvider.Class.Web) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        BackgroundWorker DownloaderWorker = new BackgroundWorker();

        private ObservableCollection<PhraseItem> phraseItems = new ObservableCollection<PhraseItem>();

        public ObservableCollection<PhraseItem> PhraseItems
        {
            get { return phraseItems; }
            set
            {
                phraseItems = value;
                OnPropertyChanged("PhraseItems");
            }
        }
        private String phraseFileName = "New Phrase File";

        public String PhraseFileName
        {
            get { return phraseFileName; }
            set
            {
                phraseFileName = value;
                OnPropertyChanged("PhraseFileName");
            }
        }

        private String outputDirectoryName;

        public String OutputDirectoryName
        {
            get { return outputDirectoryName; }
            set
            {
                outputDirectoryName = value;
                OnPropertyChanged("OutputDirectoryName");
            }
        }

        private Boolean workerFinished = true;

        public Boolean WorkerFinished
        {
            get { return workerFinished; }
            set
            {
                workerFinished = value;
                OnPropertyChanged("WorkerFinished");
            }
        }

        private int downloadCount = 0;

        public int DownloadCount
        {
            get { return downloadCount; }
            set
            {
                downloadCount = value;
                OnPropertyChanged("DownloadCount");
            }
        }

        private int downloadProgress = 0;

        public int DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                OnPropertyChanged("DownloadProgress");
            }
        }


        private List<CultureInfo> cultures = new List<CultureInfo>();

        public List<CultureInfo> Cultures
        {
            get { return cultures; }
            set
            {
                cultures = value;
                OnPropertyChanged("Cultures");
            }
        }

        private CultureInfo selectedCulture = CultureInfo.CurrentCulture;

        public CultureInfo SelectedCulture
        {
            get { return selectedCulture; }
            set
            {
                selectedCulture = value;
                OnPropertyChanged("SelectedCulture");
            }
        }

        public BitmapImage HeaderImage { get; private set; }
        MediaPlayer mp = new MediaPlayer();
        private int InitialPhraseItems = 499;

        public MainWindow()
        {
            InitializeComponent();
            BackgroundWorker loadIvonaVoicesWorker = new BackgroundWorker();
            loadIvonaVoicesWorker.DoWork += delegate
            {
                IvonaSupportedVoices = IvonaRequest.IvonaListVoices();
                SelectedIvonaVoice = IvonaSupportedVoices.Voices[0];
            };
            loadIvonaVoicesWorker.RunWorkerAsync();
            

            //phraseItems.Add(new PhraseItem());
            HeaderImage = LoadImage("speech-bubble.png");
            List<PhraseItem> initialitems = new List<PhraseItem>();
            for (int i = 0; i < InitialPhraseItems; i++)
            {
                initialitems.Add(new PhraseItem());
            }

            PhraseItems = new ObservableCollection<PhraseItem>(initialitems);

            TTSEngines.Add(new VoiceProvider { Name = "Ivona", ProviderType = VoiceProvider.Provider.Ivona, ProviderClass = VoiceProvider.Class.Web });
            TTSEngines.Add(new VoiceProvider { Name = "Google Translate", ProviderType = VoiceProvider.Provider.Google, ProviderClass = VoiceProvider.Class.Web });
            foreach (var voice in ssss.GetInstalledVoices())
            {
                TTSEngines.Add(new VoiceProvider { Name = voice.VoiceInfo.Name, ProviderType = VoiceProvider.Provider.Microsoft, ProviderClass = VoiceProvider.Class.Local });
            }
            TTSEngines.Add(new VoiceProvider { Name = "fromtexttospeech.com", ProviderType = VoiceProvider.Provider.wwwfromtexttospeechcom, ProviderClass = VoiceProvider.Class.Web });

            SelectedEngine = TTSEngines[0];


            Title = String.Format("TTSAutomate {2} - {0} {1}", PhraseFileName, "(Unsaved)", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            Cultures.AddRange(CultureInfo.GetCultures(CultureTypes.FrameworkCultures));
            Cultures.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            DownloaderWorker.DoWork += DownloaderWorkder_DoWork;
            DownloaderWorker.WorkerReportsProgress = true;
            DownloaderWorker.WorkerSupportsCancellation = true;
            DownloaderWorker.RunWorkerCompleted += DownloaderWorkder_RunWorkerCompleted;
            DownloaderWorker.ProgressChanged += DownloaderWorkder_ProgressChanged;

            TextToSpeechVoices.Add(new TTSVoice { Name = "Emma (UK English)", Voice = "IVONA Amy22 (UK English)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Harry (UK English)", Voice="IVONA Brian22 (UK English)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Jade (French)", Voice = "IVONA CΘline22 (French)" });
            TextToSpeechVoices.Add(new TTSVoice { Name = "Nicole (AU English)", Voice = "IVONA Nicole22" });
            TextToSpeechVoices.Add(new TTSVoice { Name = "Gabriel (French)", Voice="IVONA Mathieu22 (French)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Nadine (German)", Voice="IVONA Marlene22 (German)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Michael (German)", Voice="IVONA Hans22 (German)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Valentina (Russian)", Voice="IVONA Tatyana22 (Russian)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "John (US English)", Voice = "IVONA Eric22"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Jenna (US English)", Voice="IVONA Jennifer22"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "George (US English)", Voice="IVONA Joey22"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Alice (US English)", Voice="IVONA Kimberly22"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Daisy (US English)", Voice="IVONA Salli22"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Alessandra (Italian)", Voice="IVONA Carla22 (Italian)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Giovanni (Italian)", Voice="IVONA Giorgio22 (Italian)"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Isabella (Spanish [Modern])", Voice="IVONA Conchita22 (Spanish [Modern])"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Mateo (Spanish [Modern])", Voice="IVONA Enrique22 (Spanish [Modern])"});
            TextToSpeechVoices.Add(new TTSVoice { Name = "Rodrigo (Portuguese)", Voice="IVONA Cristiano22 (Portuguese)"});
            SelectedVoice = TextToSpeechVoices[0];
        

            this.DataContext = this;
        }

        private void DownloaderWorkder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
        }

        private void DownloaderWorkder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerFinished = true;
            DownloadProgress = DownloadCount;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static BitmapImage LoadImage(string fileName)
        {
            var image = new BitmapImage();

            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }

        private void DownloaderWorkder_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            try
            {
                if (SelectedEngine.ProviderClass == VoiceProvider.Class.Local)
                {
                    ssss = new System.Speech.Synthesis.SpeechSynthesizer();
                }
                else
                {
                    wc = new WebClient();
                }

                foreach (var item in PhraseItems)
                {
                    if (!IsPhraseEmpty(item))
                    {

                        if (!DownloaderWorker.CancellationPending)
                        {
                            if (!item.DownloadComplete)
                            {
                                System.IO.Directory.CreateDirectory(String.Format("{0}\\mp3\\{1}\\", OutputDirectoryName, item.Folder));
                                System.IO.Directory.CreateDirectory(String.Format("{0}\\wav\\{1}\\", OutputDirectoryName, item.Folder));
                                switch (SelectedEngine.ProviderType)
                                {
                                    case VoiceProvider.Provider.Microsoft:
                                        ssss.SelectVoice(SelectedEngine.Name);
                                        ssss.Volume = Volume;
                                        ssss.Rate = SpeechRate;

                                        ssss.SetOutputToWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", OutputDirectoryName, item.Folder, item.FileName), new System.Speech.AudioFormat.SpeechAudioFormatInfo(16000, System.Speech.AudioFormat.AudioBitsPerSample.Sixteen, System.Speech.AudioFormat.AudioChannel.Mono));
                                        ssss.Speak(item.Phrase);

                                        break;
                                    case VoiceProvider.Provider.Google:
                                        wc.DownloadFile(String.Format("http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&client=tw-ob&q={0}&tl={1}", item.Phrase, SelectedCulture.Name), String.Format("{0}\\mp3\\{1}\\{2}.mp3", OutputDirectoryName, item.Folder, item.FileName));
                                        using (Mp3FileReader mp3 = new Mp3FileReader(String.Format("{0}\\mp3\\{1}\\{2}.mp3", OutputDirectoryName, item.Folder, item.FileName)))
                                        {
                                            var newFormat = new WaveFormat(16000, 16, 1);
                                            using (var conversionStream = new WaveFormatConversionStream(newFormat, mp3))
                                            {
                                                WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", OutputDirectoryName, item.Folder, item.FileName), conversionStream);
                                            }
                                        }
                                        break;
                                    case VoiceProvider.Provider.Ivona:
                                        File.WriteAllBytes(String.Format("{0}\\mp3\\{1}\\{2}.mp3", OutputDirectoryName, item.Folder, item.FileName), IvonaRequest.IvonaCreateSpeech(item.Phrase, SelectedIvonaVoice));
                                        using (Mp3FileReader mp3 = new Mp3FileReader(String.Format("{0}\\mp3\\{1}\\{2}.mp3", OutputDirectoryName, item.Folder, item.FileName)))
                                        {
                                            var newFormat = new WaveFormat(16000, 16, 1);
                                            using (var conversionStream = new WaveFormatConversionStream(newFormat, mp3))
                                            {
                                                WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", OutputDirectoryName, item.Folder, item.FileName), conversionStream);
                                            }
                                        }

                                        break;
                                    case VoiceProvider.Provider.wwwfromtexttospeechcom:
                                        HTTPPost h = new HTTPPost(item.Phrase, String.Format("{0}\\mp3\\{1}\\{2}.mp3", OutputDirectoryName, item.Folder, item.FileName), SelectedVoice.Voice);
                                        using (Mp3FileReader mp3 = new Mp3FileReader(String.Format("{0}\\mp3\\{1}\\{2}.mp3", OutputDirectoryName, item.Folder, item.FileName)))
                                        {
                                            var newFormat = new WaveFormat(16000, 16, 1);
                                            using (var conversionStream = new WaveFormatConversionStream(newFormat, mp3))
                                            {
                                                WaveFileWriter.CreateWaveFile(String.Format("{0}\\wav\\{1}\\{2}.wav", OutputDirectoryName, item.Folder, item.FileName), conversionStream);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                item.DownloadComplete = true;
                                DownloaderWorker.ReportProgress(++i);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (SelectedEngine.ProviderClass == VoiceProvider.Class.Local)
                {
                    ssss.Dispose();
                }
                else
                {
                    wc.Dispose();

                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Couldn't download audio\r\n\r\n" + Ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio(((Button)sender).CommandParameter);
        }

        private void PlayAudio(object file)
        {
            mp.Open(new Uri(String.Format("{0}\\wav\\{1}.wav", OutputDirectoryName, file), UriKind.RelativeOrAbsolute));
            mp.Volume = 1;
            mp.Play();
            mp.MediaEnded += delegate { mp.Close(); };
        }

        private void VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in PhraseItems)
            {
                item.DownloadComplete = false;
            }
        }

        private void WordsListView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                int rowIndex = e.Row.GetIndex();
                (WordsListView.Items[rowIndex] as PhraseItem).DownloadComplete = false;
                NeedToSave = true;
            }
        }

        private void LoadPhraseFile(String filename)
        {
            PhraseItems.Clear();
            Regex r = new Regex(@"(?<Folder>.+)\|(?<FileName>.+)\|(?<Phrase>.+)\b");

            List<PhraseItem> items = new List<PhraseItem>();

            for (int i = 0; i < InitialPhraseItems; i++)
            {
                items.Add(new PhraseItem());
            }

            int j = 0;

            using (System.IO.StreamReader file = new StreamReader(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
                string contents = file.ReadToEnd();

                foreach (Match match in r.Matches(contents))
                {
                    if (j >= phraseItems.Count)
                    {
                        items.Add(new PhraseItem());
                    }
                    items[j++] = (new PhraseItem { Index = PhraseItems.Count, Folder = match.Groups["Folder"].Value, FileName = match.Groups["FileName"].Value, Phrase = match.Groups["Phrase"].Value, DownloadComplete = false });
                }
            }

            PhraseItems = new ObservableCollection<PhraseItem>(items);
        }

        private void SavePhraseFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrEmpty(PhraseFileName);
        }

        private void BrowsePhraseFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NeedToSave && PhraseItems.Count(n => !IsPhraseEmpty(n)) > 0)
            {
                switch (MessageBox.Show("Your phrases file is not saved. Would you like to save it before you open another file?", "Unsaved Phrases file", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.Yes:
                        SaveOrSaveAs();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        break;
                }
            }

            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.InitialDirectory = Properties.Settings.Default.LastPhraseFile;

            dlg.Title = "Open a Phrase file";
            dlg.Filter = "Phrase Files (*.psv)|*.psv|All Files (*.*)|*.*";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                PhraseFileName = dlg.FileName;
                LoadPhraseFile(PhraseFileName);
                filenameSelected = true;
                Properties.Settings.Default.LastPhraseFile = Path.GetDirectoryName(dlg.FileName);
                NeedToSave = false;
            }
        }

        private void CreatePhraseFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NeedToSave && PhraseItems.Count(n => !IsPhraseEmpty(n)) > 0)
            {
                switch (MessageBox.Show("Your phrases file is not saved. Would you like to save it before you create another file?", "Unsaved Phrases file", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.Yes:
                        if (SaveOrSaveAs())
                        {
                            break;
                        }
                        else
                        {
                            return;
                        }
                    case MessageBoxResult.No:
                        break;
                    default:
                        return;
                }
            }

            PhraseFileName = "New Phrase File";
            filenameSelected = false;
            NeedToSave = false;
        }

        private void BrowseOutputDirectoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select output directory";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Properties.Settings.Default.LastOutputDirectory;
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OutputDirectoryName = dlg.FileName;
                Properties.Settings.Default.LastOutputDirectory = dlg.FileName;
            }
        }

        private void SavePhraseFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveOrSaveAs();
        }

        private Boolean SaveOrSaveAs()
        {
            if (filenameSelected)
            {
                SavePhraseFile(PhraseFileName);
                return true;
            }
            else
            {
                return SaveAs();
            }
        }

        private void SavePhraseFile(string Filename)
        {
            using (System.IO.StreamWriter file = new StreamWriter(new FileStream(Filename, FileMode.Create, FileAccess.Write)))
            {
                foreach (var item in PhraseItems)
                {
                    if (!IsPhraseEmpty(item))
                    {
                        file.WriteLine(String.Format("{0}|{1}|{2}", item.Folder, item.FileName, item.Phrase));
                    }
                }
                PhraseFileName = Filename;
                filenameSelected = true;
                NeedToSave = false;
            }
        }

        private bool IsPhraseEmpty(PhraseItem item)
        {
            return String.IsNullOrEmpty(item.Folder) && String.IsNullOrEmpty(item.FileName) && String.IsNullOrEmpty(item.Phrase);
        }

        private void SaveAsPhraseFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }

        private Boolean SaveAs()
        {
            var dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Title = "Save Phrase file";
            dlg.Filter = "Phrase Files (*.psv)|*.psv|All Files (*.*)|*.*";
            dlg.InitialDirectory = Properties.Settings.Default.LastPhraseFile;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SavePhraseFile(dlg.FileName);
                Properties.Settings.Default.LastPhraseFile = Path.GetDirectoryName(dlg.FileName);
            }
            return result == System.Windows.Forms.DialogResult.OK;
        }

        private void StartDownloadingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (!String.IsNullOrEmpty(OutputDirectoryName) && !String.IsNullOrEmpty(PhraseFileName) && WorkerFinished);
        }

        private void StartDownloadingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!DownloaderWorker.IsBusy)
            {
                WorkerFinished = false;
                DownloaderWorker.RunWorkerAsync();
                DownloadCount = PhraseItems.Count(n => (n.DownloadComplete == false && !IsPhraseEmpty(n)));
            }
        }
        private void StopDownloadingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DownloaderWorker.IsBusy;
        }

        private void StopDownloadingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DownloaderWorker.IsBusy)
            {
                DownloaderWorker.CancelAsync();
            }
        }

        private void InsertRowsAboveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WordsListView.SelectedItems.Count > 0;
        }

        private void InsertRowsAboveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int rowsToAdd = WordsListView.SelectedItems.Count;
            for (int i = 0; i < rowsToAdd; i++)
            {
                PhraseItems.Insert(PhraseItems.IndexOf(WordsListView.SelectedItems[0] as PhraseItem), new PhraseItem());
            }
        }

        private void InsertRowsBelowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WordsListView.SelectedItems.Count > 0;
        }

        private void InsertRowsBelowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int rowsToAdd = WordsListView.SelectedItems.Count;
            for (int i = 0; i < rowsToAdd; i++)
            {
                PhraseItems.Insert(PhraseItems.IndexOf(WordsListView.SelectedItems[WordsListView.SelectedItems.Count - 1] as PhraseItem) + 1, new PhraseItem());
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (PhraseItems.Count > 0)
            {
                if (NeedToSave && PhraseItems.Count(n => !IsPhraseEmpty(n)) > 0)
                {
                    switch (MessageBox.Show("Your phrases file is not saved. Would you like to save it before you exit?", "Unsaved Phrases file", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            break;
                        case MessageBoxResult.Yes:
                            SaveOrSaveAs();
                            break;
                        case MessageBoxResult.No:
                            break;
                        default:
                            break;
                    }
                }
                Properties.Settings.Default.Save();
            }
        }

        private void WordsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) NeedToSave = true;
            if (e.Key == Key.Enter)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while ((dep != null) && !(dep is DataGridCell) && !(dep is System.Windows.Controls.Primitives.DataGridColumnHeader))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                if (dep is DataGridCell)
                {
                    if ((dep as DataGridCell).Column.Header.ToString() != "Play")
                    {
                        if ((dep as DataGridCell).IsEditing)
                        {
                            WordsListView.CommitEdit();
                        }
                        else
                        {
                            WordsListView.BeginEdit();
                        }
                    }
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Right && !((DependencyObject)e.OriginalSource is TextBox))
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                DataGridCell cell = dep as DataGridCell;
                cell.IsSelected = false;
                DependencyObject nextRowCell;
                nextRowCell = cell.PredictFocus(FocusNavigationDirection.Right);

                if (nextRowCell == null || (nextRowCell as DataGridCell).Column.Header.ToString() == "Play")
                {
                    nextRowCell = cell.PredictFocus(FocusNavigationDirection.Left);
                    nextRowCell = (nextRowCell as DataGridCell).PredictFocus(FocusNavigationDirection.Down);

                    while ((nextRowCell as DataGridCell).PredictFocus(FocusNavigationDirection.Left) != null)
                    {
                        nextRowCell = (nextRowCell as DataGridCell).PredictFocus(FocusNavigationDirection.Left);
                    }
                    WordsListView.CurrentCell = new DataGridCellInfo(nextRowCell as DataGridCell);
                    (nextRowCell as DataGridCell).IsSelected = true;
                    e.Handled = true;
                }
            }
        }

        private void EngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in PhraseItems)
            {
                item.DownloadComplete = false;
            }

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var item in PhraseItems)
            {
                item.DownloadComplete = false;
            }

        }

        private void WordsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRowCount = WordsListView.SelectedItems.Count;
        }
    }
    public class VoiceProvider : INotifyPropertyChanged
    {

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

    }

    public class PhraseItem : INotifyPropertyChanged
    {
        public int Index { get; set; }

        private String folder;

        public String Folder
        {
            get { return folder; }
            set
            {
                folder = value;
                OnPropertyChanged("Folder");
                OnPropertyChanged("FullPathAndFile");
            }
        }

        private String fileName;

        public String FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
                OnPropertyChanged("FullPathAndFile");
            }
        }

        public String FullPathAndFile
        {
            get
            {
                return String.Format("{0}\\{1}", Folder, FileName);
            }
        }
        public String Phrase { get; set; }

        private Boolean downloadComplete = false;

        public Boolean DownloadComplete
        {
            get { return downloadComplete; }
            set
            {
                downloadComplete = value;
                OnPropertyChanged("DownloadComplete");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
    public class RowIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((values[0] as DataGridRow).GetIndex() + 1).ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TTSVoice
    {
        public String Name { get; set; }
        public String Voice { get; set; }
    }

    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private bool _notificationSupressed = false;
        private bool _supressNotification = false;
        public bool SupressNotification
        {
            get
            {
                return _supressNotification;
            }
            set
            {
                _supressNotification = value;
                if (_supressNotification == false && _notificationSupressed)
                {
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
                    _notificationSupressed = false;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SupressNotification)
            {
                _notificationSupressed = true;
                return;
            }
            base.OnCollectionChanged(e);
        }
    }
}
// http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&client=tw-ob&q=Thousand&tl=En-au