using System;
using System.Collections.Generic;
using System.Linq;
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
            {nameof(ContinuationOptions), JsonUtils.Serialize(ContinuationOptions)}
        };

        [JsonConstructor]
        public SubAtomCreatedState(string atomId, JobContinuationOptions continuationOptions, IState nextState)
        {
            AtomId = atomId;
            NextState = nextState;
            ContinuationOptions = continuationOptions;
        }

        public string AtomId { get; }
        public IState NextState { get; }
        public JobContinuationOptions ContinuationOptions { get; }
        public string Name => StateName;
        public string Reason { get; }
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        internal class Handler : IStateHandler, IElectStateFilter
        {
            private readonly IBackgroundJobStateChanger _stateChanger;

            public Handler()
                : this(new BackgroundJobStateChanger())
            {

            }

            public Handler(IBackgroundJobStateChanger stateChanger)
            {
                _stateChanger = stateChanger;
            }

            public string StateName => SubAtomCreatedState.StateName;

            public void OnStateElection(ElectStateContext context)
            {
                if (context.CandidateState is SucceededState)
                {
                    var atomId = context.Connection.GetJobParameter(context.BackgroundJob.Id, "!AtomId");
                    // Only handle if job is subatom
                    if (atomId == null) return;

                    context.Connection.SetRangeInHash(
                        Atom.GenerateSubAtomKeys(atomId),
                        Atom.GenerateSubAtomStatePair(context.BackgroundJob.Id, Atom.Finished));

                    ChangeAtomState(atomId, context.Connection as JobStorageConnection, context);
                }
            }

            private void ChangeAtomState(string atomId, JobStorageConnection connection, ElectStateContext context)
            {
                var key = Atom.GenerateSubAtomKeys(atomId);
                var shouldStart = connection.GetAllEntriesFromHash(key).All(x => x.Value == Atom.Finished);
                if (shouldStart)
                {
                    var atomKey = Atom.GenerateAtomKey(atomId);
                    var alreadyRun = connection.GetValueFromHash(atomKey, "running") == "true";
                    if (alreadyRun) return;

                    try
                    {
                        using (connection.AcquireDistributedLock(atomKey, TimeSpan.Zero))
                        {
                            alreadyRun = connection.GetValueFromHash(atomKey, "running") == "true";
                            if (alreadyRun) return;

                            _stateChanger.ChangeState(new StateChangeContext(context.Storage, connection, atomId, new EnqueuedState()));
                            connection.SetRangeInHash(atomKey, Atom.GenerateSubAtomStatePair("running", "true"));
                        }
                    }
                    catch (DistributedLockTimeoutException)
                    {
                        // Assume started
                    }
                }
            }

            public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.NewState is SubAtomCreatedState state)
                {
                    context.Connection.SetJobParameter(context.BackgroundJob.Id, "!AtomId", state.AtomId);
                    transaction.SetRangeInHash(
                        Atom.GenerateSubAtomKeys(state.AtomId),
                        Atom.GenerateSubAtomStatePair(context.BackgroundJob.Id, Atom.Waiting));
                }
            }

            public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
            }
        }
    }
}
