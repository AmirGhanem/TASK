using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public record UploadFileResult : BaseCommandResult
    {
        public List<UploadFile> UploadFiles { get; set; } = new List<UploadFile>();
    }

    public record UploadFile
    {
        public string Url { get; set; }
        public string FileName { get; set; }
    }
}
