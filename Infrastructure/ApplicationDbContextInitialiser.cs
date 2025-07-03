
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Identity;
using Infrastructure;
using System.Reflection;
using Infrastructure.Common.Extensions;
using Domain.Common.Constants;



namespace Infrastructure;
public partial class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    // Default users
    readonly ApplicationUser administrator = new ApplicationUser
    {
        Name = "Admin",
        EmailConfirmed = true,
        PhoneNumberConfirmed = true,
        UserName = "admin",
        Email = "admin@localhost",
        PhoneNumber = "01002057457"

    };
    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {

            if (_context.Database.IsSqlServer())
            {

#if DEBUG || LOCAL
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                }
                //await ChangeAdminPassword("123456", "Admin@991");
                //await _context.Database.EnsureCreatedAsync();
                //await _context.Database.MigrateAsync();
                await SeedRolesAsync();
                await SeedAdminAsync();
                //await SeedFunctions();

                await SeedOrdersAsync();
               
#endif
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
    private async Task ChangeAdminPassword(string oldPassword, string newPassword)
    {
        var admin = await _userManager.FindByNameAsync(administrator.UserName);
        await _userManager.ChangePasswordAsync(admin, oldPassword, newPassword);
    }
    public async Task SeedFunctions()
    {
        try
        {
            foreach (var file in await Assembly.GetExecutingAssembly().GetFilesContentInFolderAsync("SqlScripts"))
            {
                await _context.Database.ExecuteSqlRawAsync(file.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task SeedRolesAsync()
    {
        try
        {
#if DEBUG || LOCAL || LIVESTAGING
            // Default roles
            foreach (var role in Roles.GetAllRoles())
            {
                if (_roleManager.Roles.All(r => r.Name != role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }


    public async Task SeedOrdersAsync()
    {
        try
        {
#if DEBUG 
           
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding Order List the database.");
            throw;
        }
    }

  

   

    public async Task SeedAdminAsync()
    {

        try
        {
#if DEBUG || LOCAL || LIVESTAGING


            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "123456");
            }

            _context.UserRoles.RemoveRange(_context.UserRoles.Where(x => x.UserId == administrator.Id).ToList());
            await _context.SaveChangesAsync();


            var roles = Roles.GetAllRoles();
            var result = await _userManager.AddToRoleAsync(administrator, Roles.ADMIN);
            // Default data
            // Seed, if necessary
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

}
