using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Application.Services.UserService;
using Application.Common.User.Commands.UpdateUserRoles;
using Application.Common.User.Queries.GetUser;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Common.Constants;
using Domain.Identity;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.UserService
{
    public sealed class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMemoryCache _memoryCache;

        public UserService(ApplicationDbContext dbContext,UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor httpContext,
            IMemoryCache memoryCache)
        {
            this._dbContext = dbContext;
            this._userManager = userManager;
            this._httpContext = httpContext;
            _memoryCache = memoryCache;

        }
        public async Task<string> GetSecurityStamp(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return "";
            }
            return user.SecurityStamp;
        }
        public async Task<BaseCommandResult> AssignUserToRolesAsync(string userId, List<string> Roles)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new BaseCommandResult() { ErrorCode = Domain.Common.ErrorCode.NotFound, Errors = { "User not found." }, IsSuccess = false };
                }
                _dbContext.UserRoles.RemoveRange(_dbContext.UserRoles.Where(u => u.UserId == userId));
                if (Roles != null && Roles.Any())
                {
                    foreach (var roleName in Roles)
                    {
                        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                        if (role == null)
                        {
                            return new BaseCommandResult() { ErrorCode = Domain.Common.ErrorCode.NotFound, Errors = { $"role {role} not found." }, IsSuccess = false };
                        }
                        var userroles = new IdentityUserRole<string>
                        {
                            UserId = userId,
                            RoleId = role.Id
                        };
                        await _dbContext.UserRoles.AddAsync(userroles);
                    }
                }
                await _userManager.UpdateSecurityStampAsync(user);
                await _dbContext.SaveChangesAsync();
                return new BaseCommandResult() { IsSuccess = true };
            }
            catch(Exception ex)
            {
                return new BaseCommandResult() { ErrorCode = Domain.Common.ErrorCode.NotFound, IsSuccess = false,
#if DEBUG
                    Errors = { ex.Message }
#endif
                };
            }
        }
        public async Task<string> GenerateResetCodeAsync(string userId)
        {
            Random random = new Random();
            int resetCode = random.Next(100000, 1000000);

            // Store the code in MemoryCache
            string cacheKey = $"ResetCode_{userId}";
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _memoryCache.Set(cacheKey, resetCode.ToString(), cacheEntryOptions);

            return resetCode.ToString();
        }

        public async Task<bool> VerifyResetCodeAsync(string userId, string code)
        {
            return await Task.Run(() =>
            {
                string cacheKey = $"ResetCode_{userId}";
                if (_memoryCache.TryGetValue(cacheKey, out string storedCodeAndToken))
                {
                    // Compare the provided code and token with the stored values
                    return code == storedCodeAndToken;
                }
                return false;
            });
        }
        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = _httpContext.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            return await _userManager.FindByIdAsync(userId);
        }
      

        public async Task<bool> CheckAdminPermissionAsync()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, Domain.Common.Constants.Roles.ADMIN) ;
        }
    }
}
