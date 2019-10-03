using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire.Annotations;

namespace Hangfire.Atoms.Builder
{
    public interface IAtomBuilder
    {
        string Enqueue([NotNull, InstantHandle] Expression<Action> action, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
        string Enqueue([NotNull, InstantHandle] Expression<Func<Task>> action, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule([NotNull, InstantHandle] Expression<Action> action, DateTime enqueueAt, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
        string Schedule([NotNull, InstantHandle] Expression<Func<Task>> action, DateTime enqueueAt, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule([NotNull, InstantHandle] Expression<Action> action, TimeSpan enqueueIn, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
        string Schedule([NotNull, InstantHandle] Expression<Func<Task>> action, TimeSpan enqueueIn, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        void WaitForTriggerSet(string triggerName);

        string OnTriggerSet(string triggerName, [NotNull, InstantHandle] Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
        string OnTriggerSet(string triggerName, [NotNull, InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string ContinueJobWith(string parentId, [NotNull, InstantHandle] Expression<Action> action, 
            JobContinuationOptions continuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
        string ContinueJobWith(string parentId, [NotNull, InstantHandle] Expression<Func<Task>> action, 
            JobContinuationOptions continuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
    }
}
