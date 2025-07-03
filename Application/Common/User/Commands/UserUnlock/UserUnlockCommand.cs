using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.UserUnlock
{
    public record UserUnlockCommandResult : BaseCommandResult
    {

    }
    public record UserUnlockCommand : IRequest<UserUnlockCommandResult>
    {
        [Required]
        public string Id { get; set; }
    }


    public sealed class UserUnlockCommandHandler : IRequestHandler<UserUnlockCommand, UserUnlockCommandResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserUnlockCommandHandler> _logger;

        public UserUnlockCommandHandler(UserManager<ApplicationUser> userManager, ILogger<UserUnlockCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<UserUnlockCommandResult> Handle(UserUnlockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.Id);
                if (user == null)
                {
                    return new UserUnlockCommandResult()
                    {
                        ErrorCode = ErrorCode.UserNotFound,
                        IsSuccess = false
                    };
                }
                user.LockoutEnd = null;
                user.ModifiedDate = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new UserUnlockCommandResult
                    {
                        ErrorCode = ErrorCode.Error,
                        IsSuccess = false,
                        Errors = result.Errors.Select(x => x.Description).ToList()
                    };
                }


                return new UserUnlockCommandResult()
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UserUnlockCommandResult()
                {
                    ErrorCode = ErrorCode.Error,
                    IsSuccess = false,
#if DEBUG
                    Errors = { ex.Message }
#endif
                };
            }
        }
    }
}
