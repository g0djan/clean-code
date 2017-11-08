using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Markdown
{
	public class Md
	{
	    private HashSet<Tag> tags;
	    private TextTagsState state;
	    private TagsFinder finder;
	    private TagsReplacer replacer;

	    public Md()
	    {
	        tags = new HashSet<Tag>();
            state = new TextTagsState();
            finder = new TagsFinder();
            replacer = new TagsReplacer();
        }

        public string RenderToHtml(string markdown)
		{
            
			return markdown; //TODO
		}
	}

    public class TagsReplacer
    {
        private Dictionary<TagName, string> openedTagsReplacement;
        private Dictionary<TagName, string> closedTagsReplacement;

        public TagsReplacer()
        {
            openedTagsReplacement = new Dictionary<TagName, string>();
            closedTagsReplacement = new Dictionary<TagName, string>();
        }

        public string ReplaceTags(string text, List<Tag> tags)
        {
            throw new NotImplementedException();
        }
    }

    public class TagsFinder
    {
        private Dictionary<string, string> closedTag;

        public TagsFinder()
        {
            closedTag = new Dictionary<string, string>();
        }

        public Tag GetFirstTagOnSegment(string text, TextTagsState state, int startIndex, int endIndex)
        {
            throw new NotImplementedException();
        }

        private int GetIndexFirstOpenedTag(int startIndex)
        {
            throw new NotImplementedException();
        }

        private int GetIndexFirstClosedTag(int startIndex, TagName tagName)
        {
            throw new NotImplementedException();
        }
    }

    public class TextTagsState
    {
        private BitArray mask;
        private Dictionary<TagName, int> tagsNumbers;

        public TextTagsState()
        {
            mask = new BitArray(2);
            tagsNumbers = new Dictionary<TagName, int>();
        }

        public void SetInTag(TagName name)
        {
            throw new NotImplementedException();
        }

        public void SetOutTag(TagName name)
        {
            throw new NotImplementedException();
        }

        public bool IsInTag(TagName name)
        {
            throw new NotImplementedException();
        }

    }


    public class Tag
    {
        public readonly TagName name;
        public readonly int openIndex;
        public readonly int closeIndex;

        public Tag(TagName name, int openIndex, int closeIndex)
        {
            this.openIndex = openIndex;
            this.closeIndex = closeIndex;
            this.name = name;
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