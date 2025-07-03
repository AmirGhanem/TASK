using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Application.Common.User.Commands.ChangePassword;
using Application.Utilities.Extensions;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Common.Attributes;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.SetPassword
{
    public record SetPasswordCommandResult : BaseCommandResult
    {

    }
    public record SetPasswordCommand : IRequest<SetPasswordCommandResult>
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = nameof(ErrorCode.FieldRequired))]
        [NonEmptyGuid(ErrorMessage = nameof(ErrorCode.FieldRequired))]
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = nameof(ErrorCode.FieldRequired))]
        public string NewPassword { get; set; }
    }
    public sealed class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, SetPasswordCommandResult>
    {
        readonly UserManager<ApplicationUser> _userManager;

        public SetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<SetPasswordCommandResult> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) { return new SetPasswordCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.UserNotFound }; }
            else
            {
                var passwordErrors = await _userManager.ValidatePasswordAsync(request.NewPassword);
                if (passwordErrors.Any())
                {
                    return new SetPasswordCommandResult { ErrorCode = ErrorCode.InvalidPasswordRequirements, Errors = passwordErrors };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (token == null) return new SetPasswordCommandResult { ErrorCode = ErrorCode.Error, Errors = { "failed generate reset password token" } };
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                if (result == IdentityResult.Success)
                {
                    user.SecurityStamp = Guid.NewGuid().ToString("D");
                    await _userManager.UpdateSecurityStampAsync(user);
                    return new SetPasswordCommandResult() { IsSuccess = true };
                }
                return new SetPasswordCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.Error, Errors = result.Errors.Select(x => x.Description).ToList() };
            }

        }
    }
}
