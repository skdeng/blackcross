using System;

namespace BlackCross.Core.Markets
{
    /// <summary>
    /// Exception indicating that market data feed has been interrupted
    /// </summary>
    public class DataFeedInterruptedException : Exception
    {
        /// <summary>
        /// Name of the data feed
        /// </summary>
        public string DataFeedName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataFeedName">Name of the data feed</param>
        public DataFeedInterruptedException(string dataFeedName)
        {
            DataFeedName = dataFeedName;
        }
    }
}
