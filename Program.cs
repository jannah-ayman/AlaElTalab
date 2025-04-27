using AlaElTalab.Data;
using AlaElTalab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AlaElTalab
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddRazorPages();

            // Add authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomerOnly", policy =>
                    policy.RequireRole("Customer"));

                options.AddPolicy("ServiceProviderOnly", policy =>
                    policy.RequireRole("ServiceProvider"));
            });

            // Configure cookie settings for redirection
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/Login"; // Redirect to login for access denied
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Seed roles
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Customer", "ServiceProvider" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Custom unauthorized handling middleware
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 403) // Forbidden (wrong role)
                {
                    if (context.User.Identity.IsAuthenticated)
                    {
                        if (context.User.IsInRole("Customer"))
                            context.Response.Redirect("/Customers/Index");
                        else if (context.User.IsInRole("ServiceProvider"))
                            context.Response.Redirect("/ServiceProviders/Index");
                    }
                    else
                    {
                        context.Response.Redirect("/Identity/Account/Login");
                    }
                }
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}