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

            SetupAtomMachinery();
            SetupDashboard();

            return configuration;
        }

        private static void SetupAtomMachinery()
        {
            var atomRunningHandler = new AtomRunningState.Handler();
            var subAtomHandler = new SubAtomCreatedState.Handler();

            GlobalStateHandlers.Handlers.Add(atomRunningHandler);
            GlobalStateHandlers.Handlers.Add(subAtomHandler);

            GlobalJobFilters.Filters.Add(atomRunningHandler);
            GlobalJobFilters.Filters.Add(subAtomHandler);
        }

        private static void SetupDashboard()
        {
            DashboardRoutes.Routes.AddRazorPage("/jobs/atoms", x => new AtomsPage());
            DashboardRoutes.Routes.AddRazorPage("/jobs/atoms/(?<JobId>.+)", x => new AtomDetailsPage(x.Groups["JobId"].Value));

            JobHistoryRenderer.AddBackgroundStateColor(AtomCreatingState.StateName, "#e6f7ff");
            JobHistoryRenderer.AddForegroundStateColor(AtomCreatingState.StateName, "#006699");

            JobHistoryRenderer.AddBackgroundStateColor(SubAtomCreatedState.StateName, "#e6f2ff");
            JobHistoryRenderer.AddForegroundStateColor(SubAtomCreatedState.StateName, "#0066cc");

            JobHistoryRenderer.AddBackgroundStateColor(AtomCreatedState.StateName, "#e6f2ff");
            JobHistoryRenderer.AddForegroundStateColor(AtomCreatedState.StateName, "#0066cc");

            JobHistoryRenderer.AddBackgroundStateColor(AtomRunningState.StateName, "#fff5e6");
            JobHistoryRenderer.AddForegroundStateColor(AtomRunningState.StateName, "#ff9900");

            JobHistoryRenderer.Register(SubAtomCreatedState.StateName, AtomJobHistoryRenderer.AtomRender);
            JobHistoryRenderer.Register(AtomCreatingState.StateName, JobHistoryRenderer.NullRenderer);
            JobHistoryRenderer.Register(AtomCreatedState.StateName, AtomJobHistoryRenderer.AtomRender);
            JobHistoryRenderer.Register(AtomRunningState.StateName, AtomJobHistoryRenderer.AtomRender);

            JobsSidebarMenu.Items.Add(AtomJobSidebar.RenderAtomJobMenu);
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
