using Microsoft.AspNetCore.Http;
using Application.Utilities.Contractors;
using Application.Utilities.Extensions;
using Application.Utilities.Models;
using Domain.Contractors;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.File
{
    public class LocalFiletService : IFileService
    {
        public async Task<DownloadFileResult> DownloadFileAsync(string Url)
        {
            try
            {
                // Check if the file exists
                if (!System.IO.File.Exists(Url))
                {
                    return new DownloadFileResult()
                    {
                        IsSuccess=false,
                        ErrorCode=Domain.Common.ErrorCode.NotFound,
#if DEBUG
                        Errors = { "File Not Found." },
#endif
                    };
                }

                // Read the file contents as a byte array
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(Url);

                return new DownloadFileResult() { 
                IsSuccess = true,
                 DownloadFile=new DownloadFile() {  Stream=fileBytes, Url=Url}
                };
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                Console.WriteLine($"Error downloading file: {ex.Message}");
                throw;
            }
        }
        public async Task DeleteFilesAsync(string Id)
        {
            string path = GetUploadDirectory(Id);
            if (Directory.Exists(path))
            {
                await Task.Run(() => { Directory.Delete(path, true); });
            }
        }
        public async Task<GetFilesResult> GetFilesUrlAsync(string Id)
        {
            var path = Path.Combine("wwwroot", GetUploadDirectory(Id));

            if (Directory.Exists(path))
            {
                var fileUrls = Directory.GetFiles(path).Select(file =>
                {
                    var relativePath = Path.GetRelativePath("wwwroot", file);
                    return "/" + relativePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }).ToList();

                return new GetFilesResult { Urls = fileUrls, IsSuccess = true };
            }

            return new GetFilesResult { IsSuccess = false };
        }


        public async Task<UploadFileResult> UploadFilesAsync(string Id, IFormFileCollection files)
        {
            try
            {
                List<UploadFile> results = new List<UploadFile>();
                foreach (var file in files)
                {
                    if (file == null || file.Length == 0)
                        return new UploadFileResult() { ErrorCode = Domain.Common.ErrorCode.BadFile, Errors = { "Bad file" }, IsSuccess = false };
                    var filePath = Path.Combine("wwwroot", GetUploadDirectory(Id) +Guid.NewGuid().ToString() + "_" + file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    results.Add(new UploadFile { Url=filePath,FileName= Path.GetFileName(filePath) } );
                }
                return new UploadFileResult() { IsSuccess = true, UploadFiles = results };
            }
            catch(Exception error)
            {
                return new UploadFileResult() { ErrorCode = Domain.Common.ErrorCode.Error,
#if DEBUG
                    Errors = { error.Message},
#endif
                    IsSuccess = false };
            }

        }

        public async Task DeleteFileAsync(string Url)
        {
            if (System.IO.File.Exists(Url))
            {
                await Task.Run(() => { System.IO.File.Delete(Url); });
            }
        }

        internal string GetUploadDirectory(string Id)
        {
            var dir = Path.GetFullPath(".\\wwwroot");
            string dirPath = dir + @$"\\Uploads\\{Id.ToString()}\\";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return dirPath;
        }
    }
}
