using System;
using System.Collections.Generic;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms.States
{
    internal sealed class SubAtomStateElectionFilter : JobFilterAttribute, IApplyStateFilter
    {
        private readonly ILog _logger = LogProvider.For<ContinuationsSupportAttribute>();

        private readonly HashSet<string> _knownFinalStates;
        private readonly IBackgroundJobStateChanger _stateChanger;

        public SubAtomStateElectionFilter()
            : this(new HashSet<string> { DeletedState.StateName, SucceededState.StateName })
        {
        }

        public SubAtomStateElectionFilter(HashSet<string> knownFinalStates)
            : this(knownFinalStates, new BackgroundJobStateChanger())
        {
        }

        public SubAtomStateElectionFilter(HashSet<string> knownFinalStates, IBackgroundJobStateChanger stateChanger)
        {
            _knownFinalStates = knownFinalStates ?? throw new ArgumentNullException(nameof(knownFinalStates));
            _stateChanger = stateChanger ?? throw new ArgumentNullException(nameof(stateChanger));
            Order = 800;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            var subatomId = context.BackgroundJob.Id;

            var atomId = context.Connection.GetAtomId(subatomId);
            if (atomId == null) return; // Only handle if job is subatom

            var continuationOption = context.Connection.GetSubAtomContinuationOption(subatomId);
            if (ShouldProgressAtom(continuationOption, context.NewState))
            {
                MarkSubatomFinished(context, atomId);
                MarkAtomFinishedIfAllSucceeded(context, atomId);
            }
        }

        private bool ShouldProgressAtom(JobContinuationOptions continuationOptions, IState candidateState)
            => continuationOptions switch
            {
                JobContinuationOptions.OnAnyFinishedState when candidateState.IsFinal => true,
                JobContinuationOptions.OnlyOnSucceededState when _knownFinalStates.Contains(candidateState.Name) => true,
                _ => false
            };

        private void MarkSubatomFinished(ApplyStateContext context, string atomId)
        {
            using var tr = context.Connection.CreateWriteTransaction();
            var subatomId = context.BackgroundJob.Id;
            tr.RemoveFromSet(Atom.GenerateSubAtomRemainingKeys(atomId), subatomId);
            tr.Commit();
            _logger.Debug($"Set subatom:{subatomId} of atom:{atomId} succeeded");
        }

        private void MarkAtomFinishedIfAllSucceeded(ApplyStateContext context, string atomId)
        {
            var connection = (JobStorageConnection)context.Connection;

            var jobsRemainingCount = connection.GetSetCount(Atom.GenerateSubAtomRemainingKeys(atomId));

            if (jobsRemainingCount == 0)
            {
                var alreadyRunning = connection.CheckIfAtomIsFinished(atomId);
                if (alreadyRunning) return;

                try
                {
                    using var _ = connection.AcquireDistributedLock(Atom.GenerateAtomKey(atomId), TimeSpan.Zero);

                    alreadyRunning = connection.CheckIfAtomIsFinished(atomId);
                    if (alreadyRunning) return;

                    var createdAt = context.BackgroundJob.CreatedAt;
                    var duration = (long)(DateTime.UtcNow - createdAt).TotalMilliseconds;
                    var succeededState = new SucceededState(null, 0, duration);
                    _stateChanger.ChangeState(new StateChangeContext(context.Storage, connection, atomId, succeededState));
                    _logger.Debug($"Set atom:{atomId} succeeded");
                }
                catch (DistributedLockTimeoutException)
                {
                    // Assume started
                }
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}
