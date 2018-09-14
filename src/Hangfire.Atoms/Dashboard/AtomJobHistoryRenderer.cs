using System.Collections.Generic;
using System.Text;
using Hangfire.Atoms.States;
using Hangfire.Dashboard;

namespace Hangfire.Atoms.Dashboard
{
    public static class AtomJobHistoryRenderer
    {
        public static NonEscapedString AtomRender(HtmlHelper helper, IDictionary<string, string> stateData)
        {
            var builder = new StringBuilder();

            if (stateData.ContainsKey(nameof(AtomCreatedState.AtomId)))
            {
                var atomId = stateData[nameof(AtomCreatedState.AtomId)];
                builder.Append("<dl class=\"dl-horizontal\">");
                builder.Append("<dt>Atom details:</dt>");
                builder.Append($"<dd><a href=\"/jobs/atoms/{atomId}\">{atomId}</a></dd>");
                builder.Append("</dl>");
            }

            return new NonEscapedString(builder.ToString());
        }
    }
}
