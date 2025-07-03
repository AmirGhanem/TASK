using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Utilities.Models;
using Application.Utilities.Extensions;
using Application.Utilities.Sort;
using Application.Utilities.Filter;
using Microsoft.AspNetCore.Identity;

namespace Application.Common.User.Queries.GetUsers
{
    public record GetUsersQueryResult : BaseCommandResult
    {
        public BasePaginatedList<UserVM> Result { get; set; }
    }
    public record GetUsersQuery : BasePaginatedQuery, IRequest<GetUsersQueryResult>
    {

    }
    public record UserVM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
    public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, GetUsersQueryResult>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<GetUsersHandler> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUsersHandler(ApplicationDbContext dbContext, ILogger<GetUsersHandler> logger,UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            this._userManager = userManager;
        }

        public async Task<GetUsersQueryResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {


                var result = await _dbContext.Users
                    .Search(request.SearchTerm)
                    .Select(u => new UserVM
                    {
                        Email = u.Email,
                        Id = u.Id,
                        Name = u.Name,
                        PhoneNumber = u.PhoneNumber??"",
                        IsActive = u.LockoutEnd == null ? true : false
                    })
                    .Filter(request.Filters)
                    .Sort(request.Sorts ?? new List<SortedQuery>() { new SortedQuery() { PropertyName = "FirstName", Direction = SortDirection.ASC } })
                    .ToPaginatedListAsync(request.PageNumber, request.PageSize);
                foreach (var item in result.Items)
                {
                    var user = await _userManager.FindByIdAsync(item.Id);
                    var roles = await _userManager.GetRolesAsync(user);
                    item.Roles = roles.ToList();
                }
                if (result == null)
                {
                    return new GetUsersQueryResult
                    {
                        ErrorCode = Domain.Common.ErrorCode.UserNotFound,
                        IsSuccess = false,
                        Result = null
                    };
                }
                return new GetUsersQueryResult
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new GetUsersQueryResult
                {
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    IsSuccess = false,
                    Result = null
                };
            }
        }
    }
}
