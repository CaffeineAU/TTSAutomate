using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TTSAutomate;

namespace TTSAutomate.Commands
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand BrowsePhraseFileCommand = new RoutedUICommand
    (
            "BrowsePhraseFileCommand",
            "BrowsePhraseFileCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Control)
            }
    );

        public static readonly RoutedUICommand BrowsePhraseFileNoShortcutCommand = new RoutedUICommand
    (
            "BrowsePhraseFileNoShortcutCommand",
            "BrowsePhraseFileNoShortcutCommand",
            typeof(CustomCommands),
            null
            );

        public static readonly RoutedUICommand CreatePhraseFileCommand = new RoutedUICommand
    (
            "CreatePhraseFileCommand",
            "CreatePhraseFileCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.N, ModifierKeys.Control)
            }
    );

        public static readonly RoutedUICommand BrowseOutputDirectoryCommand = new RoutedUICommand
    (
            "BrowseOutputDirectoryCommand",
            "BrowseOutputDirectoryCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.P, ModifierKeys.Control)
            }
    );

        public static readonly RoutedUICommand StartDownloadingCommand = new RoutedUICommand
    (
            "StartDownloadingCommand",
            "StartDownloadingCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.G, ModifierKeys.Control)
            }
    );
        public static readonly RoutedUICommand StopDownloadingCommand = new RoutedUICommand
    (
            "StopDownloadingCommand",
            "StopDownloadingCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.H, ModifierKeys.Control)
            }
    );

        public static readonly RoutedUICommand SavePhraseFileCommand = new RoutedUICommand
    (
            "SavePhraseFileCommand",
            "SavePhraseFileCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            }
    );

        public static readonly RoutedUICommand SaveAsPhraseFileCommand = new RoutedUICommand
    (
            "SaveAsPhraseFileCommand",
            "SaveAsPhraseFileCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand InsertRowsAboveCommand = new RoutedUICommand
    (
            "InsertRowsAboveCommand",
            "InsertRowsAboveCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Insert, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand InsertRowsBelowCommand = new RoutedUICommand
    (
            "InsertRowsBelowCommand",
            "InsertRowsBelowCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Insert, ModifierKeys.Control | ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand MoveRowsUpCommand = new RoutedUICommand
    (
            "MoveRowsUpCommand",
            "MoveRowsUpCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.PageUp,ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand MoveRowsDownCommand = new RoutedUICommand
    (
            "MoveRowsDownCommand",
            "MoveRowsDownCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.PageDown, ModifierKeys.Alt)
            }
    );

        public static readonly RoutedUICommand ShowSettingsCommand = new RoutedUICommand
    (
            "ShowSettingsCommand",
            "ShowSettingsCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F12)
            }
    );
        public static readonly RoutedUICommand PlayAllCommand = new RoutedUICommand
    (
            "PlayAllCommand",
            "PlayAllCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.A, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand PlaySelectedCommand = new RoutedUICommand
    (
            "PlaySelectedCommand",
            "PlaySelectedCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.P, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand PausePlayingCommand = new RoutedUICommand
    (
            "PausePlayingCommand",
            "PausePlayingCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.U, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand ResumePlayingCommand = new RoutedUICommand
    (
            "ResumePlayingCommand",
            "ResumePlayingCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.I, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand StopPlayingCommand = new RoutedUICommand
    (
            "StopPlayingCommand",
            "StopPlayingCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand OpenOutputDirectoryCommand = new RoutedUICommand
    (
            "OpenOutputDirectoryCommand",
            "OpenOutputDirectoryCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Alt)
            }
    );
        public static readonly RoutedUICommand OpenPhraseFileCommand = new RoutedUICommand
    (
            "OpenPhraseFileCommand",
            "OpenPhraseFileCommand",
            typeof(CustomCommands),
            null
    );

        public static readonly RoutedUICommand OpenCSVFileCommand = new RoutedUICommand
    (
            "OpenCSVFileCommand",
            "OpenCSVFileCommand",
            typeof(CustomCommands),
            null
    );

    }
}
