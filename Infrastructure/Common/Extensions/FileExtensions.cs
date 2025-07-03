using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Extensions
{
    public static class FileExtensions
    {
        public static async Task<Dictionary<string, string>> GetFilesContentInFolderAsync(this Assembly assembly, string folderName)
        {
            var resourceNames = assembly.GetManifestResourceNames();
            var folderPrefix = folderName + ".";
            var fileContentDict = new Dictionary<string, string>();

            string assemblyNamespace = assembly.GetName().Name; // Automatically determine the assembly's namespace

            foreach (var resourceName in resourceNames)
            {
                if (resourceName.StartsWith(assemblyNamespace + "." + folderPrefix))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var content = await reader.ReadToEndAsync();
                                var fileName = resourceName.Substring(assemblyNamespace.Length + 1 + folderPrefix.Length);
                                fileContentDict[fileName] = content;
                            }
                        }
                    }
                }
            }

            return fileContentDict;
        }
        public static async Task<string> ReadEmbeddedResourceAsync(this Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found in assembly.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
