using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Services.Email;
using Application.Services.Notification;
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

namespace Application.Common.User.Commands.SendConfirmEmail
{
    public record SendConfirmEmailCommandResult : BaseCommandResult
    {
        public string token { get; set; }
    }
    public record SendConfirmEmailCommand : IRequest<SendConfirmEmailCommandResult>
    {
        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = nameof(ErrorCode.InvalidEmailAddress))]
        public string Email { get; set; }

    }
    public sealed class SendConfirmEmailCommandHandler : IRequestHandler<SendConfirmEmailCommand, SendConfirmEmailCommandResult>
    {
        readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SendConfirmEmailCommandHandler> _logger;
        NotificationSender notification;

        public SendConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager, EmailSender notificationSender, ILogger<SendConfirmEmailCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
            notification = new NotificationSender(notificationSender);

        }
        public async Task<SendConfirmEmailCommandResult> Handle(SendConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new SendConfirmEmailCommandResult
                {
                    ErrorCode = ErrorCode.UserNotFound,
                    IsSuccess = false
                };
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (token == null)
            {
                return new SendConfirmEmailCommandResult
                {
                    ErrorCode = ErrorCode.Error,
                    IsSuccess = false
                };
            }

            //await notification.SendAsync(user.Email, "Confirm Froget Password", token);// dont forget to add link with token

            return new SendConfirmEmailCommandResult
            {
                IsSuccess = true,
                token = token
            };
        }
    }
}
