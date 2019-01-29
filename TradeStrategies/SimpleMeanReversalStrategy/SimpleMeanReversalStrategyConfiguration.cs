using System.ComponentModel;
using BlackCross.Core.TradeStrategies;

namespace BlackCross.TradeStrategies.SimpleMeanReversalStrategy
{
    public class SimpleMeanReversalStrategyConfiguration : SingleAssetStrategyConfiguration
    {
        public override string Name => "Simple Mean Reversal Strategy";

        [Description("Look back for computing moving average")]
        public int Lookback { get; set; }

        [Description("Trade frequency in seconds")]
        public int TradeFrequency { get; set; }
    }
}
