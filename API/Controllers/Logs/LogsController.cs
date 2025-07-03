using Application.Logs.Queries.GetDatabaseTables;
using Application.Logs.Queries.GetLogs;
using Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Controllers.Logs
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Settings")]
    [Authorize]
    public class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

         

        [HttpPost("GetLogs")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetLogsQueryResult>> GetLogs([FromBody] GetLogsQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }
        
        
        
        [HttpPost("GetTablesName")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetDatabaseTablesQueryResult>> GetTablesName([FromBody] GetDatabaseTablesQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }
    }
}
