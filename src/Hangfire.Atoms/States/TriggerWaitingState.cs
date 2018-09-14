using System.Collections.Generic;
using Hangfire.States;
using Hangfire.Storage;
using Newtonsoft.Json;

namespace Hangfire.Atoms.States
{
    public class TriggerWaitingState : IState
    {
        public static readonly string StateName = "Trigger waiting";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>
        {
            {nameof(TriggerName), TriggerName}
        };

        [JsonConstructor]
        public TriggerWaitingState(string triggerName)
        {
            TriggerName = triggerName;
        }

        public string TriggerName { get; }
        public string Name => StateName;
        public string Reason { get; }
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        internal class Handler : IStateHandler
        {
            public string StateName => TriggerWaitingState.StateName;

            public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.NewState is TriggerWaitingState state)
                {
                    var triggerJobId = context.BackgroundJob.Id;
                    var triggerName = state.TriggerName;
                    var triggerKey = Trigger.GenerateTriggerKey(triggerName);

                    context.Connection.SetJobParameter(triggerJobId, Trigger.ParameterName, triggerName);

                    transaction.InsertToList(Trigger.JobListKey, triggerJobId);
                    transaction.InsertToList(triggerKey, triggerJobId);
                }
            }

            public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
            {
                if (context.OldStateName == TriggerWaitingState.StateName)
                {
                    var triggerJobId = context.BackgroundJob.Id;
                    var triggerName = context.Connection.GetJobParameter(triggerJobId, Trigger.ParameterName);
                    var triggerKey = Trigger.GenerateTriggerKey(triggerName);

                    transaction.RemoveFromList(triggerKey, triggerJobId);
                    transaction.RemoveFromList(Trigger.JobListKey, triggerJobId);
                }
            }
        }
    }
}
