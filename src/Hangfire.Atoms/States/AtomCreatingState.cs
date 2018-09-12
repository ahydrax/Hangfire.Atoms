using System.Collections.Generic;
using Hangfire.States;

namespace Hangfire.Atoms.States
{
    public class AtomCreatingState : IState
    {
        public static readonly string StateName = "Atom creating";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>();
        
        public string Name => StateName;
        public string Reason => "Waiting until atom created";
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;
    }
}
