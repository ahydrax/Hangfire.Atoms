using System;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Atoms.Tests.Web
{
    public class Startup
    {
        private const string DefaultConnectionString = @"Server=localhost;Integrated Security=true;Database=hangfire_tests";

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
                configuration.UsePostgreSqlStorage(Environment.GetEnvironmentVariable("Hangfire_PostgreSql_ConnectionString"));
                configuration.UsePostgreSqlMetrics();
                //configuration.UseRedisStorage("192.168.5.32");
                configuration.UseAtoms();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerCheckInterval = TimeSpan.FromSeconds(10),
                HeartbeatInterval = TimeSpan.FromSeconds(10),
                ServerTimeout = TimeSpan.FromSeconds(15),
                WorkerCount = 400,
                ServerName = "ATOMS SERVER",
                Queues = new[] { "queue1", "default", "queue2" }
            });

            app.UseHangfireDashboard("", new DashboardOptions { StatsPollingInterval = 1000 });
            RecurringJob.AddOrUpdate("test-1", () => TestSuite.AtomTest(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-2", () => TestSuite.AtomTest2(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-3", () => TestSuite.AtomTest3(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-4", () => TestSuite.AtomTest4(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-5", () => TestSuite.AtomTest5(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-6", () => TestSuite.AtomTest6(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-7", () => TestSuite.AtomTest7(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-8", () => TestSuite.AtomTest8(), Cron.Yearly, TimeZoneInfo.Utc);

            var asyncTestSuite = new AsyncTestSuite();
            RecurringJob.AddOrUpdate("test-async", () => asyncTestSuite.AsyncAtomTest(), Cron.Yearly, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate("test-async-2", () => asyncTestSuite.AsyncAtomTest2(), Cron.Yearly, TimeZoneInfo.Utc);
        }
    }
}
