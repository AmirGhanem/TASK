using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Application.Common.User.Commands.ConfirmEmail;
using Application.Common.User.Commands.Register;
using Application.Utilities.Extensions;
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
using System.Windows.Input;

namespace Application.Common.User.Commands.ChangePassword
{
    public record ChangePasswordCommandResult : BaseCommandResult
    {

    }
    public record ChangePasswordCommand : IRequest<ChangePasswordCommandResult>
    {

        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }


    }
    public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordCommandResult>
    {
        readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;

        public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _httpContext = httpContext;
        }
        public async Task<ChangePasswordCommandResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContext.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return new ChangePasswordCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.UserNotFound }; }
            else
            {
                var passwordErrors = await _userManager.ValidatePasswordAsync(request.NewPassword);
                if (passwordErrors.Any())
                {
                    return new ChangePasswordCommandResult { ErrorCode = ErrorCode.InvalidPasswordRequirements, Errors = passwordErrors };
                }
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (result == IdentityResult.Success)
                {
                    user.SecurityStamp = Guid.NewGuid().ToString("D");
                    await _userManager.UpdateSecurityStampAsync(user);
                    return new ChangePasswordCommandResult() { IsSuccess = true };
                }
                return new ChangePasswordCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.Error , Errors = result.Errors.Select(e => e.Description).ToList() };
            }

        }
    }
}
