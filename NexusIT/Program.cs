using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;

namespace NexusIT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================================================
            // DATABASE
            // =====================================================

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));


            // =====================================================
            // DATABASE DEVELOPMENT TOOLS
            // =====================================================

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();


            // =====================================================
            // IDENTITY
            // =====================================================

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                // Users do not need email confirmation during development
                options.SignIn.RequireConfirmedAccount = false;

                // Password requirements
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LoginPath = "/Identity/Account/Login";
            });


            // =====================================================
            // MVC
            // =====================================================

            builder.Services.AddControllersWithViews();


            // =====================================================
            // HTTP CONTEXT
            // =====================================================

            builder.Services.AddHttpContextAccessor();


            // =====================================================
            // NEXUSIT SERVICES
            // =====================================================

            builder.Services.AddScoped<NexusIT.Services.ActivityLogService>();


            // =====================================================
            // BUILD APPLICATION
            // =====================================================

            var app = builder.Build();

            // Keep a fresh developer checkout self-starting: create the database
            // schema before role/demo data is seeded.
            // ============================================================
            // DATABASE INITIALIZATION
            // ============================================================

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<ApplicationDbContext>();

                db.Database.Migrate();

                RoleSeeder.SeedAsync(services).GetAwaiter().GetResult();
                DemoDataSeeder.SeedAsync(services).GetAwaiter().GetResult();
            }

            // =====================================================
            // ERROR HANDLING
            // =====================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            // =====================================================
            // MIDDLEWARE
            // =====================================================

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapGet("/health", async (ApplicationDbContext db) =>
            {
                var databaseHealthy = await db.Database.CanConnectAsync();
                return databaseHealthy
                    ? Results.Ok(new { status = "healthy", service = "NexusIT", timestamp = DateTimeOffset.UtcNow })
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            });

            // Authentication MUST come before Authorization
            app.UseAuthentication();

            app.UseAuthorization();


            // =====================================================
            // MVC ROUTING
            // =====================================================

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Index}/{id?}");


            // =====================================================
            // ASP.NET CORE IDENTITY PAGES
            // =====================================================

            app.MapRazorPages();


            // =====================================================
            // RUN
            // =====================================================

            app.Run();
        }
    }
}