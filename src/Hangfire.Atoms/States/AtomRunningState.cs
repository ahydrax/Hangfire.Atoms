using System.Collections.Generic;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms.States
{
    public class AtomRunningState : IState
    {
        public static readonly string StateName = "Atom running";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>
        {
            {nameof(AtomId), AtomId}
        };

        public AtomRunningState(string atomId)
        {
            AtomId = atomId;
        }

        public string AtomId { get; }
        public string Name => StateName;
        public string Reason => string.Empty;
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        public class Handler : IStateHandler
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public string StateName => AtomRunningState.StateName;

            public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                var atomId = context.BackgroundJob.Id;
                transaction.InsertToList(Atom.JobListKey, atomId);
            }

            public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                var jst = (JobStorageTransaction)transaction;
                var atomId = context.BackgroundJob.Id;
                jst.RemoveFromList(Atom.JobListKey, atomId);
                jst.ExpireSet(Atom.GenerateSubAtomKeys(atomId), context.JobExpirationTimeout);
            }
        }
    }
}
