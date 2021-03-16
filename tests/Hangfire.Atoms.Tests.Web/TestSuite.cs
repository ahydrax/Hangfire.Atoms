using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.States;

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
                var job3 = builder.ContinueJobWith(job2, () => Wait(2000));
                var job4 = builder.Schedule(() => Wait(3000), DateTime.UtcNow.AddSeconds(5));
                var job5 = builder.ContinueJobWith(job4, () => Wait(3000));
            });

            client.ContinueJobWith(atomId, () => Done());
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
                     var job3 = builder.ContinueJobWith(job2, () => Wait(500));
                 }
             });

            client.ContinueJobWith(atomId, () => Done());
        }

        public static void AtomTest3()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var job1 = client.Enqueue(() => Wait(2000));

            var atomId = client.ContinueJobWith(job1, "atom-3", builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue(() => Done());
                    var job2 = builder.Enqueue(() => Wait(1000));
                    var job3 = builder.ContinueJobWith(job2, () => Wait(500));
                }
            });

            client.ContinueJobWith(atomId, () => Done());
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
                    var job3 = builder.ContinueJobWith(job2, () => Wait(500));
                }
            });

            client.ContinueJobWith(atomId, () => Done());
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
                    var job3 = builder.ContinueJobWith(job2, () => Wait(500));
                }
            });

            client.ContinueJobWith(atomId, "atom-5-continuation", builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue(() => Done());
                }
            });
        }

        public static void AtomTest6()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            Parallel.For(0, 10, counter =>
            {
                var atomId = client.Schedule($"Atom №{counter}", TimeSpan.FromSeconds(3), builder =>
                {
                    for (var i = 0; i < 500; i++)
                    {
                        builder.Enqueue(() => Done());
                        var job2 = builder.Enqueue(() => Wait(1000));
                        var job3 = builder.ContinueJobWith(job2, () => Wait(500));
                    }
                });

                client.ContinueJobWith(atomId, () => Done());
            });
        }

        public static void AtomTest7()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Schedule($"Atom test-7", TimeSpan.FromSeconds(3), builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue(() => Done());
                    builder.Enqueue(() => FailFast());
                    var job2 = builder.Enqueue(() => Wait(1000));
                    var job3 = builder.ContinueJobWith(job2, () => Wait(500));
                }
            });

            client.ContinueJobWith(atomId, () => Done());
        }
        
        public static void AtomTest8()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Schedule($"Atom test-8", TimeSpan.FromSeconds(3), builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue<AtomTask>(task => task.Done());
                    var job2 = builder.Enqueue(() => Wait(1000));
                    var job3 = builder.ContinueJobWith<AtomTask>(job2, task => task.Wait(500));
                }
            });

            client.ContinueJobWith(atomId, () => Done());
        }

        public static void AtomTest9()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Schedule("Atom test-9", TimeSpan.FromSeconds(3), builder =>
            {
                for (var i = 0; i < 50; i++)
                {
                    builder.Enqueue<AtomTask>(task => task.Done());
                    var job2 = builder.Enqueue(() => Wait(1000), new EnqueuedState("queue2"));
                    var job3 = builder.ContinueJobWith<AtomTask>(job2, task => task.Wait(500), new EnqueuedState("queue2"));
                }
            });

            client.ContinueJobWith(atomId, () => Done());
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public static void FailFast() => throw new ApplicationException("Test OK");
    }

    public class AtomTask
    {
        public Task<string> Done() => Task.FromResult("ALL DONE");

        public void Wait(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }
}
