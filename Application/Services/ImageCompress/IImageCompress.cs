using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ImageCompress
{
    public interface IImageCompress
    {
        public Task Compress(IFormFile Image, string outputPath, int quality = 75);
    }
}
