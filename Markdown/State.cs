using System;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public class State
    {
        private Lexeme[] Lexemes { get; }
        public int Start { get; }
        public int End { get; }

        public TagType TagType { get; }

        public State(Lexeme[] lexemes, int start, int end, TagType type)
        {
            if (start > end)
                throw new ArgumentException();
            Lexemes = lexemes;
            Start = start;
            End = end;
            TagType = type;
        }

        public State(Lexeme[] lexemes, int start, int end)
        {
            if (start > end)
                throw new ArgumentException();
            Lexemes = lexemes;
            Start = start;
            End = end;
            TagType = TagType.None;
        }

        public Lexeme GetLexeme(int index) => Lexemes[index];

        public State ChangeTagType(TagType type) =>
            new State(Lexemes, Start, End, type);

        public State ChangeSegment(int start, int end) =>
            new State(Lexemes, start, end, TagType);
    }

    [TestFixture]
    public class State_Should
    {
        [Test]
        public void StartCanNotToBeGreaterThanEnd()
        {
            Assert.Throws<ArgumentException>(() => new State(new Lexeme[1], 1, 0));
        }

        [Test]
        public void TestIsInTag()
        {
            var state = new State(new Lexeme[1], 0, 0,
                TagType.Bold);
            (state.TagType == TagType.Bold && state.TagType != TagType.Italic).Should().BeTrue();
        }

        [Test]
        public void SwitchTag_ChangeBoldItalic()
        {
            var state = new State(new Lexeme[1], 0, 0);
            (state.ChangeTagType(TagType.BoldItalic).TagType == TagType.BoldItalic).Should().BeTrue();
        }
    }
}