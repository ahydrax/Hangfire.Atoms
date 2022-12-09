using System;
using System.Threading.Tasks;

namespace Hangfire.Atoms.Tests.Web;

public class AsyncTestSuite
{
    public async Task AsyncAtomTest()
    {
        await Task.Yield();

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

    public async Task AsyncAtomTest2()
    {
        await Task.Yield();

        var client = new BackgroundJobClient(JobStorage.Current);

        var atomId = client.Enqueue("atom-async", builder =>
        {
            for (var i = 0; i < 150; i++)
            {
                builder.Enqueue(() => AsyncWaitOrException(1000));
            }
        });

        client.ContinueJobWith(atomId, () => Done());
    }

    public async Task AsyncWait(int wait)
    {
        await Task.Delay(wait);
    }

    public async Task AsyncWaitOrException(int wait)
    {
        var r = new Random();

        if (0.5 > r.NextDouble())
        {
            await Task.Delay(wait);
        }
        else
        {
            throw new Exception("Test OK");
        }
    }

    public static string Done() => "DONE";
}
