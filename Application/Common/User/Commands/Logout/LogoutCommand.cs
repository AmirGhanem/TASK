using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Common.User.Commands.Login;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.Logout
{
    public record LogoutCommandResult : BaseCommandResult
    {

    }


    public record LogoutCommand : IRequest<LogoutCommandResult>
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = nameof(ErrorCode.FieldRequired))]
        public string Id { get; set; }
    }

    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutCommandResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        public LogoutCommandHandler(UserManager<ApplicationUser> userManager, ILogger<LoginCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<LogoutCommandResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.Id);
                if (user == null)
                {
                    return new LogoutCommandResult
                    {
                        ErrorCode = ErrorCode.NotFound,
                        IsSuccess = false
                    };
                }
                await _userManager.UpdateSecurityStampAsync(user);
                await _userManager.UpdateAsync(user);
                return new LogoutCommandResult
                {
                    IsSuccess = true
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new LogoutCommandResult
                {
                    ErrorCode = ErrorCode.Error,
                    IsSuccess = false
                };
            }
        }
    }

}
