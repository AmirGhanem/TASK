using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application.Utilities.Models;
using Domain.Contractors;
using Domain.Identity;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Contractors
{
    public interface IFileService
    {
        Task<UploadFileResult> UploadFilesAsync(string Id, IFormFileCollection files);
        Task<GetFilesResult> GetFilesUrlAsync(string Id);
        Task DeleteFilesAsync(string Id);

        Task DeleteFileAsync(string Url);
        Task<DownloadFileResult> DownloadFileAsync(string fileName);
    }

}
