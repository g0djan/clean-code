using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public class SyntaxTreeNode
    {
        private readonly List<SyntaxTreeNode> childs;
        public TagType TagType { get; }
        public string LeafContent { get; private set; }

        public SyntaxTreeNode(TagType tagType)
        {
            TagType = tagType;
            childs = new List<SyntaxTreeNode>();
        }

        public void BuildSyntaxTree(State state)
        {
            foreach (var tag in TagExtractor.GetAllTags(state))
            {
                childs.Add(new SyntaxTreeNode(tag.Type));
                if (tag.Type == TagType.None)
                {
                    childs.Last().AccumulateLeafContent(state.OnSegment(tag.Start, tag.End));
                    continue;
                }
                var inTagState = state
                    .OnSegment(tag.Start, tag.End)
                    .SwitchTag(tag.Type);
                childs.Last().BuildSyntaxTree(inTagState);
            }
        }

        public IEnumerable<SyntaxTreeNode> GetAllChilds() => childs;

        private void AccumulateLeafContent(State state)
        {
            var length = state.End - state.Start + 1;
            LeafContent = string
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
            var root = new SyntaxTreeNode(TagType.None);
            var lexemes = new[] {new Lexeme(LexemeType.Text, "content")};

            root.BuildSyntaxTree(new State(lexemes, 0, lexemes.Length - 1));
            var childs = root.GetAllChilds().ToArray();
            childs.Length.Should().Be(1);
            childs[0].LeafContent.Should().Be("content");
        }


        private SyntaxTreeNode GetTreeBasedOn2Tags(string firstMdTag, string secondMdTag)
        {
            var root = new SyntaxTreeNode(TagType.None);
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
            var childs = root.GetAllChilds();
            childs.Select(child => child.TagType).Should()
                .BeEquivalentTo(new[] {TagType.None, TagType.Bold, TagType.None});
        }

        [Test]
        public void ItalicInBold_SecondLevelContainsItalicInText()
        {
            var root = GetTreeBasedOn2Tags("__", "_");
            var childsOfBold = root.GetAllChilds().ToArray()[1].GetAllChilds();
            childsOfBold.Select(child => child.TagType).Should()
                .BeEquivalentTo(new[] { TagType.None, TagType.Italic, TagType.None });
        }

        [Test]
        public void SeveralItalicTags()
        {
            var root = GetTreeBasedOn2Tags("_", "_");
            root.GetAllChilds().Select(child => child.TagType).Should()
                .BeEquivalentTo(new[] {TagType.None, TagType.Italic, TagType.None, TagType.Italic, TagType.None });
        }
    }
}