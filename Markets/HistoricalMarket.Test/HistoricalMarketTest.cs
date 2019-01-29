using System;
using System.Collections.Generic;
using System.IO;
using BlackCross.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackCross.Markets.HistoricalMarket.Test
{
    [TestClass]
    public class HistoricalMarketTest
    {
        [TestMethod]
        public void OpenOrder_LimitOrder()
        {
            var startingPortfolio = new Portfolio();
            startingPortfolio.AddSecurity("USD", 1000m);
            var market = new HistoricalMarket(TimeSpan.FromHours(1), startingPortfolio, 0);

            GenerateConstantDataCsv("test.csv", 100, 100, TimeSpan.FromHours(1));
            market.LoadDataFromCsv("test.csv", "SPX", "USD");

            market.Tick();

            market.OpenOrder(new Order()
            {
                BaseCurrency = "USD",
                Price = 100,
                Security = "SPX",
                Side = OrderSide.Buy,
                Type = OrderType.LimitOrder,
                Volume = 1
            });
        }

        [TestMethod]
        public void SimpleTick()
        {
            var securityName = "TEST";
            var currencyName = "USD";

            var startingPortfolio = new Portfolio();
            var market = new HistoricalMarket(TimeSpan.FromHours(1), startingPortfolio, 0);

            var csvName = "test.csv";
            GenerateConstantDataCsv(csvName, 100, 100, TimeSpan.FromHours(1));
            market.LoadDataFromCsv(csvName, securityName, currencyName);

            var last = market.GetLastPrice();
            Assert.IsNull(last);

            last = market.Tick();
            Assert.IsNotNull(last);
            Assert.IsNotNull(last[securityName]);
            Assert.IsTrue(last[securityName].Price > 0);

            var firstTickTime = last[securityName].Timestamp;

            last = market.Tick();
            Assert.IsNotNull(last);
            Assert.IsNotNull(last[securityName]);
            Assert.IsTrue(last[securityName].Price > 0);

            Assert.IsTrue((last[securityName].Timestamp - firstTickTime).TotalHours == 1);
        }

        private void GenerateConstantDataCsv(string filename, decimal value, int count, TimeSpan interval)
        {
            var lines = new List<string>
            {
                "Date,Close"
            };
            var date = new DateTime(2000, 1, 1);
            for (var i = 0; i < 5; i++)
            {
                lines.Add($"{date},{value}");
                date += interval;
            }
            File.WriteAllLines(filename, lines);
        }
    }
}
