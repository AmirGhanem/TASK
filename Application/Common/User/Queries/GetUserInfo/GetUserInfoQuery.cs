using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Common.User.Queries.GetUser;
using Application.Utilities.Models;
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

namespace Application.Common.User.Queries.GetUserInfo
{
    public record GetUserInfoQueryResult : BaseCommandResult
    {
        public UserInfo UserInfo { get; set; }
    }
    public class GetUserInfoQuery : IRequest<GetUserInfoQueryResult>
    {
        [Required]
        public string? Id { get; set; }
    }
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

    }
    public sealed class GetUserHandler : IRequestHandler<GetUserInfoQuery, GetUserInfoQueryResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<GetUserHandler> _logger;
        private readonly IHttpContextAccessor _httpContext;

        public GetUserHandler(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<GetUserHandler> logger, IHttpContextAccessor httpContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContext = httpContext;
            _userManager = userManager;
        }

        public async Task<GetUserInfoQueryResult> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = request.Id ?? _httpContext.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
              
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new GetUserInfoQueryResult
                    {
                        ErrorCode = Domain.Common.ErrorCode.UserNotFound,
                        IsSuccess = false,
                        UserInfo = null
                    };
                }
                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = new UserInfo
                {
                    Email = user.Email,
                    Name = user.Name,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                };
                return new GetUserInfoQueryResult
                {

                    IsSuccess = true,
                    UserInfo = userInfo
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                return new GetUserInfoQueryResult
                {
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    IsSuccess = false
                };
            }
        }
    }
}
