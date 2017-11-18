using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class TagRenderer
    {
        private static readonly Dictionary<TagType, Tuple<string, string>> ToHtml = new Dictionary<TagType, Tuple<string, string>>
        {
            [TagType.Italic] = Tuple.Create("<em>", "</em>"),
            [TagType.Bold] = Tuple.Create("<strong>", "</strong>"),
            [TagType.BoldITalic] = Tuple.Create("<strong><em>", "</em></strong>"),
            [TagType.None] = Tuple.Create("", "")
        };

        public static string Render(SyntaxTreeNode node)
        {
            if (!node.GetAllChilds().Any())
                return node.leafContent;
            var open = ToHtml[node.Tag].Item1;
            var close = ToHtml[node.Tag].Item2;
            return open + 
                string.Join("", node.GetAllChilds().Select(Render)) +
                close;
        }
    }
}