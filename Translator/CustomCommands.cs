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
                new KeyGesture(Key.A, ModifierKeys.Control)
            }
    );
        public static readonly RoutedUICommand InsertRowsAboveCommand = new RoutedUICommand
    (
            "InsertRowsAboveCommand",
            "InsertRowsAboveCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Up, ModifierKeys.Control)
            }
    );
        public static readonly RoutedUICommand InsertRowsBelowCommand = new RoutedUICommand
    (
            "InsertRowsBelowCommand",
            "InsertRowsBelowCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Down, ModifierKeys.Control)
            }
    );
        public static readonly RoutedUICommand MoveRowsUpCommand = new RoutedUICommand
    (
            "MoveRowsUpCommand",
            "MoveRowsUpCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.PageUp, ModifierKeys.Control)
            }
    );
        public static readonly RoutedUICommand MoveRowsDownCommand = new RoutedUICommand
    (
            "MoveRowsDownCommand",
            "MoveRowsDownCommand",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.PageDown, ModifierKeys.Control)
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
    }
}
