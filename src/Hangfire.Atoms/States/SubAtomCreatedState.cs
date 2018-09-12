using System.Collections.Generic;
using Hangfire.States;
using Newtonsoft.Json;

namespace Hangfire.Atoms.States
{
    public class SubAtomCreatedState : IState
    {
        public static readonly string StateName = "Subatom created";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>
        {
            {nameof(AtomId), AtomId},
            {nameof(NextState), JsonUtils.Serialize(NextState)}
        };

        [JsonConstructor]
        public SubAtomCreatedState(string atomId, IState nextState)
        {
            AtomId = atomId;
            NextState = nextState;
        }

        public string AtomId { get; }
        public IState NextState { get; }
        public string Name => StateName;
        public string Reason => "Created as part of atom.";
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;
    }
}
