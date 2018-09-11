namespace Hangfire.Atoms.Dashboard.Pages
{
    internal partial class AtomDetailsPage
    {
        public string JobId { get; }

        public AtomDetailsPage(string jobId)
        {
            JobId = jobId;
        }
    }
}
