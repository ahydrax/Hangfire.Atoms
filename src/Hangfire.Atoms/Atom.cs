using System;
using System.Collections.Generic;
using System.ComponentModel;
using Hangfire.Atoms.Builder;
using Hangfire.States;

namespace Hangfire.Atoms
{
    public static class Atom
    {
        public static readonly string JobListKey = "atoms";
        public static readonly string Waiting = "waiting";
        public static readonly string Finished = "finished";
        public static readonly string ParameterAtomId = "AtomId";
        public static readonly string ParameterIsAtom = "IsAtom";
        public static readonly string ParameterRunning = "AtomRunning";

        public static string Enqueue(this IBackgroundJobClient client, string name, Action<IAtomBuilder> buildAtom)
        {
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom);
            return builder.Build();
        }

        public static string Schedule(this IBackgroundJobClient client, string name, TimeSpan enqueueIn, Action<IAtomBuilder> buildAtom)
        {
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom, new ScheduledState(enqueueIn));
            return builder.Build();
        }

        public static string Schedule(this IBackgroundJobClient client, string name, DateTime enqueueAt, Action<IAtomBuilder> buildAtom)
        {
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom, new ScheduledState(enqueueAt));
            return builder.Build();
        }

        public static string ContinueWith(this IBackgroundJobClient client, string parentId, string name, Action<IAtomBuilder> buildAtom)
        {
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom, new AwaitingState(parentId));
            return builder.Build();
        }

        public static string OnTriggerSet(this IBackgroundJobClient client, string triggerName, string name, Action<IAtomBuilder> buildAtom)
        {
            var triggerId = client.OnTriggerSet(triggerName);
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom, new AwaitingState(triggerId));
            return builder.Build();
        }

        internal static string GenerateAtomKey(string jobId) => "atom:" + jobId;

        internal static string GenerateSubAtomKeys(string jobId) => "atom:subs:" + jobId;

        internal static KeyValuePair<string, string>[] GenerateSubAtomStatePair(string jobId, string state)
            => new[]
            {
                new KeyValuePair<string, string>(jobId, state)
            };

        [DisplayName("{0}")]
        public static void Running(string name)
        {
        }
    }
}
