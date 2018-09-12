using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Reason { get; }
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        public class Handler : IStateHandler, IApplyStateFilter, IElectStateFilter
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

            public string StateName => AtomRunningState.StateName;

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
                    var jst = (JobStorageTransaction)transaction;
                    jst.RemoveFromList(Atom.JobListKey, context.BackgroundJob.Id);
                    jst.ExpireHash(Atom.GenerateSubAtomKeys(context.BackgroundJob.Id), context.JobExpirationTimeout);
                }
            }

            public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.NewState is AtomRunningState state)
                {
                    var subatomIds = context.Connection.GetAllEntriesFromHash(Atom.GenerateSubAtomKeys(state.AtomId))
                        .Select(x => x.Key)
                        .ToList();

                    foreach (var subatomId in subatomIds)
                    {
                        var subatomStateData = context.Connection.GetStateData(subatomId);
                        var subatomInitialState = subatomStateData.Data[nameof(SubAtomCreatedState.NextState)];
                        if (subatomInitialState == null) throw new InvalidOperationException("NextState is null");

                        var nextState = JsonUtils.Deserialize<IState>(subatomInitialState);

                        _stateChanger.ChangeState(
                            new StateChangeContext(
                                context.Storage,
                                context.Connection,
                                subatomId,
                                nextState));
                    }
                }
            }

            public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
            }

            public void OnStateElection(ElectStateContext context)
            {
                if (context.CurrentState == ScheduledState.StateName || context.CurrentState == AwaitingState.StateName)
                {
                    var isAtom = context.Connection.GetJobParameter(context.BackgroundJob.Id, "!IsAtom") != null;
                    if (isAtom)
                    {
                        context.CandidateState = new AtomRunningState(context.BackgroundJob.Id);
                    }
                }
            }
        }
    }
}
