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
using System.Windows.Threading;
using NAudio.Wave.SampleProviders;
using System.Windows.Shell;
using System.Threading;

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Boolean filenameSelected = false;

        private bool isManualEditCommit;

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

        private List<TTSProvider> providers = new List<TTSProvider>();

        public List<TTSProvider> TTSEngines
        {
            get { return providers; }
            set
            {
                providers = value;
                OnPropertyChanged("TTSEngines");
            }
        }

        private TTSProvider selectedEngine;

        public TTSProvider SelectedEngine
        {
            get { return selectedEngine; }
            set
            {
                selectedEngine = value;
                OnPropertyChanged("SelectedEngine");
            }
        }

        private int selectedRowCount;

        public int SelectedRowCount
        {
            get { return selectedRowCount; }
            set
            {
                selectedRowCount = value;
                OnPropertyChanged("SelectedRowCount");
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

        public BitmapImage HeaderImage { get; private set; }
        public static MediaPlayer media = new MediaPlayer();
        private int InitialPhraseItems = 499;
        private static bool LoadedWindow = false;

        public MainWindow()
        {
            InitializeComponent();

            //NAudio.MediaFoundation.MediaFoundationApi.Startup();
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

            HeaderImage = LoadImage("speech-bubble.png");

            List<PhraseItem> initialitems = new List<PhraseItem>();
            for (int i = 0; i < InitialPhraseItems; i++)
            {
                initialitems.Add(new PhraseItem { Phrase = "" });
            }

            PhraseItems = new ObservableCollection<PhraseItem>(initialitems);

            TTSEngines.Add(new IvonaTTSProvider());
            TTSEngines.Add(new GoogleTTSProvider());
            TTSEngines.Add(new MicrosoftTTSProvider());
            TTSEngines.Add(new FromTextToSpeechTTSProvider());
            //foreach (var voice in ssss.GetInstalledVoices())
            //{
            //    TTSEngines.Add(new TTSProvider { Name = voice.VoiceInfo.Name, ProviderType = VoiceProvider.Provider.Microsoft, ProviderClass = VoiceProvider.Class.Local });
            //}
            //TTSEngines.Add(new TTSProvider { Name = "fromtexttospeech.com", ProviderType = VoiceProvider.Provider.wwwfromtexttospeechcom, ProviderClass = VoiceProvider.Class.Web });

            SelectedEngine = TTSEngines[0];

            Title = String.Format("TTSAutomate {2} - {0} {1}", PhraseFileName, "(Unsaved)", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            DownloaderWorker.DoWork += DownloaderWorker_DoWork;
            DownloaderWorker.WorkerReportsProgress = true;
            DownloaderWorker.WorkerSupportsCancellation = true;
            DownloaderWorker.RunWorkerCompleted += DownloaderWorker_RunWorkerCompleted;
            DownloaderWorker.ProgressChanged += DownloaderWorker_ProgressChanged;

            this.DataContext = this;
        }

        private void DownloaderWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
            TaskbarItemInfo.ProgressValue = (double)(e.ProgressPercentage)/(double)(DownloadCount);
        }

        private void DownloaderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerFinished = true;
            DownloadProgress = DownloadCount;
            TaskbarItemInfo.ProgressValue = 1.0;
            WordsListView.Focus();
            //TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

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

        private void DownloaderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var item in PhraseItems)
                {
                    if (!IsPhraseEmpty(item))
                    {
                        if (!DownloaderWorker.CancellationPending)
                        {
                            if (!item.DownloadComplete)
                            {
                                DownloadItem(item, false);
                            }
                        }
                        else
                        {
                            e.Result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Couldn't download audio\r\n\r\n" + Ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            while (PhraseItems.Count(n => n.DownloadComplete) < PhraseItems.Count(n=> !String.IsNullOrEmpty(n.Phrase)))
            {
                if (!DownloaderWorker.CancellationPending)
                {
                    DownloaderWorker.ReportProgress(PhraseItems.Count(n => n.DownloadComplete));
                    Thread.Sleep(100);
                }
                else
                {
                    return;
                }

            }
        }

        private void DownloadItem(PhraseItem item, bool play)
        {
            System.IO.Directory.CreateDirectory(String.Format("{0}\\mp3\\{1}\\", OutputDirectoryName, item.Folder));
            System.IO.Directory.CreateDirectory(String.Format("{0}\\wav\\{1}\\", OutputDirectoryName, item.Folder));
            if (play)
            {
                SelectedEngine.DownloadAndPlayItem(item, OutputDirectoryName);
            }
            else
            {
                SelectedEngine.DownloadItem(item, OutputDirectoryName);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridRow)) // Find the cell that was clicked
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            // now dep is the row that contains the button that was clicked

            if (((dep as DataGridRow).DataContext as PhraseItem).DownloadComplete)
            {
                PlayAudio(((Button)sender).CommandParameter);
            }
            else
            {
                if (!IsPhraseEmpty((dep as DataGridRow).DataContext as PhraseItem))
                {
                    DownloadItem((dep as DataGridRow).DataContext as PhraseItem, true);
                }
            }
        }

        private void PlayAudio(object file)
        {
            PlayAudioFullPath(String.Format("{0}\\wav\\{1}.wav", OutputDirectoryName, file), false);
        }

        public static void PlayAudioFullPath(string file, Boolean? deleteAfterPlay = false)
        {
            if (LoadedWindow)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    media.Open(new Uri(file, UriKind.RelativeOrAbsolute));
                    media.Volume = 1;
                    media.Play();
                    media.MediaEnded += delegate
                    {
                        media.Close();
                        if (deleteAfterPlay.Value == true)
                        {
                            File.Delete(file);
                        }

                    };
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new Action(() =>
                      {
                          media.Open(new Uri(file, UriKind.RelativeOrAbsolute));
                          media.Volume = 1;
                          media.Play();
                          media.MediaEnded += delegate
                          {
                              media.Close();
                              if (deleteAfterPlay.Value == true)
                              {
                                  File.Delete(file);
                              }

                          };
                      }));
                }
            }
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
                if (!isManualEditCommit)
                {
                    isManualEditCommit = true;
                    DataGrid grid = (DataGrid)sender;
                    grid.CommitEdit(DataGridEditingUnit.Row, true);
                    isManualEditCommit = false;
                }
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
                items.Add(new PhraseItem { Phrase = "" });
            }

            int j = 0;

            using (System.IO.StreamReader file = new StreamReader(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
                string contents = file.ReadToEnd();

                foreach (Match match in r.Matches(contents))
                {
                    if (j >= items.Count)
                    {
                        items.Add(new PhraseItem { Phrase = "" });
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
            try
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
                    foreach (var item in PhraseItems)
                    {
                        item.DownloadComplete = false;
                    }
                }
            }
            catch (Exception ex) // Are we on windows XP? then we must use the old folder browser dialog
            {
                var dlg = new System.Windows.Forms.FolderBrowserDialog();
                dlg.RootFolder = Environment.SpecialFolder.MyComputer;// 
                dlg.SelectedPath = Properties.Settings.Default.LastOutputDirectory;
                dlg.ShowNewFolderButton = true;
                //dlg.Title = "Select output directory";
                //dlg.IsFolderPicker = true;
                //dlg.InitialDirectory 
                //dlg.AddToMostRecentlyUsedList = false;
                //dlg.AllowNonFileSystemItems = false;
                //dlg.EnsurePathExists = true;
                //dlg.EnsureReadOnly = false;
                //dlg.EnsureValidNames = true;
                //dlg.Multiselect = false;
                //dlg.ShowPlacesList = true;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OutputDirectoryName = dlg.SelectedPath;
                    Properties.Settings.Default.LastOutputDirectory = dlg.SelectedPath;
                    foreach (var item in PhraseItems)
                    {
                        item.DownloadComplete = false;
                    }

                }

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
            bool canDownload = (!String.IsNullOrEmpty(OutputDirectoryName));
            foreach (var item in PhraseItems)
            {
                item.CanDownload = canDownload;
            }
            e.CanExecute = canDownload && !String.IsNullOrEmpty(PhraseFileName) && WorkerFinished;
        }

        private void StartDownloadingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!DownloaderWorker.IsBusy)
            {
                WorkerFinished = false;
                DownloaderWorker.RunWorkerAsync();
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

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

        private void MoveRowsUpCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WordsListView.SelectedItems.Count > 0;
        }

        private void MoveRowsUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (PhraseItem item in WordsListView.SelectedItems)
            {
                if (PhraseItems.IndexOf(item) > 0)
                {
                    PhraseItems.Move(PhraseItems.IndexOf(item), PhraseItems.IndexOf(item) - 1);
                }
            }
        }

        private void MoveRowsDownCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WordsListView.SelectedItems.Count > 0;
        }

        private void MoveRowsDownCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            for (int i = WordsListView.SelectedItems.Count-1; i >= 0; i--)
            {
                if (PhraseItems.IndexOf(WordsListView.SelectedItems[i] as PhraseItem) < PhraseItems.Count - 1)
                {
                    Logger.Log(String.Format("Moving {0} to {1}", PhraseItems.IndexOf(WordsListView.SelectedItems[i] as PhraseItem), PhraseItems.IndexOf(WordsListView.SelectedItems[i] as PhraseItem) + 1));
                    PhraseItems.Move(PhraseItems.IndexOf(WordsListView.SelectedItems[i] as PhraseItem), PhraseItems.IndexOf(WordsListView.SelectedItems[i] as PhraseItem) + 1);
                }
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
                //NAudio.MediaFoundation.MediaFoundationApi.Shutdown();
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
                            WordsListView.EndInit();

                        }
                        else
                        {
                            WordsListView.BeginEdit();
                        }
                        e.Handled = true;
                    }
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

        private void SpeechRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var item in PhraseItems)
            {
                item.DownloadComplete = false;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadedWindow = true;
            foreach (var item in TTSEngines)
            {
                item.initialLoad = false;
            }
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

        private String phrase;

        public String Phrase
        {
            get { return phrase; }
            set { phrase = value;
                OnPropertyChanged("Phrase");
            }
        }

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

        private Boolean canDownload = false;

        public Boolean CanDownload
        {
            get { return canDownload; }
            set
            {
                canDownload = value;
                OnPropertyChanged("CanDownload");
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
}
// http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&client=tw-ob&q=Thousand&tl=En-au