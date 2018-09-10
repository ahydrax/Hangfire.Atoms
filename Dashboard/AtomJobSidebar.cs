using System.Globalization;
using Hangfire.Dashboard;
using Hangfire.Storage;

namespace Hangfire.Atoms.Dashboard
{
    public static class AtomJobSidebar
    {
        public static MenuItem RenderAtomJobMenu(RazorPage page)
        {
            return new MenuItem("Atoms", page.Url.To("/jobs/atoms"))
            {
                Active = page.RequestPath.StartsWith("/jobs/atoms"),
                Metric = new DashboardMetric("atoms-count", AtomsCount)
            };
        }

        private static Metric AtomsCount(RazorPage page)
        {
            var val = "???";

            using (var connection = page.Storage.GetConnection())
            {
                if (connection is JobStorageConnection jsc)
                {
                    val = jsc.GetListCount(Atom.JobListKey).ToString(CultureInfo.InvariantCulture);
                }
            }

            return new Metric(val);
        }
    }
}
