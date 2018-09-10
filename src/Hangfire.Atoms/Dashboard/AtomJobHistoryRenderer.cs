using System.Collections.Generic;
using System.Text;
using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    public static class AtomJobHistoryRenderer
    {
        public static NonEscapedString Render(HtmlHelper helper, IDictionary<string, string> stateData)
        {
            var builder = new StringBuilder();

            builder.Append("<dl class=\"dl-horizontal\">");
            //TODO
            builder.Append("</dl>");

            return new NonEscapedString(builder.ToString());
        }
    }
}
