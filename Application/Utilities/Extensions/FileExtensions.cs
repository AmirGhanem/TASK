using Domain.Contractors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Extensions
{
    internal static class FileExtensions
    {
        internal static string GetUploadDirectory<T>(this IModelBase<T> modelBase)
        {
            var dir = Path.GetFullPath(".\\wwwroot");
            string dirPath = dir + @$"\\Uploads\\{modelBase?.Id?.ToString()}\\";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return dirPath;
        }
    }
}
