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

        internal static string GenerateAtomKey(string jobId) => "atom:" + jobId;

        internal static string GenerateSubAtomKeys(string jobId) => "atom:subs:" + jobId;

        [DisplayName("Subatom of {0} finished")]
        public static void OnSubatomFinished(string name, string atomId, string subAtomId, PerformContext context)
        {
            var jsc = (JobStorageConnection)context.Connection;

            var key = GenerateSubAtomKeys(atomId);
            context.Connection.SetRangeInHash(key, new[] { new KeyValuePair<string, string>(subAtomId, Finished) });

            var shouldStart = context.Connection.GetAllEntriesFromHash(key).All(x => x.Value == Finished);
            if (shouldStart)
            {
                var atomKey = GenerateAtomKey(atomId);
                var alreadyRun = jsc.GetValueFromHash(atomKey, "running") == "true";
                if (alreadyRun) return;

                using (jsc.AcquireDistributedLock(atomKey, TimeSpan.Zero))
                {
                    alreadyRun = jsc.GetValueFromHash(atomKey, "running") == "true";
                    if (alreadyRun) return;

                    // TODO extract client
                    var client = new BackgroundJobClient();
                    client.ChangeState(atomId, new EnqueuedState());
                    jsc.SetRangeInHash(atomKey, new[] { new KeyValuePair<string, string>(atomKey, "true") });
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
                    jst.ExpireHash(GenerateSubAtomKeys(context.BackgroundJob.Id), DefaultAtomsExpiration);
                    jst.RemoveFromList(JobListKey, context.BackgroundJob.Id);
                    tr.Commit();
                }
            }
        }
    }
}
