using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Utilities;
using Application.Common.User.Commands.ChangePassword;
using Application.Common.User.Commands.Logout;
using Application.Common.User.Queries.GetUsers;
using Domain.Identity;
using System.IdentityModel.Tokens.Jwt;
using Application.Utilities.Models;
using Application.Common.User.Commands.ConfirmEmail;
using Application.Common.User.Commands.ForgotPassword;
using Application.Common.User.Commands.ForgotPasswordConfirmation;
using Application.Common.User.Commands.Login;
using Application.Common.User.Commands.Register;
using Application.Common.User.Commands.SendConfirmEmail;
using Asp.Versioning;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Common.Controllers.Authentication
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Common")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Sign-in User
        /// </summary>
        /// <param name="loginCommand"></param>
        /// <returns>JWT tokken</returns>
        [HttpPost("Login")]
        public async Task<ActionResult<LoginCommandResult>> Login([FromBody] LoginCommand loginCommand)
        {

            return await this.HandleCommandResult(_mediator.Send(loginCommand));

        }
        /// <summary>
        /// Confirm User Email
        /// </summary>
        /// <param name="confirmEmailCommand"></param>
        /// <returns></returns>
        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult<ConfirmEmailCommandResult>> ConfirmEmail([FromQuery] ConfirmEmailCommand confirmEmailCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(confirmEmailCommand));
        }
        /// <summary>
        /// Confirm user forgot password
        /// </summary>
        /// <param name="confirmForgetPasswordCommand"></param>
        /// <returns></returns>
        [HttpPost("ConfirmForgotPassword")]
        public async Task<ActionResult<ForgotPasswordConfirmationCommandResult>> ConfirmForgetPassword([FromBody] ForgotPasswordConfirmationCommand confirmForgetPasswordCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(confirmForgetPasswordCommand));
        }
        /// <summary>
        /// Send confirm code to user email, using for confirm user password when change it
        /// </summary>
        /// <param name="forgetPasswordCommand"></param>
        /// <returns></returns>
        [HttpGet("ForgotPassword")]
        public async Task<ActionResult<ForgotPasswordCommandResult>> ForgetPassword([FromQuery] ForgotPasswordCommand forgetPasswordCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(forgetPasswordCommand));
        }
        /// <summary>
        /// Resend email for confirm user email again
        /// </summary>
        /// <param name="confirmEmailCommand"></param>
        /// <returns></returns>
        [HttpGet("ResendConfirmEmail")]
        public async Task<ActionResult<SendConfirmEmailCommandResult>> ResendConfirmEmail([FromQuery] SendConfirmEmailCommand confirmEmailCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(confirmEmailCommand));
        }
        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="registerCommand"></param>
        /// <returns></returns>
        [HttpPost("CreateUser")]
        public async Task<ActionResult<RegisterCommandResult>> Register([FromBody] RegisterCommand registerCommand)
        {
            return await this.HandleCommandResult(_mediator.Send(registerCommand));
        }
    }
}
