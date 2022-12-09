using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    internal static class HtmlUtils
    {
        public static NonEscapedString JobIdLink(this UrlHelper helper, string jobId)
            => new NonEscapedString($"<a href=\"{helper.JobDetails(jobId)}\">{jobId}</a>");

        public static NonEscapedString AtomLink(this HtmlHelper helper, string jobAtomId)
        {
            var url = GetUrlHelper(helper);
            var href = url.To($"/jobs/atoms/{jobAtomId}");
            return new NonEscapedString($"<a href=\"{href}\">{jobAtomId}</a>");
        }

        private static UrlHelper GetUrlHelper(this HtmlHelper htmlHelper)
        {
            var page = new EmptyPage();
            _ = htmlHelper.RenderPartial(page);
            return page.Url;
        }

        private class EmptyPage : RazorPage
        {
            public override void Execute()
            {
            }
        }
    }
}
