using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interceptors
{
    internal class ApplicationUserQueryInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            // Check if the command is for selecting from ModelBase<T> and includes CreatedBy or ModifiedBy
            if (command.CommandText.Contains("ModelBase") && (command.CommandText.Contains("CreatedBy") || command.CommandText.Contains("ModifiedBy")))
            {
                // Modify the SQL command to select only the desired columns from ApplicationUser
                command.CommandText = command.CommandText.Replace("SELECT CreatedBy.*", "SELECT CreatedBy.Id, CreatedBy.Name")
                                                         .Replace("SELECT ModifiedBy.*", "SELECT ModifiedBy.Id, ModifiedBy.Name");
            }

            return base.ReaderExecuting(command, eventData, result);
        }
    }
}
