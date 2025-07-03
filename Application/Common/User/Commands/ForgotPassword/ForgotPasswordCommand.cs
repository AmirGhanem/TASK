using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Services.Email;
using Application.Services.Notification;
using Application.Services.UserService;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.ForgotPassword
{

    public record ForgotPasswordCommandResult : BaseCommandResult
    {
        // remove this after setting SMTP server settings for sending email confirmation 
        public string Token { get; set; }
    }
    public record ForgotPasswordCommand : IRequest<ForgotPasswordCommandResult>
    {

        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = nameof(ErrorCode.InvalidEmailAddress))]
        public string Email { get; set; }

    }

    public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordCommandResult>
    {
        readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;
        NotificationSender notification;

        public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IUserService userService, EmailSender notificationSender, ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            notification = new NotificationSender(notificationSender);

        }

        public async Task<ForgotPasswordCommandResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new ForgotPasswordCommandResult
                    {
                        ErrorCode = ErrorCode.UserNotFound,
                        IsSuccess = false
                    };
                }

                //var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                //if (token == null)
                //{
                //    return new ForgotPasswordCommandResult
                //    {
                //        ErrorCode = ErrorCode.Error,
                //        IsSuccess = false
                //    };
                //}
                var code = await _userService.GenerateResetCodeAsync(user.Id);
                await notification.SendAsync(user.Email, "Confirm Froget Password", code);// dont forget to add link with token
                return new ForgotPasswordCommandResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new ForgotPasswordCommandResult
                {
                    ErrorCode = ErrorCode.Error,
                    IsSuccess = false
                };
            }
        }
    }
}
