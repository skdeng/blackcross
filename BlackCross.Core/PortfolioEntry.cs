namespace BlackCross.Core
{
    public class PortfolioEntry
    {
        public decimal TotalQuantity => FreeQuantity + FrozenQuantity;

        public decimal FreeQuantity { get; set; }

        public decimal FrozenQuantity { get; set; }
    }
}
