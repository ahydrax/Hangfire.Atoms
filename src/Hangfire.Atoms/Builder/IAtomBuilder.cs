using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire.Annotations;

namespace Hangfire.Atoms.Builder
{
    [PublicAPI]
    public interface IAtomBuilder
    {
        string Enqueue(
            [InstantHandle] Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Enqueue(
            [InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Enqueue<T>(
            [InstantHandle] Expression<Func<T, Task>> action, 
            JobContinuationOptions atomProgress);

        string Schedule(
            [InstantHandle] Expression<Action> action,
            DateTime enqueueAt,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule(
            [InstantHandle] Expression<Func<Task>> action,
            DateTime enqueueAt,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule(
            [InstantHandle] Expression<Action> action,
            TimeSpan enqueueIn,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string Schedule(
            [InstantHandle] Expression<Func<Task>> action,
            TimeSpan enqueueIn,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        void WaitForTriggerSet(string triggerName);

        string OnTriggerSet(
            string triggerName,
            [InstantHandle] Expression<Action> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string OnTriggerSet(
            string triggerName,
            [InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string ContinueJobWith(
            string parentId,
            [InstantHandle] Expression<Action> action,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string ContinueJobWith(
            string parentId,
            [InstantHandle] Expression<Func<Task>> action,
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);

        string ContinueJobWith<T>(
            string parentId, 
            [InstantHandle] Expression<Func<T, Task>> action, 
            JobContinuationOptions jobContinuationOptions = JobContinuationOptions.OnlyOnSucceededState,
            JobContinuationOptions atomProgress = JobContinuationOptions.OnlyOnSucceededState);
        );
    }
}
