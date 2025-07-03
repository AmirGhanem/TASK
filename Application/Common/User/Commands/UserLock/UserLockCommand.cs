using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Common.User.Commands.UpdateUser;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.UserLock
{
    public record UserLockCommandResult : BaseCommandResult
    {

    }
    public record UserLockCommand : IRequest<UserLockCommandResult>
    {
        [Required]
        public string Id { get; set; }
        public DateTime? Date { get; set; } = DateTime.UtcNow.AddYears(10);
    }


    public sealed class UserLockCommandHandler : IRequestHandler<UserLockCommand, UserLockCommandResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserLockCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLockCommandHandler(UserManager<ApplicationUser> userManager, ILogger<UserLockCommandHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<UserLockCommandResult> Handle(UserLockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.Id);
                if (user == null)
                {
                    return new UserLockCommandResult()
                    {
                        ErrorCode = ErrorCode.UserNotFound,
                        IsSuccess = false
                    };
                }

                var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (userId == request.Id)
                {
                    return new UserLockCommandResult
                    {
                        ErrorCode = ErrorCode.CantLockYourself,
                        IsSuccess = false
                    };
                }

                user.LockoutEnd = request.Date ?? DateTime.Now.AddYears(10);
                user.ModifiedDate = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new UserLockCommandResult
                    {
                        ErrorCode = ErrorCode.Error,
                        IsSuccess = false
                    };
                }
                var sResult = await _userManager.UpdateSecurityStampAsync(user);

                return new UserLockCommandResult()
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UserLockCommandResult()
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
