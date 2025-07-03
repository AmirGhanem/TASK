using Application.Utilities.Extensions;
using Application.Utilities.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Logs.Queries.GetDatabaseTables
{
    public record GetDatabaseTablesQueryResult : BaseCommandResult
    {
        public BasePaginatedList<string> TableNames { get; set; } 
    }

    public record GetDatabaseTablesQuery : BasePaginatedQuery, IRequest<GetDatabaseTablesQueryResult>
    {
    }
    public sealed class GetDatabaseTablesHandler : IRequestHandler<GetDatabaseTablesQuery, GetDatabaseTablesQueryResult>
    {
        private readonly ApplicationDbContext _context;
        

        public GetDatabaseTablesHandler(ApplicationDbContext dbContext, ILogger<GetDatabaseTablesHandler> logger)
        {
            _context = dbContext;
        }

        public async Task<GetDatabaseTablesQueryResult> Handle(GetDatabaseTablesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve table names from the database model
                var tableNamesQuery = _context.Model.GetEntityTypes()
                    .Select(t => t.GetTableName()).Distinct();

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    tableNamesQuery = tableNamesQuery.Where(n => n.Contains(request.SearchTerm));
                }

                var tableNames = await tableNamesQuery
                    .ToList().ToPaginatedListAsync(request.PageNumber,request.PageSize);

                return new GetDatabaseTablesQueryResult
                {
                    IsSuccess = true,
                    TableNames = tableNames
                };
            }
            catch (Exception ex)
            {
                return new GetDatabaseTablesQueryResult
                {
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    IsSuccess = false
                };
            }
        }
    }

}
