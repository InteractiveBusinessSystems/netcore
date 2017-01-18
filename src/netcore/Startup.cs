using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NetCore.Code;

namespace NetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserManagement", policy => policy.RequireClaim("manage_user"));
            });

            var efConnection = Configuration["ConnectionString"];
            services.AddDbContext<MyContext>(options =>
            {
                options.UseSqlServer(efConnection);

                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<MyContext>()
                .AddDefaultTokenProviders();

            services.AddOpenIddict()
                .AddEntityFrameworkCoreStores<MyContext>()
                .AddMvcBinders()
                .EnableTokenEndpoint("/connect/token")
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .DisableHttpsRequirement()
                .AddEphemeralSigningKey();

            services.AddSwaggerGen();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, MyContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            BuildUser(userManager, roleManager).Wait();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            
            loggerFactory.AddDebug();
            //loggerFactory.AddProvider(new FileLoggerProvider(@"c:\git\log.txt", LogLevel.Information));

            app.UseIdentity();
            app.UseOAuthValidation();
            app.UseOpenIddict();

            app.UseMvc();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi();
        }

        public async Task BuildUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = "admin"
                });
            }

            var r = await roleManager.FindByNameAsync("admin");
            var rc = await roleManager.GetClaimsAsync(r);

            if (!rc.Any(x => x.Type == "manage_user"))
            {
                await roleManager.AddClaimAsync(r, new System.Security.Claims.Claim("manage_user", "true"));
            }

            await CreateJD(userManager);
            await CreateIBS(userManager);
        }

        private static async Task CreateJD(UserManager<ApplicationUser> userManager)
        {
            var u = await userManager.FindByNameAsync("jdotson");
            if (u == null)
            {
                u = new ApplicationUser
                {
                    UserName = "jdotson",

                };
                var x = await userManager.CreateAsync(u, "MySecurePassword1234");
            }
            var uc = await userManager.GetClaimsAsync(u);
            if (!uc.Any(x => x.Type == "phone"))
            {
                await userManager.AddClaimAsync(u, new System.Security.Claims.Claim("phone", "867-5309"));
            }

            if (!await userManager.IsInRoleAsync(u, "admin"))
            {
                await userManager.AddToRoleAsync(u, "admin");
            }
        }

        private static async Task CreateIBS(UserManager<ApplicationUser> userManager)
        {
            var u = await userManager.FindByNameAsync("ibstest");
            if (u == null)
            {
                u = new ApplicationUser
                {
                    UserName = "ibstest",

                };
                var x = await userManager.CreateAsync(u, "MySecurePassword1234");
            }
            var uc = await userManager.GetClaimsAsync(u);
            if (!uc.Any(x => x.Type == "phone"))
            {
                await userManager.AddClaimAsync(u, new System.Security.Claims.Claim("phone", "867-5309"));
            }
        }
    }
}
