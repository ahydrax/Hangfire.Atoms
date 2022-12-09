using System;
using System.Threading.Tasks;
using Hangfire.Annotations;

namespace Hangfire.Atoms.Tests.Web;

[UsedImplicitly]
public class TriggersTestSuite
{
    [UsedImplicitly]
    public async Task TriggerTest1()
    {
        var client = new BackgroundJobClient(JobStorage.Current);

        var triggerId = client.OnTriggerSet("trigger-test-1");

        client.ContinueJobWith(triggerId, () => Done());

        await Task.Delay(TimeSpan.FromSeconds(10));
    }

    public static string Done() => "TRIGGER TEST DONE";
}
