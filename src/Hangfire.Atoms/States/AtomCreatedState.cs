using System.Collections.Generic;
using Hangfire.States;

namespace Hangfire.Atoms.States
{
    public class AtomCreatedState : IState
    {
        public static readonly string StateName = "Atom created";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>
        {
            {nameof(AtomId), AtomId},
        };

        public AtomCreatedState(string atomId)
        {
            AtomId = atomId;
        }

        public string AtomId { get; }
        public string Name => StateName;
        public string Reason { get; }
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;
    }
}
