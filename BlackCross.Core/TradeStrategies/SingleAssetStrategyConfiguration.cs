using Newtonsoft.Json;

namespace BlackCross.Core.TradeStrategies
{
    public abstract class SingleAssetStrategyConfiguration : IStrategyConfiguration
    {
        public abstract string Name { get; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string SecuritySymbol { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string BaseCurrencySymbol { get; set; }
    }
}
