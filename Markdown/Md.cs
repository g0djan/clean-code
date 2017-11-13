using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;


namespace Markdown
{
	public class Md
	{
	    private HashSet<Tag> tags;
	    private TextTagsState state;
	    private TagsFinder finder;
	    private UnderliningReplacer replacer;

	    public Md()
	    {
	        tags = new HashSet<Tag>();
            finder = new TagsFinder();
            replacer = new UnderliningReplacer();
        }

        public string RenderToHtml(string markdown)
		{
            state = new TextTagsState(markdown, 0, markdown.Length - 1);
		    var tags = new List<Tag>();
            AddAllTags(state, tags);
			return replacer.ReplaceTags(markdown, tags); //TODO
		}

	    private void AddAllTags(TextTagsState textState, List<Tag> tags)
	    {
	        var last = finder.GetFirstTagOnSegment(textState);
            if (last == null)
                return;
            tags.Add(last);
	        var insideStart = last.OpenIndex + Tag.GetMd(last.TagName).Length;
	        var insideEnd = last.CloseIndex - 1;
	        var outsideStart = last.CloseIndex + Tag.GetMd(last.TagName).Length;
            AddAllTags(textState.ChangeSegment(insideStart, insideEnd).SwitchTag(last.TagName), tags);
            if (outsideStart <= textState.End)
                AddAllTags(textState.ChangeSegment(outsideStart, textState.End), tags);
	    }
	}


    [TestFixture]
	public class Md_ShouldRender
	{
	    private Md markdown;

	    [SetUp]
	    public void SetUp()
	    {
	        markdown = new Md();
	    }

	    [TestCase("_simple_", TestName = "Just Em tag", ExpectedResult = "<em>simple</em>")]
	    [TestCase("__simple__", TestName = "Just Strong tag", ExpectedResult = "<strong>simple</strong>")]
	    [TestCase("t__t__t_t_", TestName = "Several tags", ExpectedResult = "t<strong>t</strong>t<em>t</em>")]
	    [TestCase("t", TestName = "No tags", ExpectedResult = "t")]
	    [TestCase("t_", TestName = "No paired underlining is not tag", ExpectedResult = "t_")]
	    [TestCase("___simple___", TestName = "Just Strong Em tag", ExpectedResult = "<strong><em>simple</em></strong>")]
	    [TestCase("__a_b_c__", TestName = "Em in Strong is active", ExpectedResult = "<strong>a<em>b</em>c</strong>")]
	    [TestCase("_a__b__c_", TestName = "Strong in em is not active", ExpectedResult = "<em>a__b__c</em>")]
	    [TestCase("____a____", TestName = "Ignore more than 3 consecutive underlinig", ExpectedResult = "____a____")]
	    [TestCase("__a __", TestName = "Before closing underlining mustn't be space", ExpectedResult = "__a __")]
	    [TestCase("__ a__", TestName = "After opening underlining mustn't be space", ExpectedResult = "__ a__")]
	    [TestCase("__1a__", TestName = "No digits opening tags", ExpectedResult = "__1a__")]
	    [TestCase("__a1__", TestName = "No digits closing tags", ExpectedResult = "__a1__")]
	    [TestCase("__a_a_a__a___a___", TestName = "Several embedded tags", 
            ExpectedResult = "<strong>a<em>a</em>a</strong>a<strong><em>a</em></strong>")]
	    public string TestRenderToHtml(string md) => 
            markdown.RenderToHtml(md);
	}
}