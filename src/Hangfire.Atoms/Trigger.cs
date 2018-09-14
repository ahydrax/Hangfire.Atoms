using System;
using System.ComponentModel;
using Hangfire.Atoms.States;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms
{
    public static class Trigger
    {
        public static readonly string JobListKey = "atom:triggers";
        public static readonly string JobIdKey = "jobId";
        public static readonly string JobSetKey = "set";
        public static readonly string ParameterName = "Trigger";

        public static string OnTriggerSet(this IBackgroundJobClient client, string triggerName)
            => client.Create(() => On(triggerName), new TriggerWaitingState(triggerName));

        public static void SetTrigger(this IStorageConnection connection, string triggerName)
        {
            var jsc = (JobStorageConnection)connection;
            var triggerKey = GenerateTriggerKey(triggerName);
            var jobIds = jsc.GetAllItemsFromList(triggerKey);

            var client = new BackgroundJobClient();
            try
            {
                using (connection.AcquireDistributedLock(triggerKey, TimeSpan.Zero))
                {
                    foreach (var jobId in jobIds)
                    {
                        client.ChangeState(jobId, new EnqueuedState());
                    }
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // Assume already run
            }
        }

        internal static string GenerateTriggerKey(string triggerName) => "atoms:trigger:" + triggerName;

        [DisplayName("{0}")]
        public static void On(string triggerName)
        {

        }
    }
}
