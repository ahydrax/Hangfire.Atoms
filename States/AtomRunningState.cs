using System.Collections.Generic;
using Hangfire.States;

namespace Hangfire.Atoms.States
{
    public class AtomRunningState : IState
    {
        public static readonly string StateName = "atomrunning";
        
        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>();
        public string Name => StateName;
        public string Reason => "Atomic job is running";
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;
    }
}
