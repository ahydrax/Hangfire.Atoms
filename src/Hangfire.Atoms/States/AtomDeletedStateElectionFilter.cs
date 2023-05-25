using System;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms.States
{
    internal sealed class AtomDeletedStateElectionFilter : JobFilterAttribute, IApplyStateFilter
    {
        private readonly ILog _logger = LogProvider.For<ContinuationsSupportAttribute>();

        private readonly IBackgroundJobStateChanger _stateChanger;

        public AtomDeletedStateElectionFilter() : this(new BackgroundJobStateChanger())
        {
        }

        public AtomDeletedStateElectionFilter(IBackgroundJobStateChanger stateChanger)
        {
            _stateChanger = stateChanger ?? throw new ArgumentNullException(nameof(stateChanger));
            Order = 900;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is DeletedState)
            {
                var jsc = (JobStorageConnection)context.Connection;
                var jst = (JobStorageTransaction)transaction;

                var isAtom = jsc.GetIfJobIsAtom(context.BackgroundJob.Id);
                if (isAtom) // we deal with atom itself
                {
                    var atomId = context.BackgroundJob.Id;
                    DeleteAsAtom(jsc, context.Storage, atomId);
                }
                else // it's just subatom
                {
                    var subatomId = context.BackgroundJob.Id;
                    var atomId = jsc.GetAtomId(subatomId);

                    if (atomId != null)
                    {
                        DeleteAsSubatom(jst, atomId, subatomId);
                    }
                }
            }
        }

        private void DeleteAsAtom(JobStorageConnection jsc, JobStorage storage, string atomId)
        {
            var subatomIds = jsc.GetAllItemsFromSet(Atom.GenerateSubAtomKeys(atomId));

            foreach (var subatomId in subatomIds)
            {
                var context = new StateChangeContext(storage, jsc, subatomId, new DeletedState());
                _stateChanger.ChangeState(context);
            }
        }

        private void DeleteAsSubatom(JobStorageTransaction jst, string atomId, string subatomId)
        {
            jst.RemoveFromSet(Atom.GenerateSubAtomKeys(atomId), subatomId);
            jst.RemoveFromSet(Atom.GenerateSubAtomRemainingKeys(atomId), subatomId);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}
