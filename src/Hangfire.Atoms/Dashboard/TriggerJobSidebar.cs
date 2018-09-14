using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    public class TriggerJobSidebar
    {
        public static MenuItem RenderMenu(RazorPage page)
        {
            return new MenuItem("Triggers", page.Url.To("/jobs/triggers"))
            {
                Active = page.RequestPath.StartsWith("/jobs/triggers"),
                Metric = new DashboardMetric("triggers-count", TriggersCount)
            };
        }

        private static Metric TriggersCount(RazorPage page)
        {
            long triggersCount;
            using (var connection = page.Storage.GetJobStorageConnection())
            {
                triggersCount = connection.GetListCount(Trigger.JobListKey);
            }
            return new Metric(triggersCount)
            {
                Title = "Triggers",
                Style = triggersCount == 0 ? MetricStyle.Default : MetricStyle.Info
            };
        }
    }
}
