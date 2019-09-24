using System.Collections.Generic;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms.States
{
    public class AtomCreatedState : IState
    {
        public static readonly string StateName = "Atom created";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>
        {
            {nameof(AtomId), AtomId}
        };

        public AtomCreatedState(string atomId)
        {
            AtomId = atomId;
        }

        public string AtomId { get; }
        public string Name => StateName;
        public string Reason => string.Empty;
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        internal class Handler : IStateHandler
        {
            public string StateName => AtomCreatedState.StateName;

            public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                var atomId = context.BackgroundJob.Id;
                context.Connection.SetJobIsAtom(atomId);
            }

            public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
            }
        }
    }
}
