using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    public static class AtomJobSidebar
    {
        public static MenuItem RenderAtomJobMenu(RazorPage arg)
        {
            // TODO
            return new MenuItem("Atoms", arg.Url.To("/jobs/atoms"));
        }
    }
}
