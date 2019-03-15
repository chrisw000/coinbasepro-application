using CoinbasePro.Shared.Types;

namespace CoinbasePro.Application.Extend
{
    public static class Extend
    {
        public static int DecimalPlaces(this Currency instance)
        {
            switch (instance)
            {
                case Currency.GBP:
                    return 2;
                case Currency.EUR:
                    return 2;
                case Currency.USD:
                    return 2;
                case Currency.USDC:
                    return 2;
                default:
                    return 8;
            }
        }
    }
}