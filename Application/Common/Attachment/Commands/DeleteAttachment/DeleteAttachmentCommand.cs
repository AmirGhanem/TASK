using MediatR;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;

using Application.Services.File;
using Application.Services.UserService;
using Application.Utilities.Contractors;
using Application.Utilities.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Attachment.Commands.DeleteAttachment
{
    public record DeleteAttachmentCommandResult : BaseCommandResult
    {

    }
    public record DeleteAttachmentCommand : IRequest<DeleteAttachmentCommandResult>
    {
        public string Url { get; set; }
    }

    public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, DeleteAttachmentCommandResult>
    {
        private readonly ILogger<DeleteAttachmentCommandHandler> _logger;
        private readonly IFileService attachmentService;
        public DeleteAttachmentCommandHandler(LocalFiletService attachmentService,
            ILogger<DeleteAttachmentCommandHandler> logger)
        {
            this.attachmentService = attachmentService;
            _logger = logger;
        }

        public async Task<DeleteAttachmentCommandResult> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dir = Path.GetFullPath(".\\wwwroot");
                string filePath = dir + @$"\\{request.Url}";
                
                if (!System.IO.File.Exists(filePath))
                {
                    return new DeleteAttachmentCommandResult()
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
#if DEBUG || LOCAL
                        Errors = { $"Url Not Found." }
#endif
                    };
                }
                await attachmentService.DeleteFileAsync(filePath);
                return new DeleteAttachmentCommandResult()
                {
                    IsSuccess = true
                };
            }
            catch (Exception error)
            {
                return new DeleteAttachmentCommandResult()
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
#if DEBUG || LOCAL
                    Errors = { error.Message }
#endif
                };
            }
        }
    }
}
