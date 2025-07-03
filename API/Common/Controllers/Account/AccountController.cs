using Application.Common.User.Commands.ChangePassword;
using Application.Common.User.Commands.DeleteUser;
using Application.Common.User.Commands.Logout;
using Application.Common.User.Commands.SetPassword;
using Application.Common.User.Commands.UpdateUser;
using Application.Common.User.Commands.UpdateUserRoles;
using Application.Common.User.Commands.UserLock;
using Application.Common.User.Commands.UserUnlock;
using Application.Common.User.Queries.GetRoles;
using Application.Common.User.Queries.GetUser;
using Application.Common.User.Queries.GetUserInfo;
using Application.Common.User.Queries.GetUserRoles;
using Application.Common.User.Queries.GetUsers;
using Domain.Common.Constants;
using Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Common.Controllers.Account
{
    [ApiExplorerSettings(GroupName = "Common")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(IMediator mediator, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            this.userManager = userManager;
        }
        /// <summary>
        /// Get All Users
        /// </summary>
        /// <param name="query"></param>
        /// <returns>All Users</returns>
        [HttpPost("GetAllUsers")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<GetUsersQueryResult>> Get([FromBody] GetUsersQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }




      
        
        
        
        [HttpPost("GetUserRoles")]
        [Authorize(Roles = $"{Roles.ADMIN}")]
        public async Task<ActionResult<GetUserRolesQueryResult>> GetUserRoles([FromBody] GetUserRolesQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }


       
        
        
        [HttpPost("UpdateUserRoles")]
        [Authorize(Roles = $"{Roles.ADMIN}")]
        public async Task<ActionResult<UpdateUserRolesCommandResult>> UpdateUserRoles([FromBody] UpdateUserRolesCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }
        


        /// <summary>
        /// Get User Info with specific Id
        /// </summary>
        /// <param name="query">Id for user</param>
        /// <returns>User</returns>
        [HttpGet("GetUserInfo")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<GetUserInfoQueryResult>> Get([FromQuery] GetUserInfoQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }
        /// <summary>
        /// Get User with specific Id
        /// </summary>
        /// <param name="query">Id for user</param>
        /// <returns>User</returns>
        [HttpGet("GetUser")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<GetUserQueryResult>> Get([FromQuery] GetUserQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }

        

        /// <summary>
        /// Get Current Logged-in User
        /// </summary>
        /// <returns>Current Logged-in User</returns>
        [HttpGet("GetCurrentUser")]
        
        public async Task<ActionResult<GetUserQueryResult>> GetCurrentUser()
        {

            return await this.HandleCommandResult(_mediator.Send(new GetUserQuery()));
        }
        /// <summary>
        /// Update User Profile
        /// </summary>
        /// <param name="userCommand"></param>
        /// <returns>User after taking updates</returns>
        [HttpPatch("UpdateUser")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<UpdateUserCommandResult>> UpdateUser([FromBody] UpdateUserCommand userCommand)
        {

            return await this.HandleCommandResult(_mediator.Send(userCommand));
        }
        //[HttpPatch("UpdateUserProfile")]
        //public async Task<ActionResult<UpdateUserProfileCommandResult>> UpdateUserProfile([FromBody] UpdateUserProfileCommand userCommand)
        //{

        //    return await this.HandleCommandResult(_mediator.Send(userCommand));
        //}

        /// <summary>
        /// Block user from using application
        /// </summary>
        /// <param name="userCommand">User Id and [block end date] </param>
        /// <returns></returns>
        [HttpPatch("LockUser")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<UserLockCommandResult>> LockUser([FromBody] UserLockCommand userCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(userCommand));
        }
        /// <summary>
        /// Remove user block, and can use application normally 
        /// </summary>
        /// <param name="userCommand">User Id</param>
        /// <returns></returns>
        [HttpPatch("UnLockUser")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<UserUnlockCommandResult>> LockUser([FromBody] UserUnlockCommand userCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(userCommand));
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="ChangePasswordCommand"></param>
        /// <returns></returns>
        [HttpPost("ChangeUserPassword")]
        
        public async Task<ActionResult<ChangePasswordCommandResult>> ChangePassword([FromBody] ChangePasswordCommand ChangePasswordCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(ChangePasswordCommand));
        }

        /// <summary>
        /// Set user password
        /// </summary>
        /// <param name="SetPasswordCommand"></param>
        /// <returns></returns>
        [HttpPost("SetUserPassword")]
        
        public async Task<ActionResult<SetPasswordCommandResult>> SetPassword([FromBody] SetPasswordCommand SetPasswordCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(SetPasswordCommand));
        }

        [HttpGet("GetRoles")]
        [AllowAnonymous]
        public async Task<ActionResult<GetRolesQueryResult>> GetRoles([FromQuery] GetRolesQuery GetRolesQuery)
        {
            return await this.HandleCommandResult(_mediator.Send(GetRolesQuery));
        }
        //[HttpPost("UpdateUserRoles")]
        //[Authorize(Roles = Domain.Common.Constants.Roles.ADMIN)]
        //public async Task<ActionResult<UpdateUserRolesCommandResult>> UpdateUserRoles(UpdateUserRolesCommand updateUserRoles)
        //{

        //    return await this.HandleCommandResult(_mediator.Send(updateUserRoles));
        //}

        /// <summary>
        /// Log user out and end session
        /// </summary>
        /// <returns></returns>
        [HttpGet("Logout")]
        public async Task<ActionResult<LogoutCommandResult>> Logout()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            return await this.HandleCommandResult(_mediator.Send(new LogoutCommand { Id = userId }));
        }
        
        
        
        [HttpDelete("DeleteUser")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<DeleteUserCommandResult>> DeleteUser(DeleteUserCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }
        
        
        //[HttpGet("GetNotificationCredentials")]
        //[AllowAnonymous]
        //public async Task<ActionResult> GetNotificationCredentials()
        //{
        //   var credentials = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskManager-Firebase-Key.json"));
        //    return Ok(credentials?.UnderlyingCredential);
        //}

    }
}
