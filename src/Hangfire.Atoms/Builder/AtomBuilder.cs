using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Atoms.States;
using Hangfire.Common;
using Hangfire.States;

namespace Hangfire.Atoms.Builder
{
    [PublicAPI]
    public class AtomBuilder : IAtomBuilder
    {
        private readonly string _atomId;
        private readonly JobStorage _jobStorage;
        private readonly IBackgroundJobClient _client;
        private readonly IState _initialState;
        private readonly Action<IAtomBuilder> _buildAtom;
        private readonly Dictionary<string, IState> _createdSubAtoms;

        public AtomBuilder(string name, JobStorage jobStorage, IBackgroundJobClient client, Action<IAtomBuilder> buildAtom, IState? initialState = null)
        {
            _client = client;
            _atomId = _client.Create(() => Atom.Running(name), new AtomCreatingState());
            _initialState = initialState ?? new AtomRunningState(_atomId);
            _buildAtom = buildAtom;
            _jobStorage = jobStorage;
            _createdSubAtoms = new Dictionary<string, IState>();
        }

        protected string CreateSubatomInternal(
            Job job,
            IState nextState,
            JobContinuationOptions continuationOptions)
        {
            var jobId = _client.Create(job, new SubAtomCreatedState(_atomId, nextState, continuationOptions));
            _createdSubAtoms.Add(jobId, nextState);
            return jobId;
        }

        protected string CreateSubatomInternal(
            [InstantHandle] Expression<Func<Task>> action,
            IState nextState,
            JobContinuationOptions continuationOptions)
            => CreateSubatomInternal(Job.FromExpression(action), nextState, continuationOptions);

        protected string CreateSubatomInternal(
            [InstantHandle] Expression<Action> action,
            IState nextState,
            JobContinuationOptions continuationOptions)
            => CreateSubatomInternal(Job.FromExpression(action), nextState, continuationOptions);

        public string Enqueue(
            [InstantHandle] Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return Enqueue(action, new EnqueuedState(), atomProgress);
        }

        public string Enqueue(
            [InstantHandle] Expression<Action> action,
            IState state,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Enqueue(
            [InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return Enqueue(action, new EnqueuedState(), atomProgress);
        }

        public string Enqueue(
            [InstantHandle] Expression<Func<Task>> action,
            IState state,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Schedule(
            [InstantHandle] Expression<Action> action,
            DateTime enqueueAt,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new ScheduledState(enqueueAt);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Schedule(
            [InstantHandle] Expression<Func<Task>> action, 
            DateTime enqueueAt,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new ScheduledState(enqueueAt);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Schedule(
            [InstantHandle] Expression<Action> action, 
            TimeSpan enqueueIn,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new ScheduledState(enqueueIn);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string Schedule(
            [InstantHandle] Expression<Func<Task>> action,
            TimeSpan enqueueIn,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new ScheduledState(enqueueIn);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public void WaitForTriggerSet(string triggerName)
        {
            var state = new TriggerWaitingState(triggerName);
            CreateSubatomInternal(() => Trigger.On(triggerName), state, JobContinuationOptions.OnAnyFinishedState);
        }

        public string OnTriggerSet(
            string triggerName, 
            [InstantHandle] Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new TriggerWaitingState(triggerName);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string OnTriggerSet(
            string triggerName,
            [InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var state = new TriggerWaitingState(triggerName);
            return CreateSubatomInternal(action, state, atomProgress);
        }

        public string ContinueJobWith(
            string parentId,
            [InstantHandle] Expression<Action> action,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return ContinueJobWith(parentId, action, new EnqueuedState(), jobContinuationOptions, atomProgress);
        }

        public string ContinueJobWith(
            string parentId,
            [InstantHandle] Expression<Action> action,
            IState state,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var nextState = new AwaitingState(parentId, state, jobContinuationOptions);

            return CreateSubatomInternal(action, nextState, atomProgress);
        }

        public string ContinueJobWith(
            string parentId,
            [InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return ContinueJobWith(parentId, action, new EnqueuedState(), jobContinuationOptions, atomProgress);
        }

        public string ContinueJobWith(
            string parentId,
            [InstantHandle] Expression<Func<Task>> action,
            IState state,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var nextState = new AwaitingState(parentId, state, jobContinuationOptions);

            return CreateSubatomInternal(action, nextState, atomProgress);
        }

        public string Enqueue<T>(
            [InstantHandle] Expression<Func<T, Task>> action, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return Enqueue(action, new EnqueuedState(), atomProgress);
        }

        public string Enqueue<T>(
            [InstantHandle] Expression<Func<T, Task>> action,
            IState state,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var job = Job.FromExpression(action);
            return CreateSubatomInternal(job, state, atomProgress);
        }

        public string Enqueue<T>(
            [InstantHandle] Expression<Action<T>> action, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return Enqueue(action, new EnqueuedState(), atomProgress);
        }

        public string Enqueue<T>(
            [InstantHandle] Expression<Action<T>> action,
            IState state,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var job = Job.FromExpression(action);
            return CreateSubatomInternal(job, state, atomProgress);
        }

        public string ContinueJobWith<T>(
            string parentId, 
            [InstantHandle] Expression<Func<T, Task>> action, 
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return ContinueJobWith(parentId, action, new EnqueuedState(), jobContinuationOptions, atomProgress);
        }

        public string ContinueJobWith<T>(
            string parentId, 
            [InstantHandle] Expression<Func<T, Task>> action, 
            IState state,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var job = Job.FromExpression(action);
            var nextState = new AwaitingState(parentId, state, jobContinuationOptions);
            return CreateSubatomInternal(job, nextState, atomProgress);
        }

        public string ContinueJobWith<T>(
            string parentId, 
            [InstantHandle] Expression<Action<T>> action, 
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            return ContinueJobWith(parentId, action, new EnqueuedState(), jobContinuationOptions, atomProgress);
        }

        public string ContinueJobWith<T>(
            string parentId, 
            [InstantHandle] Expression<Action<T>> action,
            IState state,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState)
        {
            var job = Job.FromExpression(action);
            var nextState = new AwaitingState(parentId, state, jobContinuationOptions);
            return CreateSubatomInternal(job, nextState, atomProgress);
        }

        public string Build()
        {
            try
            {
                // CREATING
                _buildAtom(this);

                // CREATED
                CreateAtomState();

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

                using (var connection = _jobStorage.GetJobStorageConnection())
                {
                    using var tr = connection.CreateJobStorageTransaction();
                    tr.RemoveSet(Atom.GenerateSubAtomKeys(_atomId));
                    var atomRemainingKeys = Atom.GenerateSubAtomRemainingKeys(_atomId);
                    foreach (var activeSubatoms in _createdSubAtoms.Where(x => !x.Value.IsFinal))
                    {
                        tr.RemoveFromSet(atomRemainingKeys, activeSubatoms.Key);
                    }
                    tr.Commit();
                }

                _client.Delete(_atomId);
                throw;
            }

            return _atomId;
        }

        private void CreateAtomState()
        {
            using (var connection = _jobStorage.GetConnection())
            {
                using var tr = connection.CreateWriteTransaction();
                var atomRemainingKeys = Atom.GenerateSubAtomRemainingKeys(_atomId);
                foreach (var activeSubatoms in _createdSubAtoms.Where(x => !x.Value.IsFinal))
                {
                    tr.AddToSet(atomRemainingKeys, activeSubatoms.Key);
                }
                tr.Commit();
            }

            _client.ChangeState(_atomId, new AtomCreatedState(_atomId));
        }
    }
}
