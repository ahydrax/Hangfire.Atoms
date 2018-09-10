using Hangfire.Atoms.Dashboard;
using Hangfire.Atoms.States;
using Hangfire.Dashboard;

namespace Hangfire.Atoms
{
    public static class ConfigurationBootstrapperExtensions
    {
        public static IGlobalConfiguration UseAtoms(this IGlobalConfiguration configuration)
        {
            JobHistoryRenderer.AddBackgroundStateColor(AtomCreatingState.StateName, "#b3daff");
            JobHistoryRenderer.AddForegroundStateColor(AtomCreatingState.StateName, "#0066cc");

            JobHistoryRenderer.AddBackgroundStateColor(AtomCreatedState.StateName, "#b3daff");
            JobHistoryRenderer.AddForegroundStateColor(AtomCreatedState.StateName, "#0066cc");

            JobHistoryRenderer.AddBackgroundStateColor(AtomRunningState.StateName, "#b3daff");
            JobHistoryRenderer.AddForegroundStateColor(AtomRunningState.StateName, "#0066cc");

            JobHistoryRenderer.Register(AtomRunningState.StateName, AtomJobHistoryRenderer.Render);

            JobsSidebarMenu.Items.Add(AtomJobSidebar.RenderAtomJobMenu);

            return configuration;
        }
    }
}
