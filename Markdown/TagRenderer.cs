using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public static class TagRenderer
    {
        private static readonly Dictionary<TagType, Tuple<string, string>> ToHtml = 
            new Dictionary<TagType, Tuple<string, string>>
        {
            [TagType.Italic] = Tuple.Create("<em>", "</em>"),
            [TagType.Bold] = Tuple.Create("<strong>", "</strong>"),
            [TagType.BoldItalic] = Tuple.Create("<strong><em>", "</em></strong>"),
            [TagType.None] = Tuple.Create("", "")
        };

        public static string Render(SyntaxTreeNode node)
        {
            if (!node.GetAllChildren().Any())
                return node.LeafContent;
            var open = ToHtml[node.TagType].Item1;
            var close = ToHtml[node.TagType].Item2;
            return open + string.Join("", node.GetAllChildren().Select(Render)) + close;
        }
    }
}