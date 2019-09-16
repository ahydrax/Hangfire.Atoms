using System;
using Hangfire.Annotations;
using Hangfire.Atoms.Dashboard;
using Hangfire.Atoms.Dashboard.Pages;
using Hangfire.Atoms.States;
using Hangfire.Dashboard;
using Hangfire.Storage;

namespace Hangfire.Atoms
{
    public static class ConfigurationBootstrapperExtensions
    {
        [PublicAPI]
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
            GlobalStateHandlers.Handlers.Add(new AtomCreatedState.Handler());
            GlobalStateHandlers.Handlers.Add(subAtomHandler);
            GlobalStateHandlers.Handlers.Add(new TriggerWaitingState.Handler());

            GlobalJobFilters.Filters.Add(atomRunningHandler);
            GlobalJobFilters.Filters.Add(subAtomHandler);
        }

        private static void SetupDashboard()
        {
            // Atoms
            DashboardRoutes.Routes.AddRazorPage("/jobs/atoms", x => new AtomsPage());
            DashboardRoutes.Routes.AddClientBatchCommand("/jobs/atoms/delete", (client, jobId) => client.DeleteAtom(jobId));
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

            JobsSidebarMenu.Items.Add(AtomJobSidebar.RenderMenu);

            // Triggers
            JobHistoryRenderer.AddBackgroundStateColor(TriggerWaitingState.StateName, "#e6f7ff");
            JobHistoryRenderer.AddForegroundStateColor(TriggerWaitingState.StateName, "#006699");

            JobHistoryRenderer.Register(TriggerWaitingState.StateName, JobHistoryRenderer.NullRenderer);

            DashboardRoutes.Routes.AddRazorPage("/jobs/triggers", x => new TriggersPage());
            JobsSidebarMenu.Items.Add(TriggerJobSidebar.RenderMenu);
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
