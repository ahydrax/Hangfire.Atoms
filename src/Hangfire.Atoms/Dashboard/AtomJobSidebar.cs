using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    public static class AtomJobSidebar
    {
        public static MenuItem RenderMenu(RazorPage page)
        {
            return new MenuItem("Atoms running", page.Url.To("/jobs/atoms"))
            {
                Active = page.RequestPath.StartsWith("/jobs/atoms"),
                Metric = new DashboardMetric("atoms-count", AtomsCount)
            };
        }

        private static Metric AtomsCount(RazorPage page)
        {
            long atomsCount;
            using (var connection = page.Storage.GetJobStorageConnection())
            {
                atomsCount = connection.GetListCount(Atom.JobListKey);
            }
            return new Metric(atomsCount)
            {
                Title = "Atoms",
                Style = atomsCount == 0 ? MetricStyle.Default : MetricStyle.Warning
            };
        }
    }
}
