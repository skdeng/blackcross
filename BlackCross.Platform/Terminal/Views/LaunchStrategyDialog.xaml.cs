using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using BlackCross.Core.Markets;
using BlackCross.Core.TradeStrategies;
using BlackCross.Markets.HistoricalMarket;
using BlackCross.Platform.Terminal.ViewModels;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BlackCross.Platform.Terminal.Views
{
    /// <summary>
    /// Interaction logic for LaunchStrategyDialog.xaml
    /// </summary>
    public partial class LaunchStrategyDialog : Window
    {
        public LaunchStrategyDialogViewModel ViewModel { get; set; }

        public MarketBase Market { get; }

        public ITradeStrategyFactory TradeStrategyFactory { get; set; }

        public TradeStrategyBase TradeStrategy { get; set; }

        public string DataFilePath { get; set; }

        public string ConfigFilePath { get; set; }

        private readonly Regex _DigitOnlyRegex = new Regex("[^0-9]+");

        private ILog _Log = LogManager.GetLogger(nameof(LaunchStrategyDialog));

        public LaunchStrategyDialog(ITradeStrategyFactory tradeStrategyFactory, MarketBase market)
        {
            InitializeComponent();

            ViewModel = new LaunchStrategyDialogViewModel();
            DataContext = ViewModel;

            TradeStrategyFactory = tradeStrategyFactory;
            Market = market;
        }

        private void SelectDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "All files (*.*)|*.*|Text files (.txt)|*.txt|CSV files (*.csv)|*.csv",
                FilterIndex = 3,
                InitialDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
            };

            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                DataFilePath = fileDialog.FileName;
                var textShown = DataFilePath.Length > 40 ? $"...{DataFilePath.Substring(DataFilePath.Length - 40)}" : DataFilePath;
                DataFilePathText.Text = textShown;
            }
        }

        private void SelectConfigFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                DefaultExt = ".config",
                Filter = "All files (*.*)|*.*|Config files (.config)|*.config|Text files (*.txt)|*.txt",
                FilterIndex = 1,
                InitialDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
            };

            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                ConfigFilePath = fileDialog.FileName;
                var textShown = ConfigFilePath.Length > 40 ? $"...{ConfigFilePath.Substring(ConfigFilePath.Length - 40)}" : ConfigFilePath;
                ConfigFilePathText.Text = textShown;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            (Market as HistoricalMarket).LoadDataFromCsv(DataFilePath, "Security", "Currency");

            var strategyConfiguration = ReadConfiguration();
            TradeStrategy = TradeStrategyFactory.BuildStrategy(Market, strategyConfiguration);
            DialogResult = true;
        }

        private IStrategyConfiguration ReadConfiguration()
        {
            var strategyConfigurationType = TradeStrategyFactory.ConfigurationType;
            var strategyConfiguration = TradeStrategyFactory.GetConfiguration();

            var configFileJsonObj = JObject.Parse(File.ReadAllText(ConfigFilePath));

            try
            {
                foreach (var prop in strategyConfigurationType.GetProperties())
                {
                    var propName = prop.Name;
                    if (propName == "Name")
                    {
                        continue;
                    }

                    var propValue = configFileJsonObj[propName];
                    var propType = prop.PropertyType;

                    strategyConfigurationType.InvokeMember(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, strategyConfiguration, new object[] { propValue.ToObject(propType) });
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to load strategy configuration", ex);
                return null;
            }

            return strategyConfiguration;
        }
    }
}
