using JobsityChatAngular.Data;
using JobsityChatAngular.Hubs;
using JobsityChatAngular.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JobsityChatAngular
{
    /// <summary>
    /// Main backend class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Beginning of backend
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string connectionString = builder.Configuration.GetConnectionString("JobsityDbConnectionString") ?? throw new InvalidOperationException("Connection string 'JobsityDbConnectionString' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();
            builder.Services.AddAuthentication()
                .AddIdentityServerJwt();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            }).AddHubOptions<JobsityChatHub>(p =>
            {
                p.MaximumReceiveMessageSize = null;
            });

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseCors(builder =>
            {
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("https://localhost:7234", "https://localhost:44477");
            });

            app.UseAuthorization();
            app.MapHub<JobsityChatHub>("/chatroom");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapFallbackToFile("index.html");
            app.Run();
        }
    }
}
