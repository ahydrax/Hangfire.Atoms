using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Atoms;
using Hangfire.Atoms.Tests.Web;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("Hangfire_PostgreSql_ConnectionString");

builder.Services.AddHangfire(configuration =>
{
    configuration.UsePostgreSqlStorage(connectionString);
    configuration.UsePostgreSqlMetrics();
    //configuration.UseRedisStorage("192.168.5.32");
    configuration.UseAtoms();
});

builder.Services.AddHangfireServer();

var app = builder.Build();

const string dashboardLocation = "/hangfire-test"; // made for UI route tests
app.MapGet("/", context =>
{
    context.Response.Redirect(dashboardLocation);
    return Task.CompletedTask;
});

app.UseDeveloperExceptionPage();

app.UseHangfireDashboard(dashboardLocation, new DashboardOptions { StatsPollingInterval = 1000 });
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

app.Run();
