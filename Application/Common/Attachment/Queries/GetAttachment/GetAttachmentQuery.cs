using Application.Common.Attachment.Commands.UploadAttachment;
using Application.Services.File;
using Application.Utilities.Contractors;
using Application.Utilities.Models;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Attachment.Queries.GetAttachment
{
    public record GetAttachmentQueryResult : DownloadFileResult
    {

    }
    public record GetAttachmentQuery : IRequest<GetAttachmentQueryResult>
    {
        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired))]
        public string Url { get; set; }
    }
    public class GetAttachmentQueryHandler : IRequestHandler<GetAttachmentQuery, GetAttachmentQueryResult>
    {
        private readonly ILogger<GetAttachmentQueryHandler> _logger;
        private readonly IFileService attachmentService;
        public GetAttachmentQueryHandler(LocalFiletService attachmentService,
            ILogger<GetAttachmentQueryHandler> logger)
        {
            this.attachmentService = attachmentService;
            _logger = logger;
        }

        public async Task<GetAttachmentQueryResult> Handle(GetAttachmentQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await attachmentService.DownloadFileAsync(request.Url);
                if (!result.IsSuccess)
                {
                    return new GetAttachmentQueryResult()
                    {
                        IsSuccess = result.IsSuccess,
                        ErrorCode= result.ErrorCode,
#if DEBUG
                        Errors= result.Errors
#endif
                    };
                }
                return new GetAttachmentQueryResult()
                {
                    IsSuccess = result.IsSuccess,
                    DownloadFile = result.DownloadFile
                };
            }
            catch (Exception error)
            {
                return new GetAttachmentQueryResult()
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
