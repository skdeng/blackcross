using Newtonsoft.Json;

namespace BlackCross.Core.TradeStrategies
{
    public interface IStrategyConfiguration
    {
        [JsonProperty(Required = Required.DisallowNull)]
        string Name { get; }
    }
}
