using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Logic.Helper;
using Logic.IHelper;
using Logic.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoBackUp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("AutoBackupHangFire")));

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddSingleton<IGeneralConfiguration>(Configuration.GetSection("GeneralConfiguration").Get<GeneralConfiguration>());
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IBackupHelper, BackupHelper>();
            services.AddTransient<MyInitializationService>();


            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,MyInitializationService initializationService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            HangFireConfiguration(app);
            var backUpStorage = new SqlServerStorage(Configuration.GetConnectionString("AutoBackupHangFire"));
            JobStorage.Current = backUpStorage;

            initializationService.Initialize();
        }

        public void HangFireConfiguration(IApplicationBuilder app)
        {
            var robotDashboardOptions = new DashboardOptions { Authorization = new[] { new MyAuthorizationFilter() } };

            var robotOptions = new BackgroundJobServerOptions
            {
                ServerName = String.Format(
                "{0}.{1}",
                Environment.MachineName,
                Guid.NewGuid().ToString())
            };
            app.UseHangfireServer(robotOptions);
            var RobotStorage = new SqlServerStorage(Configuration.GetConnectionString("AutoBackupHangFire"));
            JobStorage.Current = RobotStorage;
            app.UseHangfireDashboard("/AutoBackUp", robotDashboardOptions, RobotStorage);

        }

        public class MyAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                //var httpContext = context.GetHttpContext();

                //// Allow all authenticated users to see the Dashboard (potentially dangerous):: that's if they can know the link.
                //return httpContext.User.Identity.IsAuthenticated;
                return true;
            }
        }

        public class MyInitializationService
        {
            private readonly IBackupHelper _backupHelper;
            public IConfiguration Configuration { get; }

            public MyInitializationService(IBackupHelper backupHelper)
            {
                _backupHelper = backupHelper;
            }

            public void Initialize()
            {
                _backupHelper.Start12();
                // do something
            }
        }
    }
}
