using System;
using System.Threading;

namespace Hangfire.Atoms.Tests.Web
{
    public static class TestSuite
    {
        public static void AtomTest()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Create("atom-1", builder =>
            {
                var job1 = builder.Enqueue(() => Wait(7000));
                var job2 = builder.Enqueue(() => Wait(4000));
                var job3 = builder.ContinueWith(job2, () => Wait(4000));
                var job4 = builder.Schedule(() => Wait(3000), DateTime.UtcNow.AddSeconds(5));
                var job5 = builder.ContinueWith(job4, () => Wait(3000));
            });

            client.ContinueWith(atomId, () => Done());
        }

        public static void Wait(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public static string Done() => "DONE";

        public static void AtomTest2()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Create("atom-2", builder =>
             {
                 for (var i = 0; i < 50; i++)
                 {
                     builder.Enqueue(() => Done());
                     var job2 = builder.Enqueue(() => Wait(1000));
                     var job3 = builder.ContinueWith(job2, () => Wait(500));
                 }
             });

            client.ContinueWith(atomId, () => Done());
        }
    }
}
