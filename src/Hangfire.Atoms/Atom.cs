using System;
using System.Collections.Generic;
using System.ComponentModel;
using Hangfire.Atoms.Builder;

namespace Hangfire.Atoms
{
    public static class Atom
    {
        public static readonly string JobListKey = "atoms";
        public static readonly string Waiting = "waiting";
        public static readonly string Finished = "finished";

        public static string Create(this IBackgroundJobClient client, string name, Action<IAtomBuilder> buildAtom)
        {
            var builder = new AtomBuilder(name, JobStorage.Current, client, buildAtom);
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
