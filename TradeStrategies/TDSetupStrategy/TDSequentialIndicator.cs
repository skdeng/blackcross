using System.Collections.Generic;
using System.Linq;

namespace BlackCross.TradeStrategies.TDSetupStrategy
{
    public static class TDSequentialIndicator
    {
        public static List<int> ComputeTDSetup(IEnumerable<decimal> priceList, TDSetupOptions options = null)
        {
            options = options ?? new TDSetupOptions();

            var tdSetup = new List<int>(priceList.Count()) { 0, 0, 0, 0 };

            for (var i = 4; i < priceList.Count(); i++)
            {
                // higher than 4 periods ago
                if (priceList.ElementAt(i) > priceList.ElementAt(i - 4))
                {
                    // previous setup was green or null
                    if (tdSetup.ElementAt(i - 1) >= 0)
                    {
                        // previous setup was 9
                        if (tdSetup.ElementAt(i - 1) == 9)
                        {
                            tdSetup.Add(options.SetupRecyclesOnNextPeriod ? 1 : 0);
                        }
                        else
                        {
                            tdSetup.Add(tdSetup.ElementAt(i - 1) + 1);
                        }
                    }
                    else
                    {
                        tdSetup.Add(options.SetupRecyclesOnNextPeriod ? 1 : 0);
                    }
                }
                // lower than 4 periods ago
                else
                {
                    // previous setup was red or null
                    if (tdSetup.ElementAt(i - 1) <= 0)
                    {
                        // previous setup was -9
                        if (tdSetup.ElementAt(i - 1) == -9)
                        {
                            tdSetup.Add(options.SetupRecyclesOnNextPeriod ? -1 : 0);
                        }
                        else
                        {
                            tdSetup.Add(tdSetup.ElementAt(i - 1) - 1);
                        }
                    }
                    else
                    {
                        tdSetup.Add(options.SetupRecyclesOnNextPeriod ? -1 : 0);
                    }
                }
            }

            return tdSetup;
        }
    }

    public class TDSetupOptions
    {
        public bool SetupRecyclesOnNextPeriod = true;

        public bool SetupStartsOnlyAfterPriceFlip = false;
    }
}
