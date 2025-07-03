using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace Application.Common.User.Commands.DeleteUser
{
    public record DeleteUserCommandResult : BaseCommandResult
    {

    }
    public record DeleteUserCommand : IRequest<DeleteUserCommandResult>
    {
        [Required(ErrorMessage = nameof(Domain.Common.ErrorCode.FieldRequired), AllowEmptyStrings = false)]
        public string UserId
        {
            get;
            set;
        }
    }
    public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserCommandResult>
    {
        private readonly ApplicationDbContext _dbContext;

        public DeleteUserCommandHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<DeleteUserCommandResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return new DeleteUserCommandResult()
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.UserNotFound,
                    Errors = { "User Not Found." }
                };
            }
            bool canDelete = true;
            if (canDelete)
            {
                _dbContext.Users.Remove(user);
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.Now.AddYears(100);
            }
            await _dbContext.SaveChangesAsync();
            return new DeleteUserCommandResult()
            {
                IsSuccess = true
            };
        }
    }
}
