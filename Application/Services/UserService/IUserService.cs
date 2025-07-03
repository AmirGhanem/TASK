using Application.Common.User.Queries.GetUser;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.UserService
{
    public interface IUserService
    {
        public  Task<string> GetSecurityStamp(string Id);
        public Task<string> GenerateResetCodeAsync(string userId);
        public Task<bool> VerifyResetCodeAsync(string userId, string code);

        public Task<BaseCommandResult> AssignUserToRolesAsync(string userId, List<string> RolesIds);

        public Task<ApplicationUser> GetCurrentUserAsync();
        public Task<bool> CheckAdminPermissionAsync();

    }
}
