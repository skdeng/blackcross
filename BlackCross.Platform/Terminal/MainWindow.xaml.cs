using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using BlackCross.Core;
using BlackCross.Core.Markets;
using BlackCross.Core.TradeStrategies;
using BlackCross.Markets.HistoricalMarket;
using BlackCross.Platform.Terminal.Helpers;
using BlackCross.Platform.Terminal.Services;
using BlackCross.Platform.Terminal.ViewModels;
using BlackCross.Platform.Terminal.Views;
using log4net;
using log4net.Config;
using MessageBox = System.Windows.MessageBox;

namespace BlackCross.Platform.Terminal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Properties
        public MainWindowViewModel ViewModel;
        #endregion

        #region Private Properties
        private TradeStrategyManagerService _TradeStrategyManagerService;
        private MarketManagerService _MarketManagerService;
        private List<(DateTime time, double price)> _PriceData;
        private TradeStrategyBase _ActiveStrategy;
        private MarketBase _ActiveMarket;
        private bool _IsTrading;

        private OpenFileDialog _FilePickerDialog;
        private FolderBrowserDialog _FolderPickerDialog;

        private static readonly ILog _Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            var loggingRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            BasicConfigurator.Configure(loggingRepository, new GuiConsoleAppender(ConsoleLog));
            // Default log level to INFO
            ConsoleLogLevelInfoMenuItem.IsChecked = true;

            _Log.Info("Starting application...");

            _TradeStrategyManagerService = new TradeStrategyManagerService();
            _MarketManagerService = new MarketManagerService();

            var portfolio = new Portfolio();
            portfolio.AddSecurity("Currency", 100000m);
            _ActiveMarket = new HistoricalMarket(TimeSpan.FromHours(1), portfolio, 0);
            _ActiveMarket.DataFeed.NewDataPoint += RecordPriceDataPoint;


            ViewModel = new MainWindowViewModel(_TradeStrategyManagerService, _MarketManagerService);
            SetStartButtonInactive();
            DataContext = ViewModel;

            _PriceData = new List<(DateTime time, double price)>();
            _IsTrading = false;

            StartButtonText.FontWeight = FontWeight.FromOpenTypeWeight(700);
            StartButtonText.Foreground = Brushes.White;

            _FilePickerDialog = new OpenFileDialog
            {
                DefaultExt = ".dll",
                Filter = "Binary file (*.dll)|*.dll",
                FilterIndex = 1,
                InitialDirectory = Assembly.GetEntryAssembly().FullName
            };

            _FolderPickerDialog = new FolderBrowserDialog
            {
                Description = "Select the directory with trade strategies",
                ShowNewFolderButton = false,
            };
        }

        #region Private Methods
        private void StartStrategy(TradeStrategyBase strategy, string dataFilePath)
        {
            _ActiveStrategy = strategy;

            _ActiveStrategy.StartAsync().ContinueWith(prevTask =>
            {
                StopStrategy();
            });

            SetStartButtonActive();
        }

        private void StopStrategy()
        {
            _ActiveStrategy.Stop().Wait();
            var buyTrades = _ActiveStrategy.ExecutedTrades.Where(t => t.Order.Side == OrderSide.Buy).Select(t => (t.Timestamp, (double)t.ExecutionPrice));
            PriceChart.AddBuySignals(buyTrades);
            var sellTrades = _ActiveStrategy.ExecutedTrades.Where(t => t.Order.Side == OrderSide.Sell).Select(t => (t.Timestamp, (double)t.ExecutionPrice));
            PriceChart.AddSellSignals(sellTrades);
            PriceChart.AddPriceData(_PriceData);

            SetStartButtonInactive();
        }

        private void ShowConsole()
        {
            ViewModel.IsConsoleVisible = true;
            _Log.Debug("Showing console");
        }

        private void HideConsole()
        {
            ViewModel.IsConsoleVisible = false;
            _Log.Debug("Hiding console");
        }

        private void SetStartButtonInactive()
        {
            ViewModel.StartButtonText = "Start Strategy";
            ViewModel.StartButtonBackgroundColor = Brushes.Green;
        }

        private void SetStartButtonActive()
        {
            ViewModel.StartButtonText = "Stop Strategy";
            ViewModel.StartButtonBackgroundColor = Brushes.Red;
        }

        private void RecordPriceDataPoint(object sender, MarketDataFrame datapoint)
        {
            var currentDataPoint = datapoint["Security"];
            var price = Math.Round((double)currentDataPoint.Price, MidpointRounding.ToEven);
            _PriceData.Add((currentDataPoint.Timestamp, price));
        }
        #endregion

        #region UI Events
        private void StrategyGetConfigurationSchema(object sender, RoutedEventArgs e)
        {
            var schema = _TradeStrategyManagerService.GetStrategyConfigurationSchema(StrategiesListBox.SelectedValue.ToString());
            MessageBox.Show(this, schema.ToString(), $"{StrategiesListBox.SelectedValue.ToString()} Configuration Schema", MessageBoxButton.OK);
        }

        private void StrategyPropertiesContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedStrategy = _TradeStrategyManagerService.LoadedStrategies.First(s => s.Name == StrategiesListBox.SelectedValue.ToString());
            var properties = selectedStrategy.GetProperties();
            var message = string.Join(Environment.NewLine, properties.Select(p => $"{p.name}: {p.value.ToString()}"));
            MessageBox.Show(this, message, $"{selectedStrategy.Name} Properties", MessageBoxButton.OK);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_IsTrading)
            {
                StopStrategy();
                _IsTrading = false;
            }
            else
            {
                if (StrategiesListBox.SelectedValue == null)
                {
                    MessageBox.Show(this, "Please select a strategy first", "No strategy selected", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var selectedStrategyFactory = _TradeStrategyManagerService.GetStrategyFactory(StrategiesListBox.SelectedValue.ToString());
                var launchStrategyDialog = new LaunchStrategyDialog(selectedStrategyFactory, _ActiveMarket);
                var result = launchStrategyDialog.ShowDialog();
                if (result == true)
                {
                    _IsTrading = true;
                    StartStrategy(launchStrategyDialog.TradeStrategy, launchStrategyDialog.DataFilePath);
                }
            }
        }

        private void ShowConsoleMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            ShowConsole();
        }

        private void ShowConsoleMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            HideConsole();
        }

        private void LoadStrategyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = _FilePickerDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _TradeStrategyManagerService.LoadTradeStrategiesFromFile(_FilePickerDialog.FileName);
            }
        }

        private void LoadStrategyFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = _FolderPickerDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var selectedFolder = _FolderPickerDialog.SelectedPath;
                var loaded = _TradeStrategyManagerService.LoadTradeStrategiesFromFolder(selectedFolder);
                _Log.Debug($"Loaded {loaded} strategies from {selectedFolder}");
            }
        }

        private void LoadMarketMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = _FilePickerDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _MarketManagerService.LoadMarketFromFile(_FilePickerDialog.FileName);
            }
        }

        private void LoadMarketFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConsoleLogLevelMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (ConsoleLogLevelDebugMenuItem.IsChecked)
            {
                ConsoleLog.LogLevel = GuiConsoleLogLevel.Debug;
            }
            else if (ConsoleLogLevelInfoMenuItem.IsChecked)
            {
                ConsoleLog.LogLevel = GuiConsoleLogLevel.Info;
            }
            else if (ConsoleLogLevelWarnMenuItem.IsChecked)
            {
                ConsoleLog.LogLevel = GuiConsoleLogLevel.Warn;
            }
            else if (ConsoleLogLevelErrorMenuItem.IsChecked)
            {
                ConsoleLog.LogLevel = GuiConsoleLogLevel.Error;
            }
        }
        #endregion
    }
}
