using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    internal static class HtmlUtils
    {
        public static NonEscapedString JobIdLink(this UrlHelper helper, string jobId)
            => new NonEscapedString($"<a href=\"{helper.JobDetails(jobId)}\">{jobId}</a>");

        public static NonEscapedString AtomLink(this HtmlHelper helper, string jobAtomId)
        {
            var href = helper.Page.Url.To($"/jobs/atoms/{jobAtomId}");
            return new NonEscapedString($"<a href=\"{href}\">{jobAtomId}</a>");
        }
    }
}
