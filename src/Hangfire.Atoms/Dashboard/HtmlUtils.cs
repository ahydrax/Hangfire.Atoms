using System;
using System.Reflection;
using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    internal static class HtmlUtils
    {
        private static readonly FieldInfo PageField = typeof(HtmlHelper).GetField("_page", BindingFlags.NonPublic | BindingFlags.Instance)
                                                      ?? throw new MissingFieldException(typeof(HtmlHelper).FullName, "_page");

        public static NonEscapedString JobIdLink(this UrlHelper helper, string jobId)
            => new NonEscapedString($"<a href=\"{helper.JobDetails(jobId)}\">{jobId}</a>");

        public static NonEscapedString AtomLink(this HtmlHelper helper, string jobAtomId)
        {
            var url = ((RazorPage)PageField.GetValue(helper)).Url;
            var href = url.To($"/jobs/atoms/{jobAtomId}");
            return new NonEscapedString($"<a href=\"{href}\">{jobAtomId}</a>");
        }
    }
}
