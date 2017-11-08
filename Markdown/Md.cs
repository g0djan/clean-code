using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Markdown
{
	public class Md
	{
		public string RenderToHtml(string markdown)
		{
			return markdown; //TODO
		}
	}

    public class Tag
    {
        public readonly TagName name ;
        public readonly int openIndex;
        public readonly int closeIndex;

        public Tag(TagName name, int openIndex, int closeIndex)
        {
            this.openIndex = openIndex;
            this.closeIndex = closeIndex;
            this.name = name;
        }
    }

    public class TagHandler
    {
        private readonly string text;
        private Dictionary<TagName, string> openedTagsReplacement;
        private Dictionary<TagName, string> closedTagsReplacement;
        private Dictionary<string, string> closedTag;
        private HashSet<Tag> tags;

        public TagHandler(string text)
        {
            this.text = text;
            tags = new HashSet<Tag>();
        }

        private void AddTagPairsOnSegment(int startIndex, int endIndex, BitArray tagsBitMask)
        {
            throw new NotImplementedException();
        }

        private int FindClosedTagPosition(int startIndex, int endIndex, TagName tagName, BitArray tagBitMask)
        {
            throw new NotImplementedException();
        }

        private Tag FindFirstTag(int startIndex, int endIndex)
        {
            throw new NotImplementedException();
        }

        public string ReplaceTags()
        {
            throw new NotImplementedException();
        }
    }

    public enum TagName
    {
        Em,
        Strong
    }

	[TestFixture]
	public class Md_ShouldRender
	{
	}
}