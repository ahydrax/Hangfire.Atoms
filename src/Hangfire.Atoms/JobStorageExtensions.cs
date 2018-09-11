using Hangfire.Storage;

namespace Hangfire.Atoms
{
    internal static class JobStorageExtensions
    {
        public static JobStorageConnection GetJobStorageConnection(this JobStorage jobStorage)
            => (JobStorageConnection)jobStorage.GetConnection();

        public static JobStorageTransaction CreateJobStorageTransaction(this JobStorageConnection jobStorageConnection)
            => (JobStorageTransaction)jobStorageConnection.CreateWriteTransaction();
    }
}
