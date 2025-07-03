using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Services.Email;
using Application.Services.Notification;
using Application.Services.UserService;
using Application.Common.User.Commands.ChangePassword;
using Application.Common.User.Commands.ForgotPassword;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.ForgotPasswordConfirmation
{

    public record ForgotPasswordConfirmationCommandResult : BaseCommandResult
    {

    }
    public record ForgotPasswordConfirmationCommand : IRequest<ForgotPasswordConfirmationCommandResult>
    {

        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public sealed class ForgotPasswordConfirmationCommandHandler : IRequestHandler<ForgotPasswordConfirmationCommand, ForgotPasswordConfirmationCommandResult>
    {
        readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ForgotPasswordConfirmationCommandHandler> _logger;
        NotificationSender notification;
        private readonly IUserService _userService;

        public ForgotPasswordConfirmationCommandHandler(UserManager<ApplicationUser> userManager, IUserService userService, EmailSender notificationSender, ILogger<ForgotPasswordConfirmationCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
            notification = new NotificationSender(notificationSender);
            _userService = userService;
        }

        public async Task<ForgotPasswordConfirmationCommandResult> Handle(ForgotPasswordConfirmationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new ForgotPasswordConfirmationCommandResult
                    {
                        ErrorCode = ErrorCode.UserNotFound,
                        IsSuccess = false
                    };
                }
                var isTokenValid = await _userService.VerifyResetCodeAsync(user.Id, request.Token);
                if (!isTokenValid)
                {
                    return new ForgotPasswordConfirmationCommandResult
                    {
                        ErrorCode = ErrorCode.Expired,
                        IsSuccess = false
                    };
                }
                string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

                if (!result.Succeeded)
                {
                    return new ForgotPasswordConfirmationCommandResult
                    {
                        ErrorCode = ErrorCode.Error,
                        IsSuccess = false,
                        Errors = result.Errors.Select(x => x.Description).ToList()
                    };
                }
                return new ForgotPasswordConfirmationCommandResult { IsSuccess = true };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new ForgotPasswordConfirmationCommandResult
                {
                    ErrorCode = ErrorCode.Error,
                    IsSuccess = false
                };
            }
        }

    }
}
