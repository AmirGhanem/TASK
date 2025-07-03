using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public record BaseCommandResult
    {
        public bool IsSuccess { get; set; }
        public ErrorCode ErrorCode { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }
}
