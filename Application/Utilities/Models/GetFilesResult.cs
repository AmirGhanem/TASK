using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public record GetFilesResult : BaseCommandResult
    {
        public List<string> Urls { get; set; } = new List<string>();
    }
}
