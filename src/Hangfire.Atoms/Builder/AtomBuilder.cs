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
        private readonly string _name;
        private readonly string _atomId;
        private readonly JobStorage _jobStorage;
        private readonly IBackgroundJobClient _client;
        private readonly Action<IAtomBuilder> _buildAtom;
        private readonly Dictionary<string, IState> _createdSubAtoms;
        private readonly List<string> _createdUtilityJobs;

        public AtomBuilder(string name, JobStorage jobStorage, IBackgroundJobClient client, Action<IAtomBuilder> buildAtom)
        {
            _name = name;
            _client = client;
            _atomId = _client.Create(() => Atom.CleanupStateOnFinish(_name, null), new AtomCreatingState());
            _buildAtom = buildAtom;
            _jobStorage = jobStorage;
            _createdSubAtoms = new Dictionary<string, IState>();
            _createdUtilityJobs = new List<string>();
        }

        private string CreateSubatomInternal(Expression<Action> action, IState nextState, JobContinuationOptions continuationOptions)
        {
            var jobId = _client.Create(Job.FromExpression(action), nextState);
            _createdSubAtoms.Add(jobId, nextState);
            var finalizationJobId = _client.ContinueWith(jobId, () => Atom.OnSubatomFinished(_name, _atomId, jobId, null), continuationOptions);
            _createdUtilityJobs.Add(finalizationJobId);

            return finalizationJobId;
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
                using (var connection = _jobStorage.GetConnection())
                {
                    using (var tr = connection.CreateWriteTransaction())
                    {
                        tr.InsertToList(Atom.JobListKey, _atomId);
                        tr.Commit();
                    }
                }

                _buildAtom(this);

                // CREATED
                using (var connection = _jobStorage.GetConnection())
                {
                    var jobData = _createdSubAtoms.Select(x => new KeyValuePair<string, string>(x.Key, Atom.Waiting));
                    connection.SetRangeInHash(Atom.GenerateSubAtomKeys(_atomId), jobData);
                }
                _client.ChangeState(_atomId, new AtomCreatedState());

                // RUNNING
                foreach (var subatom in _createdSubAtoms)
                {
                    _client.ChangeState(subatom.Key, subatom.Value);
                }
                _client.ChangeState(_atomId, new AtomRunningState());
            }
            catch
            {
                // FULL CLEANUP IN CASE OF FAIL
                foreach (var utilityJob in _createdUtilityJobs)
                {
                    _client.Delete(utilityJob);
                }

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
