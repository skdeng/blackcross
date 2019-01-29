using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlackCross.Platform.Terminal.ViewModels;

namespace BlackCross.Platform.Terminal.Views
{
    /// <summary>
    /// Interaction logic for GuiConsole.xaml
    /// </summary>
    public partial class GuiConsole : UserControl
    {
        public GuiConsoleLogLevel LogLevel
        {
            get => logLevel;
            set
            {
                logLevel = value;
                RefreshConsoleContent();
            }
        }

        private ObservableCollection<ConsoleEntry> consoleLogs;

        private GuiConsoleViewModel consoleContent;

        private GuiConsoleLogLevel logLevel;

        private const int MaxLogSize = 1000;

        public GuiConsole()
        {
            InitializeComponent();

            consoleLogs = new ObservableCollection<ConsoleEntry>();
            consoleContent = new GuiConsoleViewModel();
            ConsoleContentList.Items.Clear();
            DataContext = consoleContent;
            MenuItemScrollToBottom.IsChecked = true;
        }

        public void WriteLine(string msg, GuiConsoleLogLevel level, string color = null)
        {
            if (color is null)
            {
                color = "White";
                switch (level)
                {
                    case GuiConsoleLogLevel.Warn:
                        color = "Yellow";
                        break;
                    case GuiConsoleLogLevel.Error:
                        color = "Red";
                        break;
                }
            }

            App.Current?.Dispatcher.Invoke(() =>
            {
                var consoleEntry = new ConsoleEntry { Text = msg, Color = color, LogLevel = level };
                consoleLogs.Add(consoleEntry);
                if (consoleLogs.Count > MaxLogSize)
                {
                    consoleLogs.RemoveAt(0);
                }
                if (msg.IndexOf(FilterTextBox.Text) >= 0 && consoleEntry.LogLevel >= LogLevel)
                {
                    consoleContent.ConsoleOutput.Add(consoleEntry);
                }

                if (MenuItemScrollToBottom.IsChecked)
                {
                    ConsoleScrollViewer.ScrollToBottom();
                }
            });
        }

        private void ClearConsole(object sender, RoutedEventArgs e)
        {
            consoleLogs.Clear();
            consoleContent.ConsoleOutput.Clear();
        }

        private void FilterConsoleLogs(object sender, RoutedEventArgs e)
        {
            RefreshConsoleContent();
        }

        private void RefreshConsoleContent()
        {
            consoleContent.ConsoleOutput = new ObservableCollection<ConsoleEntry>(consoleLogs.Where(log => log.Text.IndexOf(FilterTextBox.Text) >= 0 && log.LogLevel >= LogLevel));
        }
    }

    public enum GuiConsoleLogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3
    }
}
