using System;
using System.Threading.Tasks;

namespace Hangfire.Atoms.Tests.Web
{
    public class AsyncTestSuite
    {
        public async Task AsyncAtomTest()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            var atomId = client.Enqueue("atom-async", builder =>
            {
                var job1 = builder.Enqueue(() => AsyncWait(5000));
                var job2 = builder.Enqueue(() => AsyncWait(3000));
                var job3 = builder.ContinueJobWith(job2, () => AsyncWait(2000));
                var job4 = builder.Schedule(() => AsyncWait(3000), DateTime.UtcNow.AddSeconds(5));
                var job5 = builder.ContinueJobWith(job4, () => AsyncWait(3000));
            });

            client.ContinueJobWith(atomId, () => Done());
        }

        public async Task AsyncWait(int wait)
        {
            await Task.Delay(wait);
        }
        
        public static string Done() => "DONE";
    }
}
