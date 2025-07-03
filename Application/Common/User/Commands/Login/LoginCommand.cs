using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Duende.IdentityServer.Extensions;
using Domain.Common.Constants;
using Application.Utilities.Models;

namespace Application.Common.User.Commands.Login
{
    public record LoginCommandResult : BaseCommandResult
    {

        public bool Need2FA { get; set; }
        public bool IsLockout { get; set; }
        public bool IsNotAllowed { get; set; }
        public string Token { get; set; } = string.Empty;

        

    }

    public record LoginCommand : IRequest<LoginCommandResult>
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? DeviceId { get; set; }
    }

    [AllowAnonymous]
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly JWT _jwt;
        private readonly IHttpContextAccessor _httpContext;
        public LoginCommandHandler(SignInManager<ApplicationUser> signInManager, ILogger<LoginCommandHandler> logger, IOptions<JWT> jwt, IHttpContextAccessor httpContext)
        {
            _signInManager = signInManager;
            _logger = logger;
            _httpContext = httpContext;
            _jwt = jwt.Value;
        }



        public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var model = new LoginCommandResult();
                var User = await _signInManager.UserManager.Users.FirstOrDefaultAsync(x => x.Email!.ToLower().Trim().Equals(request.UserName.ToLower().Trim())
                || x.UserName.Trim().ToLower().Equals(request.UserName.Trim().ToLower())
                || x.PhoneNumber.Trim().Equals(request.UserName.Trim().ToLower())
                );
                if (User == null) return new LoginCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.UserNotFound };
               
                var result = await _signInManager.CheckPasswordSignInAsync(User, request.Password, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    await _signInManager.UserManager.UpdateAsync(User);
                    _logger.LogInformation("User logged in.");

                    var token = await CreateJwtToken(User);

                    model.IsSuccess = true;
                    model.Token = new JwtSecurityTokenHandler().WriteToken(token);


                    
                    return model;
                }
                if (result.RequiresTwoFactor)
                {
                    return new LoginCommandResult() { IsSuccess = false, Need2FA = true };
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return new LoginCommandResult() { IsSuccess = false, IsLockout = true };
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User account Not Allowed.");
                    return new LoginCommandResult() { IsSuccess = false, IsNotAllowed = true };
                }
                else
                {
                    return new LoginCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.InvalidLogin };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new LoginCommandResult() { IsSuccess = false, ErrorCode = ErrorCode.Error ,Errors = { ex.Message} };
            }
        }

        async Task<List<Claim>> GetUserClaims(ApplicationUser user)
        {
            var result = await _signInManager.UserManager.GetClaimsAsync(user);
            return result.ToList();
        }
        async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            var result = await _signInManager.UserManager.GetRolesAsync(user);
            return result.ToList();
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await GetUserClaims(user); //await _signInManager.UserManager.GetClaimsAsync(user);
            var roles = await GetUserRoles(user); //await _signInManager.UserManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            foreach (var role in roles)
            {
                roleClaims.Add(new Claim("roles", role));
            }
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim("uss",user.SecurityStamp)
            }
            .Union(userClaims)
            .Union(roleClaims);
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                    signingCredentials: signingCredentials
                );

            return jwtSecurityToken;
        }





    }

    public class JWT
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int DurationInDays { get; set; }
    }

}
