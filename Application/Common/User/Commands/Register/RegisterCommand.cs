using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Services.Email;
using Application.Services.Notification;
using Application.Services.UserService;
using Application.Utilities.Extensions;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using Infrastructure;
using System.ComponentModel.DataAnnotations;
using Application.Common.User.Commands.Login;
using Domain.Common.Constants;

namespace Application.Common.User.Commands.Register
{
    public record RegisterCommandResult : BaseCommandResult
    {
        // remove this after setting SMTP server settings for sending email confirmation 
        public string? Token { get;  set; }
    }
    public record RegisterCommand : IRequest<RegisterCommandResult>
    {
        //[Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        //[RegularExpression("^(?=[a-zA-Z0-9._]{3,20}$)(?!.*[_.]{2})[^_.].*[^_.]$", ErrorMessage = nameof(ErrorCode.InvalidUsername))]
        //public string UserName { get; set; }

        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = nameof(ErrorCode.InvalidEmailAddress))]
        public string Email { get; set; }

        /*[Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]*/// optional
        public string? Name { get; set; }
        //[Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]// optional


        //[Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        //[Phone(ErrorMessage = nameof(ErrorCode.InvalidPhoneNumber))]
        //[RegularExpression(@"^\+?[0-9][0-9\s.-]{8,14}$", ErrorMessage = nameof(ErrorCode.InvalidPhoneNumber))]
        public string? PhoneNumber { get; set; }



      
        public List<string>? Roles { get; set; } = new List<string>() { Domain.Common.Constants.Roles.USER };

        // sentd token after register done
        //add created date to getFilteredIdeas response done
        // add parameter called IsTrending to sort by review done
        // add to police table copyrights , cookies police and contact 

        // contact form frist name , last name , email , phone number and  message 

        // translate policies 
        public bool IsActive { get; set; }

    }

    [AllowAnonymous]
    public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResult>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IMediator mediator;
        NotificationSender notification;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegisterCommandHandler(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager
            , IUserService userService
            , EmailSender notificationSender
            , IHttpContextAccessor httpContextAccessor
            , ILogger<RegisterCommandHandler> logger
            , IMediator mediator
            )

        {
            _dbContext = dbContext;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            this.mediator = mediator;
            notification = new NotificationSender(notificationSender);
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<RegisterCommandResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {            
            if (await _userManager.Users.AnyAsync(x => x.Email == request.Email))
            {
                return new RegisterCommandResult { ErrorCode = ErrorCode.DuplicateEmail };
            }
            //if (await _userManager.FindByNameAsync(request.UserName) is not null)
            //{
            //    return new RegisterCommandResult { ErrorCode = ErrorCode.DuplicateUser };
            //}
            if (await _userManager.Users.AnyAsync(x => !string.IsNullOrEmpty(request.PhoneNumber) && x.PhoneNumber == request.PhoneNumber))
            {
                return new RegisterCommandResult { ErrorCode = ErrorCode.DuplicatePhoneNumber };
            }
            var passwordErrors = await _userManager.ValidatePasswordAsync(request.Password);
            if (passwordErrors.Any())
            {
                return new RegisterCommandResult { ErrorCode = ErrorCode.InvalidPasswordRequirements, Errors = passwordErrors };
            }



            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                Name = request.Name ?? request.Email,
                PhoneNumber = request.PhoneNumber,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,

            };
           
            using (var trans = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _userManager.CreateAsync(user, request.Password);

                    if (!result.Succeeded)
                    {
                        
                        await trans.RollbackAsync();
                        return new RegisterCommandResult { IsSuccess = false, ErrorCode = ErrorCode.Error, Errors = result.Errors.Select(x => x.Description).ToList() };
                    }

                    if (request.Roles.Any())
                    {

                        var addUserToRoleResult = await _userService.AssignUserToRolesAsync(user.Id, request.Roles);
                        if (!addUserToRoleResult.IsSuccess)

                        {
                            await trans.RollbackAsync();
                            return new RegisterCommandResult
                            {
                                ErrorCode = ErrorCode.Error,
                                IsSuccess = false,
                                Errors = addUserToRoleResult.Errors
                            };
                        }
                    }
                    //var response = await mediator.Send(new SendConfirmEmail.SendConfirmEmailCommand { Email = user.Email });
                    
                    await trans.CommitAsync();
                    var loginResponse = await mediator.Send(new LoginCommand { UserName = user.Email , Password = request.Password });
                    return new RegisterCommandResult
                    {
                        IsSuccess = true
                        ,Token = loginResponse.IsSuccess ? loginResponse.Token : null
                    };



                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    await trans.RollbackAsync();
                    return new RegisterCommandResult
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
}
