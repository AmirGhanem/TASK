using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public enum FilterType
    {
        Equals = 1,
        NotEquals,
        Range,
        Contains,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }
    public enum FilterOperator
    {
        And = 1, Or
    }
    public enum SortDirection { ASC = 1, DESC }
}
