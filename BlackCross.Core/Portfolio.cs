using System.Collections.Generic;

namespace BlackCross.Core
{
    public class Portfolio
    {
        private readonly Dictionary<string, PortfolioEntry> Securities;

        public Portfolio()
        {
            Securities = new Dictionary<string, PortfolioEntry>();
        }

        public decimal TotalAmount(string security)
        {
            if (Securities.ContainsKey(security))
            {
                return Securities[security].TotalQuantity;
            }
            else
            {
                return 0;
            }
        }

        public decimal FreeAmount(string security)
        {
            if (Securities.ContainsKey(security))
            {
                return Securities[security].FreeQuantity;
            }
            else
            {
                return 0;
            }
        }

        public void AddSecurity(string security, decimal amount)
        {
            if (Securities.ContainsKey(security))
            {
                Securities[security].FreeQuantity += amount;
            }
            else
            {
                Securities[security] = new PortfolioEntry()
                {
                    FreeQuantity = amount
                };
            }
        }

        public bool RemoveSecurity(string security, decimal amount)
        {
            if (Securities.ContainsKey(security))
            {
                if (Securities[security].FreeQuantity < amount)
                {
                    return false;
                }
                else
                {
                    Securities[security].FreeQuantity -= amount;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool FreezeSecurity(string security, decimal amount)
        {
            if (Securities[security].FreeQuantity > amount)
            {
                Securities[security].FreeQuantity -= amount;
                Securities[security].FrozenQuantity += amount;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UnfreezeSecurity(string security, decimal amount)
        {
            if (Securities[security].FrozenQuantity > amount)
            {
                Securities[security].FrozenQuantity -= amount;
                Securities[security].FreeQuantity += amount;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
