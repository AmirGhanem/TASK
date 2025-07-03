using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Extensions
{
    public static class NumericExtensions
    {
        public static double CalculatePercentage<T>(this T numerator, T denominator) where T : struct, IComparable<T>
        {
            if (numerator.CompareTo(default(T)) == 0 || denominator.CompareTo(default(T)) == 0)
            {
                return 0;
            }
            else
            {
                return (Convert.ToDouble(numerator) / Convert.ToDouble(denominator)) * 100.0;
            }
        }
    }
}
