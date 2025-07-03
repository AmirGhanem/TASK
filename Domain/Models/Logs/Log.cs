using Domain.Models.Contractors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Logs
{
    public class Log:ModelBase<Guid>
    {
        public string TableName { get; set; } = string.Empty;
        public string? ColomnId { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ActionBy { get; set; } = string.Empty;
    }
}
