using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    internal static class HtmlUtils
    {
        public static NonEscapedString JobIdLink(this UrlHelper helper, string jobId)
            => new NonEscapedString($"<a href=\"{helper.JobDetails(jobId)}\">{jobId}</a>");

        public static NonEscapedString AtomLink(this HtmlHelper helper, string jobAtomId)
            => new NonEscapedString($"<a href=\"/jobs/atoms/{jobAtomId}\">{jobAtomId}</a>");
    }
}
