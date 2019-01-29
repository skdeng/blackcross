using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlackCross.Core;
using BlackCross.Core.Markets;

namespace BlackCross.Markets.HistoricalMarket
{
    public class HistoricalMarketDataFeed : MarketDataFeedBase
    {

        private List<MarketDataFrame> HistoricalDataPoints;

        private IEnumerator<MarketDataFrame> CurrentDataEnumerator;

        public override MarketDataFrame First => HistoricalDataPoints.First();

        public HistoricalMarketDataFeed(TimeSpan? tickInterval = null) : base(tickInterval)
        {
            HistoricalDataPoints = null;
            CurrentDataEnumerator = null;
        }

        public override MarketDataFrame Tick(TimeSpan? tickInterval = null)
        {
            var previousVal = CurrentDataEnumerator.Current;

            if (tickInterval is null)
            {
                tickInterval = TickInterval;
            }

            while (CurrentDataEnumerator.Current.Timestamp - previousVal.Timestamp < TickInterval)
            {
                if (!CurrentDataEnumerator.MoveNext())
                {
                    Log.Warn("Reaching the end of historical data feed");
                    throw new DataFeedInterruptedException(GetType().Name);
                }
                Current = CurrentDataEnumerator.Current;
            }
            OnNewDataPoint(CurrentDataEnumerator.Current);
            return CurrentDataEnumerator.Current;
        }

        /// <summary>
        /// Load data from a csv file into <see cref="HistoricalDataPoints"/>. This operation is incremental, meaning that loadiing multiple files will create a cumulative history
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="security">Symbol for the security</param>
        /// <param name="baseCurrency">Symbol for base currency</param>
        public void LoadDataFromCsv(string filename, string security, string baseCurrency)
        {
            Log.Info($"Loading historical data from {filename}");

            HistoricalDataPoints = new List<MarketDataFrame>();

            var records = ReadCsvFile(filename, security, baseCurrency);
            foreach (var record in records)
            {
                // Index where to insert the new record
                int insertionIndex;
                // Try to find the location to insert the new record by comparing the timestamps
                for (insertionIndex = 0; insertionIndex < HistoricalDataPoints.Count && HistoricalDataPoints[insertionIndex].Timestamp < record.Timestamp; insertionIndex++) { }

                // If insertionIndex falls to the end, just append a new entry
                if (insertionIndex == HistoricalDataPoints.Count)
                {
                    var newFrame = new MarketDataFrame(record.Timestamp);
                    newFrame.AddPriceData(security, record);
                    HistoricalDataPoints.Add(newFrame);
                }
                // timestamp already exists
                else if (HistoricalDataPoints[insertionIndex].Timestamp == record.Timestamp)
                {
                    HistoricalDataPoints[insertionIndex].AddPriceData(security, record);
                }
                // insert a new timestamp
                else
                {
                    var newFrame = new MarketDataFrame(record.Timestamp);
                    newFrame.AddPriceData(security, record);
                    HistoricalDataPoints.Insert(insertionIndex, newFrame);
                }
            }

            CurrentDataEnumerator = HistoricalDataPoints.GetEnumerator();
            CurrentDataEnumerator.MoveNext();
        }

        private IEnumerable<MarketDataPoint> ReadCsvFile(string filename, string security, string baseCurrency)
        {
            var rows = File.ReadAllLines(filename);
            var header = rows[0].Split(',');

            var dateIndex = Array.FindIndex(header, h => h == "Date");
            var priceIndex = Array.FindIndex(header, h => h == "Close");

            foreach (var row in rows.Skip(1))
            {
                var tokens = row.Split(',');
                yield return new MarketDataPoint()
                {
                    Security = security,
                    BaseCurrency = baseCurrency,
                    Timestamp = DateTime.Parse(tokens[dateIndex]),
                    Price = decimal.Parse(tokens[priceIndex])
                };
            }
        }
    }
}
