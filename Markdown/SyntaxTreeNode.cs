using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public class SyntaxTreeNode
    {
        private readonly List<SyntaxTreeNode> children;
        public TagType TagType { get; }
        public string LeafContent { get; }

        public SyntaxTreeNode(TagType tagType, string content)
        {
            TagType = tagType;
            LeafContent = content;
            children = new List<SyntaxTreeNode>();
        }

        public void BuildSyntaxTree(State state)
        {
            foreach (var tag in Extractor.GetAllTags(state.ChangeTagType(TagType)))
            {
                var content = tag.Type == TagType.None
                    ? AccumulateLeafContent(state.ChangeSegment(tag.Start, tag.End))
                    : null;
                children.Add(new SyntaxTreeNode(tag.Type, content));
                if (tag.Type == TagType.None)
                    continue;
                var inTagState = state
                    .ChangeSegment(tag.Start, tag.End)
                    .ChangeTagType(tag.Type);
                children.Last().BuildSyntaxTree(inTagState);
            }
        }

        public IEnumerable<SyntaxTreeNode> GetAllChildren() => children;

        private string AccumulateLeafContent(State state)
        {
            var length = state.End - state.Start + 1;
            return string
                .Join("", Enumerable
                .Range(state.Start, length)
                .Select(i => state.GetLexeme(i).Content));
        }
    }

    [TestFixture]
    public class SyntaxTreeNode_Should
    {
        [Test]
        public void OnlyRoot()
        {
            var root = new SyntaxTreeNode(TagType.None, null);
            var lexemes = new[] {new Lexeme(LexemeType.Text, "content")};

            root.BuildSyntaxTree(new State(lexemes, 0, lexemes.Length - 1));
            var childs = root.GetAllChildren().ToArray();
            childs.Length.Should().Be(1);
            childs[0].LeafContent.Should().Be("content");
        }


        private SyntaxTreeNode GetTreeBasedOn2Tags(string firstMdTag, string secondMdTag)
        {
            var root = new SyntaxTreeNode(TagType.None, null);
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, firstMdTag),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, secondMdTag),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, secondMdTag),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, firstMdTag),
                new Lexeme(LexemeType.Text, "a")
            };
            root.BuildSyntaxTree(new State(lexemes, 0, lexemes.Length - 1));
            return root;
        }

        [Test]
        public void ItalicInBold_FirstLevelContainsBoldBetweenText()
        {
            var root = GetTreeBasedOn2Tags("__", "_");
            var children = root.GetAllChildren();
            children.Select(child => child.TagType).Should()
                .BeEquivalentTo(new[] {TagType.None, TagType.Bold, TagType.None});
        }

        [Test]
        public void ItalicInBold_SecondLevelContainsItalicInText()
        {
            var root = GetTreeBasedOn2Tags("__", "_");
            var childrenOfBold = root.GetAllChildren().ToArray()[1].GetAllChildren();
            childrenOfBold.Select(child => child.TagType).Should()
                .BeEquivalentTo(new[] { TagType.None, TagType.Italic, TagType.None });
        }

        [Test]
        public void SeveralItalicTags()
        {
            var root = GetTreeBasedOn2Tags("_", "_");
            root.GetAllChildren().Select(child => child.TagType).Should()
                .BeEquivalentTo(new[] {TagType.None, TagType.Italic, TagType.None, TagType.Italic, TagType.None });
        }
    }
}