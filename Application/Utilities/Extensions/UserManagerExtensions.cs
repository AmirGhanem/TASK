using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<List<string>> ValidatePasswordAsync<TUser>(
            this UserManager<TUser> userManager, string newPassword)
            where TUser : class
        {
            var passwordErrors = new List<string>();
            var validators = userManager.PasswordValidators;

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(userManager, null, newPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        passwordErrors.Add(error.Description);
                    }
                }
            }

            return passwordErrors;
        }
    }
}
