using System;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Atoms.Tests.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddHangfire(configuration =>
            {
                configuration
                    .UseAtoms()
                    .UseMemoryStorage();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerCheckInterval = TimeSpan.FromSeconds(1),
                HeartbeatInterval = TimeSpan.FromMilliseconds(500),
                ServerTimeout = TimeSpan.FromSeconds(2),
                WorkerCount = 2,
                ServerName = "TEST SERVER",
                Queues = new[] { "queue1", "default", "queue2" }
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerCheckInterval = TimeSpan.FromSeconds(1),
                HeartbeatInterval = TimeSpan.FromMilliseconds(500),
                ServerTimeout = TimeSpan.FromSeconds(2),
                WorkerCount = 2,
                ServerName = "TEST SERVER 1 ",
                Queues = new[] { "default", "queue2" }
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerCheckInterval = TimeSpan.FromSeconds(1),
                HeartbeatInterval = TimeSpan.FromMilliseconds(500),
                ServerTimeout = TimeSpan.FromSeconds(2),
                WorkerCount = 2,
                ServerName = "TEST SERVER 2",
                Queues = new[] { "queue1", "queue2" }
            });

            app.UseHangfireDashboard("", new DashboardOptions { StatsPollingInterval = 1000 });
            RecurringJob.AddOrUpdate("test-1", () => TestSuite.AtomTest(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-2", () => TestSuite.AtomTest2(), Cron.Yearly, TimeZoneInfo.Utc);
        }
    }
}
