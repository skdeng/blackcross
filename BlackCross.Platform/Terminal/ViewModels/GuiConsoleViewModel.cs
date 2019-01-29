using System.Collections.ObjectModel;
using BlackCross.Platform.Terminal.Views;

namespace BlackCross.Platform.Terminal.ViewModels
{
    public class GuiConsoleViewModel : ViewModelBase
    {
        private ObservableCollection<ConsoleEntry> consoleOutput = new ObservableCollection<ConsoleEntry>();
        public ObservableCollection<ConsoleEntry> ConsoleOutput
        {
            get => consoleOutput;
            set { consoleOutput = value; OnPropertyChanged(nameof(ConsoleOutput)); }
        }
    }

    public class ConsoleEntry
    {
        public string Text { get; set; }

        public string Color { get; set; }

        public GuiConsoleLogLevel LogLevel { get; set; }
    }
}
