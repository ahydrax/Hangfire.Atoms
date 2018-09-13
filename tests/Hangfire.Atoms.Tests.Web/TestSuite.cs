using System;
using System.Threading;

namespace Hangfire.Atoms.Tests.Web
{
    public static class TestSuite
    {
        public static void AtomTest()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Enqueue("atom-1", builder =>
            {
                var job1 = builder.Enqueue(() => Wait(5000));
                var job2 = builder.Enqueue(() => Wait(3000));
                var job3 = builder.ContinueWith(job2, () => Wait(2000));
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

            var atomId = client.Enqueue("atom-2", builder =>
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

        public static void AtomTest3()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var job1 = client.Enqueue(() => Wait(2000));

            var atomId = client.ContinueWith(job1, "atom-3", builder =>
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

        public static void AtomTest4()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Schedule("atom-4", TimeSpan.FromSeconds(3), builder =>
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

        public static void AtomTest5()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Schedule("atom-5", TimeSpan.FromSeconds(3), builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue(() => Done());
                    var job2 = builder.Enqueue(() => Wait(1000));
                    var job3 = builder.ContinueWith(job2, () => Wait(500));
                }
            });
            
            client.ContinueWith(atomId, "atom-5-continuation", builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue(() => Done());
                }
            });
        }
    }
}
