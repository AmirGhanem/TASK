using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.Utilities.Contractors;
using Application.Services.Email;
using Application.Common.User.Commands.ForgotPasswordConfirmation;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.ConfirmEmail
{
    public record ConfirmEmailCommandResult : BaseCommandResult
    {
    }
    public record ConfirmEmailCommand : IRequest<ConfirmEmailCommandResult>
    {
        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = nameof(ErrorCode.InvalidEmailAddress))]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
    }
    public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailCommandResult>
    {
        readonly UserManager<ApplicationUser> _userManager;
        public ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<ConfirmEmailCommandResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) { return new ConfirmEmailCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.UserNotFound }; }
            else
            {
                var isTokenValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "EmailConfirmation", request.Code);
                if (!isTokenValid)
                {
                    return new ConfirmEmailCommandResult
                    {
                        ErrorCode = ErrorCode.Expired,
                        IsSuccess = false
                    };
                }
                var result = await _userManager.ConfirmEmailAsync(user, request.Code);
                if (result == IdentityResult.Success)
                {
                    return new ConfirmEmailCommandResult() { IsSuccess = true };
                }
                return new ConfirmEmailCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.Error };
            }

        }
    }
}
