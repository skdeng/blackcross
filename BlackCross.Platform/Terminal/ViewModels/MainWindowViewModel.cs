using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using BlackCross.Platform.Terminal.Services;

namespace BlackCross.Platform.Terminal.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _StartButtonText = "Start Strategy";
        public string StartButtonText
        {
            get => _StartButtonText;
            set { _StartButtonText = value; OnPropertyChanged(nameof(StartButtonText)); }
        }

        private SolidColorBrush _StartButtonBackgroundColor = Brushes.Green;
        public SolidColorBrush StartButtonBackgroundColor
        {
            get => _StartButtonBackgroundColor;
            set { _StartButtonBackgroundColor = value; OnPropertyChanged(nameof(StartButtonBackgroundColor)); }
        }

        private bool _IsConsoleVisible = false;
        public bool IsConsoleVisible
        {
            get => _IsConsoleVisible;
            set { _IsConsoleVisible = value; OnPropertyChanged(nameof(IsConsoleVisible)); }
        }

        public IEnumerable<string> StrategiesList => _TradeStrategyManagerService.LoadedStrategies.Select(s => s.Name);

        public IEnumerable<string> MarketsList => _MarketManagerService.LoadedMarkets.Select(m => m.Name);

        private readonly TradeStrategyManagerService _TradeStrategyManagerService;

        private readonly MarketManagerService _MarketManagerService;

        public MainWindowViewModel(TradeStrategyManagerService tradeStrategyManagerService, MarketManagerService marketManagerService)
        {
            _TradeStrategyManagerService = tradeStrategyManagerService;
            _MarketManagerService = marketManagerService;
            tradeStrategyManagerService.LoadedStrategies.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(StrategiesList));
            marketManagerService.LoadedMarkets.CollectionChanged += (object snder, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(MarketsList));
        }
    }
}
