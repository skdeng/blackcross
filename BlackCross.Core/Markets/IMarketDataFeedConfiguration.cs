using Newtonsoft.Json;

namespace BlackCross.Core.Markets
{
    public interface IMarketDataFeedConfiguration
    {
        [JsonProperty(Required = Required.DisallowNull)]
        string Name { get; }

        [JsonProperty(Required = Required.Always)]
        string Uri { get; }
    }
}
