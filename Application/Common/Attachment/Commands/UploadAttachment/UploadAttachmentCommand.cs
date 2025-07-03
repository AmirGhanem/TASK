using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Application.Services.File;
using Application.Utilities.Contractors;
using Application.Utilities.Models;
using Domain.Common;

namespace Application.Common.Attachment.Commands.UploadAttachment
{
    public record UploadAttachmentCommandResult : BaseCommandResult
    {

    }
    public record UploadAttachmentCommand : IRequest<UploadAttachmentCommandResult>
    {
        [NonEmptyGuid(ErrorMessage = nameof(ErrorCode.FieldRequired))]
        public Guid Id { get; set; }
        public IFormFileCollection Files { get; internal set; } = new FormFileCollection();
    }

    public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, UploadAttachmentCommandResult>
    {
        private readonly ILogger<UploadAttachmentCommandHandler> _logger;
        private readonly IFileService attachmentService;
        public UploadAttachmentCommandHandler(LocalFiletService attachmentService,
            ILogger<UploadAttachmentCommandHandler> logger)
        {
            this.attachmentService = attachmentService;
            _logger = logger;
        }

        public async Task<UploadAttachmentCommandResult> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result=await attachmentService.UploadFilesAsync(request.Id.ToString(), request.Files);
                if (!result.IsSuccess)
                {
                    return new UploadAttachmentCommandResult()
                    {
                        IsSuccess = result.IsSuccess,
                        ErrorCode = result.ErrorCode,
#if DEBUG
                        Errors = result.Errors
#endif
                    };
                }
                return new UploadAttachmentCommandResult()
                {
                    IsSuccess = true
                };
            }
            catch (Exception error)
            {
                return new UploadAttachmentCommandResult()
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCode.Error,
#if DEBUG || LOCAL
                    Errors = { error.Message }
#endif
                };
            }
        }
    }
}
