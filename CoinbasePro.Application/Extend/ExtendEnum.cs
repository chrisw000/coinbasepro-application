using System;
using System.Collections.Generic;
using System.Linq;

namespace CoinbasePro.Application.Extend
{
    public static class ExtendEnum
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
