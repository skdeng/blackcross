using Newtonsoft.Json;

namespace BlackCross.Core.Markets
{
    public interface IMarketConfiguration
    {
        [JsonProperty(Required = Required.DisallowNull)]
        string Name { get; }

        [JsonProperty(Required = Required.Always)]
        IMarketDataFeedConfiguration DataFeedConfiguration { get; }
    }
}
