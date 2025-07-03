using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Services.UserService;
using Application.Common.User.Queries.GetRoles;
using Application.Common.User.Queries.GetUser;
using Application.Utilities.Models;
using Domain.Identity;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Commands.UpdateUserRoles
{
    public record UpdateUserRolesCommandResult : BaseCommandResult
    {
    }
    public record UpdateUserRolesCommand : IRequest<UpdateUserRolesCommandResult>
    {
        public List<string> Roles { get; set; } = new List<string>();
        public string userId { get; set; }
    }
    [Authorize(Roles = Domain.Common.Constants.Roles.ADMIN)]
    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, UpdateUserRolesCommandResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<UpdateUserRolesCommandHandler> _logger;
        private readonly ApplicationDbContext _dbContext;

        public UpdateUserRolesCommandHandler(UserManager<ApplicationUser> userManager, IUserService userService, ILogger<UpdateUserRolesCommandHandler> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _userService = userService;
        }
        public async Task<UpdateUserRolesCommandResult> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {

            using (var trans = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var addUserToRoleResult = await _userService.AssignUserToRolesAsync(request.userId, request.Roles);
                    if (addUserToRoleResult.IsSuccess)
                    {
                        await trans.CommitAsync();
                        return new UpdateUserRolesCommandResult
                        {
                            IsSuccess = true
                        };
                    }
                    else
                    {
                        await trans.RollbackAsync();
                        return new UpdateUserRolesCommandResult
                        {
                            ErrorCode = Domain.Common.ErrorCode.Error,
                            IsSuccess = false,
                            Errors = addUserToRoleResult.Errors
                        };
                    }
                    //var roles = await _userManager.GetRolesAsync(user);

                    //var result = await _userManager.RemoveFromRolesAsync(user, roles);

                    //if (result.Succeeded)
                    //{
                    //    var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                    //    if (addResult.Succeeded)
                    //    {
                    //        await trans.CommitAsync();
                    //        return new UpdateUserRolesCommandResult
                    //        {
                    //            IsSuccess = true
                    //        };
                    //    }
                    //    await trans.RollbackAsync();
                    //    return new UpdateUserRolesCommandResult
                    //    {
                    //        IsSuccess = false,
                    //        Errors = addResult.Errors.Select(i => i.Description).ToList()
                    //    };
                    //}
                    //await trans.RollbackAsync();
                    //return new UpdateUserRolesCommandResult
                    //{
                    //    IsSuccess = false,
                    //    Errors = result.Errors.Select(i => i.Description).ToList()
                    //};

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    await trans.RollbackAsync();
                    return new UpdateUserRolesCommandResult
                    {
                        ErrorCode = Domain.Common.ErrorCode.Error,
                        IsSuccess = false,
                        Errors = { ex.Message }
                    };
                }
            }
        }
    }
}
