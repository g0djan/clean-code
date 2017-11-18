using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public class SyntaxTreeNode
    {
        private List<SyntaxTreeNode> childs;
        public TagType Tag { get; }
        public string leafContent { get; private set; }

        public SyntaxTreeNode(TagType tag)
        {
            Tag = tag;
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
               childs.Last().BuildSyntaxTree(state.OnSegment(tag.Start, tag.End).SwitchTag(tag.Type));
            }
        }

        public IEnumerable<SyntaxTreeNode> GetAllChilds() => childs;

        private void AccumulateLeafContent(State state)
        {
            leafContent = string.Join("",
                Enumerable.Range(state.start, state.end - state.start + 1)
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
            childs[0].leafContent.Should().Be("content");
        }

        [Test]
        public void TwoLevels()
        {
            var root = new SyntaxTreeNode(TagType.None);
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "a")
            };
            root.BuildSyntaxTree(new State(lexemes, 0, lexemes.Length - 1));
            var childs = root.GetAllChilds().ToArray();
            childs.Length.Should().Be(3);

            childs = childs[1].GetAllChilds().ToArray();
            childs.Length.Should().Be(3);
            childs[1].Tag.Should().Be(TagType.Italic);
        }
    }
}