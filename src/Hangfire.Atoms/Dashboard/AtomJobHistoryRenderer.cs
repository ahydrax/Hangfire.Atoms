using System.Collections.Generic;
using System.Text;
using Hangfire.Atoms.States;
using Hangfire.Dashboard;
using Hangfire.States;

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
                builder.Append($"<dd>{helper.AtomLink(atomId)}</dd>");
                builder.Append("</dl>");
            }

            if (stateData.ContainsKey(nameof(SubAtomCreatedState.ContinuationOptions)))
            {
                var continuationOption = stateData[nameof(SubAtomCreatedState.ContinuationOptions)];
                builder.Append("<dl class=\"dl-horizontal\">");
                builder.Append("<dt>Atom progress:</dt>");
                builder.Append($"<dd>{continuationOption}</dd>");
                builder.Append("</dl>");
            }

            if (stateData.ContainsKey(nameof(SubAtomCreatedState.NextState)))
            {
                var state = JsonUtils.Deserialize<IState>(stateData[nameof(SubAtomCreatedState.NextState)]);
                builder.Append("<dl class=\"dl-horizontal\">");
                builder.Append("<dt>Next state:</dt>");
                builder.Append($"<dd>{state.Name}</dd>");
                builder.Append("</dl>");
            }

            return new NonEscapedString(builder.ToString());
        }
    }
}
