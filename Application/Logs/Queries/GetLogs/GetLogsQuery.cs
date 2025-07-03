using Application.Utilities.Extensions;
using Application.Utilities.Filter;
using Application.Utilities.Models;
using Application.Utilities.Sort;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Logs.Queries.GetLogs
{
    public record GetLogsQueryResult : BaseCommandResult
    {
        public BasePaginatedList<LogVM> Logs { get; set; }
    }
    public record GetLogsQuery : BasePaginatedQuery, IRequest<GetLogsQueryResult> { }
    public record LogVM
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? ColomnId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ActionBy { get; set; } = string.Empty;
        public string ActionById { get; set; } = string.Empty;
    }
    public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, GetLogsQueryResult>
    {
        private readonly ApplicationDbContext _context;

        public GetLogsQueryHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<GetLogsQueryResult> Handle(GetLogsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var logs = await _context.Logs
                    .Search(request.SearchTerm)
                    .Select(l => new LogVM
                    {
                        Id = l.Id,
                        TableName = l.TableName,
                        Action = l.Action,
                        ActionById = l.ActionBy,
                        Date = l.Date,
                        ColomnId = l.ColomnId
                    }).Sort(request.Sorts)
                .Filter(request.Filters)
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);
                foreach (var log in logs.Items)
                {
                    var actionByName = await _context.Users.Where(u => u.Id == log.ActionById).Select(u => u.Name).FirstOrDefaultAsync();
                    log.ActionBy = actionByName??"";
                }
                return new GetLogsQueryResult
                {
                    IsSuccess = true,
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                return new GetLogsQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
