using System.Collections.Generic;
using Hangfire.States;

namespace Hangfire.Atoms.States
{
    public class AtomCreatedState : IState
    {
        public static readonly string StateName = "atomcreated";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>();

        public string Name => StateName;
        public string Reason => "Atom created.";
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;
    }
}
