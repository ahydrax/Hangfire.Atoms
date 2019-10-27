using System;
using System.ComponentModel;
using Hangfire.Annotations;
using Hangfire.Atoms.States;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms
{
    public static class Trigger
    {
        public static readonly string JobListKey = "atom:triggers";
        public static readonly string ParameterName = "Trigger";

        [PublicAPI]
        public static string OnTriggerSet(this IBackgroundJobClient client, string triggerName)
            => client.Create(() => On(triggerName), new TriggerWaitingState(triggerName));

        [PublicAPI]
        public static void SetTrigger(this IStorageConnection connection, string triggerName)
        {
            var jsc = (JobStorageConnection)connection;
            var client = new BackgroundJobClient();

            SetTriggerInternal(client, jsc, triggerName);
        }

        [PublicAPI]
        public static void SetTrigger(this IBackgroundJobClient client, string triggerName)
        {
            var storage = JobStorage.Current;
            using var connection = storage.GetJobStorageConnection();

            SetTriggerInternal(client, connection, triggerName);
        }

        private static void SetTriggerInternal(
            IBackgroundJobClient client,
            JobStorageConnection connection,
            string triggerName)
        {
            var triggerKey = GenerateTriggerKey(triggerName);
            var jobIds = connection.GetAllItemsFromList(triggerKey);

            try
            {
                using var _ = connection.AcquireDistributedLock(triggerKey, TimeSpan.Zero);

                foreach (var jobId in jobIds)
                {
                    client.ChangeState(jobId, new EnqueuedState());
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
