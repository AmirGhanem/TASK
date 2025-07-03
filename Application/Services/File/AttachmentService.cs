using Microsoft.AspNetCore.Http;
using Application.Utilities.Contractors;
using Application.Utilities.Models;
using Domain.Contractors;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.File
{
    public sealed class AttachmentService
    {
        private readonly IFileService _fileService;

        public AttachmentService(IFileService fileService)
        {
            this._fileService = fileService;
        }
        public async Task DeleteFilesAsync(string Id)
        {
            await _fileService.DeleteFilesAsync(Id);
        }
        public async Task<UploadFileResult> UploadFilesAsync(string Id, IFormFileCollection files)
        {
            return await _fileService.UploadFilesAsync(Id,files);
        }
        public async Task<GetFilesResult> GetFilesUrlAsync(string Id)
        {
            return await _fileService.GetFilesUrlAsync(Id);
        }
        public async Task<DownloadFileResult> DownloadFileAsync(string fileName)
        {
            return await _fileService.DownloadFileAsync(fileName);
        }
        public async Task DeleteFileAsync(string Url)
        {
            await _fileService.DeleteFileAsync(Url);
        }
    }
}
