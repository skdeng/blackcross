using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using BlackCross.Core.TradeStrategies;
using log4net;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace BlackCross.Platform.Terminal.Services
{
    public class TradeStrategyManagerService
    {
        public ObservableCollection<LoadedStrategy> LoadedStrategies { get; private set; }

        private static readonly ILog _Log = LogManager.GetLogger(typeof(TradeStrategyManagerService));

        private const string _StrategyFileSuffix = "Strategy.dll";

        public TradeStrategyManagerService()
        {
            LoadedStrategies = new ObservableCollection<LoadedStrategy>();
        }

        public bool LoadTradeStrategiesFromFile(string filename)
        {
            try
            {
                var found = false;
                var assembly = Assembly.LoadFile(filename);
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.GetInterface(nameof(ITradeStrategyFactory)) != null)
                    {
                        var factory = (ITradeStrategyFactory)assembly.CreateInstance(t.FullName);
                        _Log.Info($"Loaded {factory.GetConfiguration().Name} from {filename}");

                        LoadedStrategies.Add(new LoadedStrategy
                        {
                            Name = factory.GetConfiguration().Name,
                            Version = assembly.GetName().Version,
                            FilePath = filename,
                            StrategyFactory = factory
                        });

                        found = true;
                    }
                }
                return found;
            }
            catch
            {
                return false;
            }
        }

        public int LoadTradeStrategiesFromFolder(string folderName)
        {
            try
            {
                var loaded = 0;
                foreach (var file in Directory.GetFiles(folderName))
                {
                    if (file.EndsWith(_StrategyFileSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        _Log.Debug($"Found strategy file {file}");
                        loaded += LoadTradeStrategiesFromFile(file) ? 1 : 0;
                    }
                }

                foreach (var directory in Directory.GetDirectories(folderName))
                {
                    loaded += LoadTradeStrategiesFromFolder(directory);
                }

                return loaded;
            }
            catch
            {
                return -1;
            }
        }

        public JSchema GetStrategyConfigurationSchema(string strategyName)
        {
            var strategyConfiguration = GetStrategyFactory(strategyName).GetConfiguration().GetType();

            var generator = new JSchemaGenerator();
            var schema = generator.Generate(strategyConfiguration);
            schema.Required.Remove("Name");
            schema.Required.Remove("name");
            return schema;
        }

        public ITradeStrategyFactory GetStrategyFactory(string strategyName)
        {
            return LoadedStrategies.FirstOrDefault(s => s.Name == strategyName).StrategyFactory;
        }
    }

    public class LoadedStrategy
    {
        public string Name { get; set; }

        public Version Version { get; set; }

        public string FilePath { get; set; }

        public ITradeStrategyFactory StrategyFactory { get; set; }

        /// <summary>
        /// Get strategy properties as name-value pairs
        /// </summary>
        /// <returns></returns>
        public List<(string name, object value)> GetProperties()
        {
            return new List<(string name, object value)>
            {
                ("Name", Name),
                ("Version", Version),
                ("Loaded From", FilePath)
            };
        }
    }
}
