using System;
using Hangfire.Atoms.Dashboard;
using Hangfire.Atoms.Dashboard.Pages;
using Hangfire.Atoms.States;
using Hangfire.Dashboard;
using Hangfire.Storage;

namespace Hangfire.Atoms
{
    public static class ConfigurationBootstrapperExtensions
    {
        public static IGlobalConfiguration UseAtoms(this IGlobalConfiguration configuration)
        {
            ThrowIfStorageIsNotSupported();

            JobHistoryRenderer.AddBackgroundStateColor(AtomCreatingState.StateName, "#b3daff");
            JobHistoryRenderer.AddForegroundStateColor(AtomCreatingState.StateName, "#0066cc");

            JobHistoryRenderer.AddBackgroundStateColor(AtomCreatedState.StateName, "#b3daff");
            JobHistoryRenderer.AddForegroundStateColor(AtomCreatedState.StateName, "#0066cc");

            JobHistoryRenderer.AddBackgroundStateColor(AtomRunningState.StateName, "#b3daff");
            JobHistoryRenderer.AddForegroundStateColor(AtomRunningState.StateName, "#0066cc");

            JobHistoryRenderer.Register(AtomRunningState.StateName, AtomJobHistoryRenderer.Render);

            JobsSidebarMenu.Items.Add(AtomJobSidebar.RenderAtomJobMenu);

            DashboardRoutes.Routes.AddRazorPage("/jobs/atoms", x => new AtomsPage());
            DashboardRoutes.Routes.AddRazorPage("/jobs/atoms/(?<JobId>.+)", x => new AtomDetailsPage(x.Groups["JobId"].Value));

            return configuration;
        }

        private static void ThrowIfStorageIsNotSupported()
        {
            if (JobStorage.Current == null) throw new InvalidOperationException("JobStorage.Current == null");

            using (var connection = JobStorage.Current.GetConnection())
            {
                var jsc = connection as JobStorageConnection;
                if (jsc == null)
                    throw new InvalidOperationException(
                        "JobStorage.Current.GetConnection() doesn't implement JobStorageConnection");

                using (var tr = connection.CreateWriteTransaction())
                {
                    var jst = tr as JobStorageTransaction;
                    if (jst == null)
                        throw new InvalidOperationException(
                            "JobStorage.Current.GetConnection().CreateWriteTransaction() doesn't implement JobStorageTransaction");
                }
            }
        }
    }
}
