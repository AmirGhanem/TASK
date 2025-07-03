using Domain.Common.Constants;
using System.Text.Json.Serialization;

namespace Configurations
{
    public static class AuthorizationConfiguration
    {
        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                //options.Conventions.Add(new FolderAuthorizeConvention("TaskManager", Policies.CAN_USE_TASKMANAGER_POLICY));
                //options.Conventions.Add(new FolderAuthorizeConvention("PMO", Policies.CAN_USE_PMO_POLICY));
                //options.Conventions.Add(new FolderAuthorizeConvention("NurseryManager", Policies.CAN_USE_NurseryManager_POLICY));
            });
            //services.AddAuthorization(o => o.AddPolicy(Policies.CAN_USE_TASKMANAGER_POLICY, builder => builder.RequireRole(Roles.ADMIN, Roles.CAN_USE_TASK_MANAGER)));
            //services.AddAuthorization(o => o.AddPolicy(Policies.CAN_USE_PMO_POLICY, builder => builder.RequireRole(Roles.ADMIN, Roles.CAN_USE_PMO)));
            //services.AddAuthorization(o => o.AddPolicy(Policies.CAN_USE_NurseryManager_POLICY, builder => builder.RequireRole(Roles.ADMIN, Roles.CAN_USE_NURSERY_MANAGER)));
            return services;
        }
    }
}