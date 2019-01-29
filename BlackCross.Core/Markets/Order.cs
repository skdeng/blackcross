namespace BlackCross.Core
{
    public class Order
    {
        public string OrderId { get; set; }

        public string ClientOrderId { get; set; }

        public OrderType Type { get; set; }

        public OrderSide Side { get; set; }

        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public string Security { get; set; }

        public string BaseCurrency { get; set; }

        private static long ClientOrderIdCounter = 0;

        public Order()
        {
            ClientOrderId = (ClientOrderIdCounter++).ToString();
        }

        /// <summary>
        /// Print a formatted string with order information
        /// </summary>
        /// <returns>Formatted string</returns>
        public string ToFormattedString()
        {
            return $"[{Security}] {Side.ToString()} {Volume} at {Price}{BaseCurrency}";
        }
    }

    public enum OrderType
    {
        MarketOrder,
        LimitOrder,
        StopOrder
    }

    public enum OrderSide
    {
        Buy,
        Sell,
        Long = Buy,
        Short = Sell
    }
}
