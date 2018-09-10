using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hangfire.Atoms.Builder;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms
{
    public static class Atom
    {
        public static readonly string JobListKey = "atoms";
        public static readonly string Waiting = "waiting";
        public static readonly string Finished = "finished";

        public static readonly TimeSpan DefaultAtomsExpiration = TimeSpan.FromDays(365);

        public static string Create(this IBackgroundJobClient client, string name, Action<IAtomBuilder> buildAtom)
        {
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom);
            return builder.Build();
        }

        internal static string GenerateKey(string jobId) => "atomic-job:" + jobId;

        [DisplayName("Subatom of {0} finished")]
        public static void OnSubatomFinished(string name, string atomId, string subAtomId, PerformContext context)
        {
            var key = GenerateKey(atomId);
            context.Connection.SetRangeInHash(key, new[] { new KeyValuePair<string, string>(subAtomId, Finished) });

            var completionData = context.Connection.GetAllEntriesFromHash(key);

            var shouldStart = completionData.All(x => x.Value == Finished);
            if (shouldStart)
            {
                using (context.Connection.AcquireDistributedLock(key, TimeSpan.FromSeconds(10)))
                {
                    // TODO extract client
                    var client = new BackgroundJobClient();
                    client.ChangeState(atomId, new EnqueuedState());
                }
            }
        }

        [DisplayName("{0}")]
        public static void CleanupStateOnFinish(string name, PerformContext context)
        {
            using (var tr = context.Connection.CreateWriteTransaction())
            {
                if (tr is JobStorageTransaction jst)
                {
                    jst.ExpireHash(GenerateKey(context.BackgroundJob.Id), DefaultAtomsExpiration);
                    jst.RemoveFromList(JobListKey, context.BackgroundJob.Id);
                    tr.Commit();
                }
            }
        }
    }
}
