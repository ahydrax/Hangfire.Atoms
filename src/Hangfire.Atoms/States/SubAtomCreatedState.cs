using System;
using System.Collections.Generic;
using Hangfire.States;
using Hangfire.Storage;
using Newtonsoft.Json;

namespace Hangfire.Atoms.States
{
    public class SubAtomCreatedState : IState
    {
        public static readonly string StateName = "Subatom created";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>
        {
            {nameof(AtomId), AtomId},
            {nameof(NextState), JsonUtils.Serialize(NextState)},
            {nameof(ContinuationOptions), ContinuationOptions.ToString("G")}
        };

        public SubAtomCreatedState(string atomId, IState nextState, JobContinuationOptions continuationOptions)
        {
            AtomId = atomId;
            NextState = nextState;
            ContinuationOptions = continuationOptions;
        }

        [Obsolete("Only for json serializer", true)]
        [JsonConstructor]
        public SubAtomCreatedState(string atomId, IState nextState, string continuationOptions)
        {
            AtomId = atomId;
            NextState = nextState;
            Enum.TryParse(continuationOptions, out JobContinuationOptions c);
            ContinuationOptions = c;
        }

        public string AtomId { get; }
        public IState NextState { get; }
        public JobContinuationOptions ContinuationOptions { get; }
        public string Name => StateName;
        public string Reason => string.Empty;
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        internal class Handler : IStateHandler
        {
            public string StateName => SubAtomCreatedState.StateName;

            public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.NewState is SubAtomCreatedState state)
                {
                    var atomId = state.AtomId;
                    var subatomId = context.BackgroundJob.Id;

                    context.Connection.SetAtomId(subatomId, atomId);
                    context.Connection.SetSubAtomContinuation(subatomId, state.ContinuationOptions);

                    transaction.AddToSet(Atom.GenerateSubAtomKeys(atomId), subatomId);
                    transaction.AddToSet(Atom.GenerateSubAtomRemainingKeys(atomId), subatomId);
                }
            }

            public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.NewState is DeletedState)
                {
                    var subatomId = context.BackgroundJob.Id;
                    var atomId = context.Connection.GetAtomId(subatomId);
                    transaction.RemoveFromSet(Atom.GenerateSubAtomRemainingKeys(atomId), subatomId);
                }
            }
        }
    }
}
