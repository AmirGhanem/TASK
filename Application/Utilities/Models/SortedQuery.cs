using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public class SortedQuery
    {
        public string PropertyName { get; set; }
        public SortDirection Direction { get; set; } = SortDirection.ASC;
    }
}
