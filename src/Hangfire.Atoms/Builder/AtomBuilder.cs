using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hangfire.Atoms.States;
using Hangfire.Common;
using Hangfire.States;

namespace Hangfire.Atoms.Builder
{
    public class AtomBuilder : IAtomBuilder
    {
        private readonly string _atomId;
        private readonly JobStorage _jobStorage;
        private readonly IBackgroundJobClient _client;
        private readonly IState _initialState;
        private readonly Action<IAtomBuilder> _buildAtom;
        private readonly Dictionary<string, IState> _createdSubAtoms;

        public AtomBuilder(string name, JobStorage jobStorage, IBackgroundJobClient client, Action<IAtomBuilder> buildAtom, IState initialState = null)
        {
            _client = client;
            _atomId = _client.Create(() => Atom.Running(name), new AtomCreatingState());
            _initialState = initialState ?? new AtomRunningState(_atomId);
            _buildAtom = buildAtom;
            _jobStorage = jobStorage;
            _createdSubAtoms = new Dictionary<string, IState>();
        }

        private string CreateSubatomInternal(Expression<Action> action, IState nextState, JobContinuationOptions continuationOptions)
        {
            var jobId = _client.Create(Job.FromExpression(action), new SubAtomCreatedState(_atomId, continuationOptions, nextState));
            _createdSubAtoms.Add(jobId, nextState);

            return jobId;
        }

        public string Enqueue(Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new EnqueuedState();
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Schedule(Expression<Action> action, DateTime enqueueAt,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new ScheduledState(enqueueAt);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Schedule(Expression<Action> action, TimeSpan enqueueIn,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new ScheduledState(enqueueIn);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string ContinueWith(string parentId, Expression<Action> action,
            JobContinuationOptions continuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new AwaitingState(parentId, new EnqueuedState(), continuationOptions);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Build()
        {
            try
            {
                // CREATING
                _buildAtom(this);

                // CREATED
                _client.ChangeState(_atomId, new AtomCreatedState(_atomId));

                // RUN
                _client.ChangeState(_atomId, _initialState);
            }
            catch
            {
                // FULL CLEANUP IN CASE OF FAIL
                foreach (var createdJobId in _createdSubAtoms)
                {
                    _client.Delete(createdJobId.Key);
                }

                using (var connection = _jobStorage.GetConnection())
                {
                    using (var tr = connection.CreateWriteTransaction())
                    {
                        tr.RemoveHash(Atom.GenerateSubAtomKeys(_atomId));
                    }
                }

                _client.Delete(_atomId);
                throw;
            }

            return _atomId;
        }
    }
}
