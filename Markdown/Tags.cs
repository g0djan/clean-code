using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    class Tags
    {
        public static void InitNewTag(TagName name, string md, string openHtml, string closeHtml)
        {
            MdToTagName[md] = name;
            OpenTagToTagName[openHtml] = name;
            CloseTagToTagName[closeHtml] = name;
            TagNameToMd[name] = md;
            TagNameToOpenHtml[name] = openHtml;
            TagNameToCloseHtml[name] = closeHtml;
        }
        
        public static string GetMd(TagName name) => TagNameToMd[name];
        
        public static string GetOpenHtml(TagName name) => TagNameToOpenHtml[name];
        
        public static string GetCloseHtml(TagName name) => TagNameToCloseHtml[name];

        public static TagName GetTagName(string tag)
        {
            if (MdToTagName.ContainsKey(tag))
                return MdToTagName[tag];
            return OpenTagToTagName.ContainsKey(tag)
                ? OpenTagToTagName[tag]
                : CloseTagToTagName[tag];
        }

        private static readonly Dictionary<string, TagName> MdToTagName = new Dictionary<string, TagName>();

        private static readonly Dictionary<string, TagName> OpenTagToTagName = new Dictionary<string, TagName>();

        private static readonly Dictionary<string, TagName> CloseTagToTagName = new Dictionary<string, TagName>();

        private static readonly Dictionary<TagName, string> TagNameToMd = new Dictionary<TagName, string>();

        private static readonly Dictionary<TagName, string> TagNameToOpenHtml = new Dictionary<TagName, string>();

        private static readonly Dictionary<TagName, string> TagNameToCloseHtml = new Dictionary<TagName, string>();
    }
}
