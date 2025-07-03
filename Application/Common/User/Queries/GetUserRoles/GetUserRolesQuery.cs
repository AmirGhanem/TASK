using Application.Services.UserService;
using Application.Utilities.Models;
using Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Queries.GetUserRoles
{
    public record GetUserRolesQueryResult : BaseCommandResult
    {
        public List<string> Roles { get;  set; }
    }
    public record GetUserRolesQuery:IRequest<GetUserRolesQueryResult>
    {
        public string? Id { get; set; }
    }
    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, GetUserRolesQueryResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public GetUserRolesQueryHandler(UserManager<ApplicationUser> userManager,IUserService userService)
        {
            this._userManager = userManager;
            this._userService = userService;
        }
        public async Task<GetUserRolesQueryResult> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = request.Id != null ? await _userManager.FindByIdAsync(request.Id): await _userService.GetCurrentUserAsync();
                if(user == null)
                {
                    return new GetUserRolesQueryResult
                    {
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
                        Errors = { "User Not Found" },
                        IsSuccess = false
                    };
                }
                var roles = await _userManager.GetRolesAsync(user);
                return new GetUserRolesQueryResult
                {
                    IsSuccess = true,
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                return new GetUserRolesQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
