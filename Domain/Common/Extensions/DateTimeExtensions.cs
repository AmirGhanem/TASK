using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static int MonthDiff(this DateTime startDate, DateTime endDate)
        {
            int monthsDifference = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
            return Math.Abs(monthsDifference); // Take the absolute value
        }
    }

}
