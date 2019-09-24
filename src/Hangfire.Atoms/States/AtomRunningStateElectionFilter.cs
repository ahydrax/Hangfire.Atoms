using System;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms.States
{
    internal sealed class AtomRunningStateElectionFilter : JobFilterAttribute, IApplyStateFilter, IElectStateFilter
    {
        private readonly ILog _logger = LogProvider.For<ContinuationsSupportAttribute>();

        private readonly IBackgroundJobStateChanger _stateChanger;

        public AtomRunningStateElectionFilter() : this(new BackgroundJobStateChanger())
        {
        }

        public AtomRunningStateElectionFilter(IBackgroundJobStateChanger stateChanger)
        {
            _stateChanger = stateChanger ?? throw new ArgumentNullException(nameof(stateChanger));
            Order = 900;
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is EnqueuedState)
            {
                var atomId = context.BackgroundJob.Id;

                var jobIsAtom = context.Connection.GetIfJobIsAtom(atomId);
                if (!jobIsAtom) return;

                context.CandidateState = new AtomRunningState(atomId);
                _logger.Debug($"Set atom:{atomId} as running. Old state is {context.CurrentState}");
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is AtomRunningState state)
            {
                var subatomIds = context.Connection.GetAllItemsFromSet(Atom.GenerateSubAtomKeys(state.AtomId));

                foreach (var subatomId in subatomIds)
                {
                    var subatomStateData = context.Connection.GetStateData(subatomId);
                    var subatomInitialState = subatomStateData.Data.GetByKey(nameof(SubAtomCreatedState.NextState));
                    if (subatomInitialState == null) throw new InvalidOperationException("Next state is NULL.");

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
    }
}
