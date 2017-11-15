using System;
using System.Collections.Generic;

namespace Markdown
{
    public class TagSegment
    {
        public readonly TagName TagName;
        public readonly int OpenIndex;
        public readonly int CloseIndex;

        public TagSegment(string md, int openIndex, int closeIndex)
        {
            OpenIndex = openIndex;
            CloseIndex = closeIndex;
            TagName = Tags.GetTagName(md);
        }

        public TagSegment(TagName tagName, int openIndex, int closeIndex)
        {
            OpenIndex = openIndex;
            CloseIndex = closeIndex;
            TagName = tagName;
        }

        public KeyValuePair<int, string>[] To2HtmlTags()
        {
            return new[]
            {
                new KeyValuePair<int, string>(OpenIndex, Tags.GetOpenHtml(TagName)),
                new KeyValuePair<int, string>(CloseIndex, Tags.GetCloseHtml(TagName))
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var tag = obj as TagSegment;
            if (tag == null)
                return false;
            return TagName.Equals(tag.TagName) &&
                   OpenIndex.Equals(tag.OpenIndex) &&
                   CloseIndex.Equals(tag.CloseIndex);

        }
    }
}