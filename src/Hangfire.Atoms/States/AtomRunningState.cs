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
        public string Reason => "Atom is running.";
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        public class Handler : IStateHandler, IApplyStateFilter
        {
            public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.NewState is AtomRunningState)
                {
                    transaction.InsertToList(Atom.JobListKey, context.BackgroundJob.Id);
                }
            }

            public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.OldStateName == StateName)
                {
                    transaction.RemoveFromList(Atom.JobListKey, context.BackgroundJob.Id);
                }
            }

            public string StateName => AtomRunningState.StateName;

            public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {

            }

            public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.OldStateName == StateName)
                {
                    var jst = (JobStorageTransaction)transaction;
                    jst.ExpireHash(Atom.GenerateSubAtomKeys(context.BackgroundJob.Id), context.JobExpirationTimeout);
                }
            }
        }
    }
}
