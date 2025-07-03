using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Common.User.Queries.GetUser;
using Application.Utilities.Models;
using Domain.Common.Constants;
using Domain.Identity;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.User.Queries.GetRoles
{
    public record GetRolesQueryResult : BaseCommandResult
    {
        public List<IdentityRole> Roles { get; set; }
    }
    public record GetRolesQuery : IRequest<GetRolesQueryResult>
    {
    }
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, GetRolesQueryResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GetRolesQueryHandler> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GetRolesQueryHandler(UserManager<ApplicationUser> userManager, ILogger<GetRolesQueryHandler> logger, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<GetRolesQueryResult> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                    var roles = _roleManager.Roles.ToList();
                    return new GetRolesQueryResult
                    {
                        IsSuccess = true,
                        Roles = roles
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new GetRolesQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
