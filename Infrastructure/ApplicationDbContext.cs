
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Domain.Identity;
using System.Reflection;
using Infrastructure.Common.Extensions;
using Infrastructure.Interceptors;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Duende.IdentityServer.EntityFramework.Options;
using Domain.Models.Logs;
using Domain.Models.Orders;
using Domain.Models.OrderItems;
using Microsoft.EntityFrameworkCore.Metadata;





namespace Infrastructure;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions
        , AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor
        ) : base(options,operationalStoreOptions)
    {
        this._auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        modelBuilder.CheckForTrim();

        modelBuilder.Entity<Order>().Property(x => x.No).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);




        base.OnModelCreating(modelBuilder);
    }
    

    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }


    public DbSet<Log> Logs { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }


}
