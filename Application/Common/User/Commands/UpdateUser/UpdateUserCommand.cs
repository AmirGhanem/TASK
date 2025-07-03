using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Services.UserService;
using Application.Common.User.Commands.Register;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Common.Attributes;
using Domain.Identity;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Common.Constants;
using Application.Services.File;

namespace Application.Common.User.Commands.UpdateUser
{
    public record UpdateUserCommandResult : BaseCommandResult
    {

    }
    public record UpdateUserCommand : IRequest<UpdateUserCommandResult>
    {
        [Required]
        public string Id { get; set; }
        //[Trim]
        //public string? UserName { get; set;}
        [Trim]
        public string? Email { get; set; }

        [Required(ErrorMessage = nameof(ErrorCode.FieldRequired), AllowEmptyStrings = false)]

        public string Name { get; set; }



        //[RegularExpression(@"^\+?[0-9][0-9\s.-]{8,14}$", ErrorMessage =nameof(ErrorCode.InvalidPhoneNumber))]
        //[Required(ErrorMessage = nameof(ErrorCode.FieldRequired))]
        public string? PhoneNumber { get; set; }


        public IFormFileCollection? Images { get; set; }


        public List<string>? Roles { get; set; } = new List<string>();

        public bool IsActive { get; set; }
    }

    public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserCommandResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly AttachmentService _attachmentService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IUserService userService,
            IHttpContextAccessor httpContext,
            LocalFiletService localFiletService
            , ILogger<UpdateUserCommandHandler> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _userService = userService;
            _httpContext = httpContext;
            _logger = logger;
            _attachmentService = new AttachmentService(localFiletService);
        }
        public async Task<UpdateUserCommandResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var userId = request.Id ?? _httpContext.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new UpdateUserCommandResult()
                {
                    ErrorCode = ErrorCode.UserNotFound,
                    IsSuccess = false
                };
            }
            var isSystemAdmin = await  _userManager.IsInRoleAsync(user ,Roles.ADMIN) && user.UserName.Equals(Admin.ADMIN, StringComparison.OrdinalIgnoreCase);
            //if ()
            //{
            //    return new UpdateUserCommandResult()
            //    {
            //        ErrorCode = ErrorCode.AccessDenied,
            //        IsSuccess = false,
            //        Errors = { "Can not modify Admin" }
            //    };
            //}
            using (var trans = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    
                    if (!string.IsNullOrEmpty(request.Email))
                    {
                        if (await _userManager.Users.FirstOrDefaultAsync(x => x.Email.Trim().ToLower() == request.Email.Trim().ToLower() && userId != x.Id) != null)
                        {
                            await trans.RollbackAsync(cancellationToken);
                            return new UpdateUserCommandResult()
                            {
                                ErrorCode = ErrorCode.DuplicateEmail,
                                IsSuccess = false
                            };
                        }
                        user.UserName = user.Email = request.Email;
                    }
                    if (!string.IsNullOrEmpty(request.PhoneNumber) && await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == request.PhoneNumber.Trim() && userId != x.Id) != null)
                    {
                        await trans.RollbackAsync(cancellationToken);
                        return new UpdateUserCommandResult()
                        {
                            ErrorCode = ErrorCode.DuplicatePhoneNumber,
                            IsSuccess = false
                        };
                    }
                    
                    user.PhoneNumber = request.PhoneNumber;


                    //if(!isSystemAdmin) user.Name = request.Name;
                    
                    user.ModifiedDate = DateTime.UtcNow;
                    user.IsActive = request.IsActive;
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        await trans.RollbackAsync(cancellationToken);
                        return new UpdateUserCommandResult
                        {
                            ErrorCode = ErrorCode.Error,
                            IsSuccess = false,
                            Errors = result.Errors.Select(x => x.Description).ToList()
                        };
                    }
                   
                    if (request.Roles.Any())
                    {
                        if (isSystemAdmin && !request.Roles.Exists(r => r == Roles.ADMIN))
                        {
                            request.Roles.Add(Roles.ADMIN);
                        }
                        var addUserToRoleResult = await _userService.AssignUserToRolesAsync(user.Id, request.Roles);
                        if (!addUserToRoleResult.IsSuccess)
                        {
                            await trans.RollbackAsync();
                            return new UpdateUserCommandResult
                            {
                                ErrorCode = ErrorCode.Error,
                                IsSuccess = false,
                                Errors = addUserToRoleResult.Errors
                            };
                        }
                    }
                    if(request.Images != null)
                    {
                        await _attachmentService.UploadFilesAsync(Path.Combine("profiles",request.Id),request.Images);
                    }

                    await trans.CommitAsync(cancellationToken);
                    return new UpdateUserCommandResult()
                    {
                        IsSuccess = true
                    };
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync(cancellationToken);
                    _logger.LogError(ex.Message, ex);
                    return new UpdateUserCommandResult()
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
