using System;
using System.Collections.Generic;

namespace Markdown
{
    public class Tag
    {
        public readonly TagName TagName;
        public readonly int OpenIndex;
        public readonly int CloseIndex;

        public Tag(string md, int openIndex, int closeIndex)
        {
            OpenIndex = openIndex;
            CloseIndex = closeIndex;
            TagName = MdToTagName[md];
        }

        public Tag(TagName tagName, int openIndex, int closeIndex)
        {
            OpenIndex = openIndex;
            CloseIndex = closeIndex;
            TagName = tagName;
        }

        public KeyValuePair<int, string>[] To2HtmlTags()
        {
            return new[]
            {
                new KeyValuePair<int, string>(OpenIndex, GetOpenHtml()),
                new KeyValuePair<int, string>(CloseIndex, GetCloseHtml())
            };
        }

        public string GetMd() => GetMd(TagName);
        public static string GetMd(TagName name) => TagNameToMd[name];

        public string GetOpenHtml() => GetOpenHtml(TagName);
        public static string GetOpenHtml(TagName name) => TagNameToOpenHtml[name];

        public string GetCloseHtml() => GetCloseHtml(TagName);
        public static string GetCloseHtml(TagName name) => TagNameToCloseHtml[name];

        public static TagName GetTagName(string tag)
        {
            if (MdToTagName.ContainsKey(tag))
                return MdToTagName[tag];
            return OpenTagToTagName.ContainsKey(tag) 
                ? OpenTagToTagName[tag] 
                : CloseTagToTagName[tag];
        }

        private static readonly Dictionary<string, TagName> MdToTagName = new Dictionary<string, TagName>
        {
            ["_"] = TagName.Em,
            ["__"] = TagName.Strong,
            ["___"] = TagName.StrongEm
        };

        private static readonly Dictionary<string, TagName> OpenTagToTagName = new Dictionary<string, TagName>
        {
            ["<em>"] = TagName.Em,
            ["<strong>"] = TagName.Strong,
            ["<strong><em>"] = TagName.StrongEm
        };

        private static readonly Dictionary<string, TagName> CloseTagToTagName = new Dictionary<string, TagName>
        {
            ["</em>"] = TagName.Em,
            ["</strong>"] = TagName.Strong,
            ["</em></strong>"] = TagName.StrongEm
        };

        private static readonly Dictionary<TagName, string> TagNameToMd = new Dictionary<TagName, string>
        {
            [TagName.Em] = "_",
            [TagName.Strong] = "__",
            [TagName.StrongEm] = "___"
        };

        private static readonly Dictionary<TagName, string> TagNameToOpenHtml = new Dictionary<TagName, string>
        {
            [TagName.Em] = "<em>",
            [TagName.Strong] = "<strong>",
            [TagName.StrongEm] = "<strong><em>"
        };

        private static readonly Dictionary<TagName, string> TagNameToCloseHtml = new Dictionary<TagName, string>
        {
            [TagName.Em] = "</em>",
            [TagName.Strong] = "</strong>",
            [TagName.StrongEm] = "</em></strong>"
        };

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var tag = obj as Tag;
            if (tag == null)
                return false;
            return TagName.Equals(tag.TagName) &&
                   OpenIndex.Equals(tag.OpenIndex) &&
                   CloseIndex.Equals(tag.CloseIndex);

        }
    }
}