using System;
using System.Collections.ObjectModel;
using BlackCross.Core.Markets;
using log4net;

namespace BlackCross.Platform.Terminal.Services
{
    public class MarketManagerService
    {
        public ObservableCollection<LoadedMarket> LoadedMarkets { get; private set; }

        private static readonly ILog _Log = LogManager.GetLogger(typeof(MarketManagerService));

        private const string _MarketFileSuffix = "Market.dll";

        public MarketManagerService()
        {
            LoadedMarkets = new ObservableCollection<LoadedMarket>();
        }

        public bool LoadMarketFromFile(string filename)
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int LoadMarketFromFolder(string folderName)
        {
            try
            {
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }

    public class LoadedMarket
    {
        public string Name { get; set; }

        public Version Version { get; set; }

        public string FilePath { get; set; }

        public MarketBase Market { get; set; }
    }
}
