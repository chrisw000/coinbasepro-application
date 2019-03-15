using System;
using System.Diagnostics;

namespace CoinbasePro.Application.Data
{
    public static class Constants
    {
        public static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly char Comma = ',';

        [DebuggerStepThrough]
        public static DateTime AsUtc(this DateTime instance)
        {
            return DateTime.SpecifyKind(instance, DateTimeKind.Utc);
        }

        [DebuggerStepThrough]
        public static double ProfitAfterFees(this TA4N.Trade trade)
        {
            return ProfitAfterFees(trade.Entry.Price, trade.Exit.Price);
        }

        private static double ProfitAfterFees(TA4N.Decimal buyPrice, TA4N.Decimal sellPrice)
        {
            // 0.3% applied to entry and exit
            return sellPrice.MultipliedBy(0.997).DividedBy(buyPrice.MultipliedBy(1.003)).ToDouble();
        }
    }
}
