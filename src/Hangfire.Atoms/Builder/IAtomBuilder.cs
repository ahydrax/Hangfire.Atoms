using System;
using System.Linq.Expressions;

namespace Hangfire.Atoms.Builder
{
    public interface IAtomBuilder
    {
        string Enqueue(Expression<Action> action, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule(Expression<Action> action, DateTime enqueueAt, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule(Expression<Action> action, TimeSpan enqueueIn, 
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        void WaitForTriggerSet(string triggerName);

        string OnTriggerSet(string triggerName, Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string ContinueJobWith(string parentId, Expression<Action> action, 
            JobContinuationOptions continuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
    }
}
