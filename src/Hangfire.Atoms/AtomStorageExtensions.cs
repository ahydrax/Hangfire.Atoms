using System;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Atoms
{
    internal static class AtomStorageExtensions
    {
        public static string GetAtomId(this IStorageConnection connection, string subatomId)
            => connection.GetJobParameter(subatomId, Atom.ParameterAtomId);

        public static void SetAtomId(this IStorageConnection connection, string subatomId, string atomId)
            => connection.SetJobParameter(subatomId, Atom.ParameterAtomId, atomId);

        public static JobContinuationOptions GetSubAtomContinuationOption(this IStorageConnection connection, string subatomId)
        {
            var continuationOptionsValue = connection.GetJobParameter(subatomId, Atom.ParameterSubatomContinuation);
            Enum.TryParse(continuationOptionsValue, out JobContinuationOptions continuationOptions);
            return continuationOptions;
        }

        public static void SetSubAtomContinuation(this IStorageConnection connection, string subatomId, JobContinuationOptions continuationOptions)
        {
            var serializedOptions = continuationOptions.ToString("G");
            connection.SetJobParameter(subatomId, Atom.ParameterSubatomContinuation, serializedOptions);
        }

        public static void SetJobIsAtom(this IStorageConnection connection, string atomId)
            => connection.SetJobParameter(atomId, Atom.ParameterIsAtom, "true");

        public static bool GetIfJobIsAtom(this IStorageConnection connection, string atomId)
            => connection.GetJobParameter(atomId, Atom.ParameterIsAtom) == "true";

        public static bool CheckIfAtomIsFinished(this JobStorageConnection connection, string atomId)
        {
            var atomState = connection.GetStateData(atomId);
            return atomState.Name == SucceededState.StateName;
        }
    }
}
