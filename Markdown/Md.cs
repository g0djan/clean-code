using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;


namespace Markdown
{
	public class Md
	{
	    private HashSet<TagSegment> tags;
	    private TextTagsState state;
	    private TagsFinder finder;
	    private UnderliningReplacer replacer;

	    public Md()
	    {
	        tags = new HashSet<TagSegment>();
            finder = new TagsFinder();
            replacer = new UnderliningReplacer();
            Tags.InitNewTag(TagName.Em, "_", "<em>", "</em>");
            Tags.InitNewTag(TagName.Strong, "__", "<strong>", "</strong>");
            Tags.InitNewTag(TagName.StrongEm, "___", "<strong><em>", "</em></strong>");
        }

        public string RenderToHtml(string markdown)
		{
            state = new TextTagsState(markdown, 0, markdown.Length - 1);
            return replacer.ReplaceTags(markdown, GetAllTags(state).ToList()); //TODO
		}

	    private IEnumerable<TagSegment> GetAllTags(TextTagsState state)
	    {
            var nextStates = new Stack<TextTagsState>();
	        for (var index = 0; index < state.Text.Length; index++)
	        {
	            var tag = finder.GetFirstTagOnSegment(state);
	            if (tag == null)
	            {
                    if (nextStates.Count == 0)
                        break;
	                index = nextStates.Peek().Start - 1;
	                state = nextStates.Pop();
	                continue;
	            }
	            yield return tag;
	            var newStart = tag.CloseIndex + Tags.GetMd(tag.TagName).Length;
                if (newStart < state.End)
                    nextStates.Push(state.ChangeSegment(newStart, state.End));
	            index = tag.OpenIndex + Tags.GetMd(tag.TagName).Length;
                if (index <= state.End - 1)
                    state = state.SwitchTag(tag.TagName).ChangeSegment(index, tag.CloseIndex - 1);
	            index -= 1;
	        }
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