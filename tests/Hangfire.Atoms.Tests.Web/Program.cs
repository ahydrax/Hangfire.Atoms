using Microsoft.AspNetCore.Hosting;
using static Microsoft.AspNetCore.WebHost;

namespace Hangfire.Atoms.Tests.Web
{
    public static class Program
    {
        public static void Main(string[] args) => 
            CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
    }
}
