using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    class TagExtractor : ITagExtractor
    {
        public string MarkDown { get; }
        public TagType Type { get; }

        private static readonly Dictionary<TagType, string> toMarkDown = new Dictionary<TagType, string>
        {
            [TagType.Italic] = "_",
            [TagType.Bold] = "__",
            [TagType.BoldItalic] = "___"
        };

        public TagExtractor(TagType type)
        {
            MarkDown = toMarkDown[type];
            Type = type;
        }

        public bool StartsAt(State state)
        {
            return state.GetLexeme(state.Start).Content == MarkDown &&
                   state.IsThereStartsOpenedTag() &&
                   !IsInItalicTag(state);
        }

        public bool HasClosed(State state, out int index)
        {
            var length = state.End - state.Start + 1;
            var closedIndex = -1;
            if (Enumerable.Range(state.Start, length)
                .Any(i => state.GetLexeme(closedIndex = i).Content == MarkDown &&
                          state.ChangeSegment(i, state.End).IsThereStartsClosedTag() &&
                          !IsInItalicTag(state.ChangeSegment(i, state.End))))
            {
                index = closedIndex;
                return true;
            }
            index = -1;
            return false;
        }

        public Tag ExtractTag(State state) => 
            new Tag(Type, state.Start, state.End);

        private bool IsInItalicTag(State state) => 
            state.TagType == TagType.Italic || state.TagType == TagType.BoldItalic;
    }

    [TestFixture]
    public class TagItalicExtractor_Should
    {
        private TagExtractor tagItalicExtractor;

        [SetUp]
        public void SetUp()
        {
            tagItalicExtractor = new TagExtractor(TagType.Italic);
        }

        [Test]
        public void NoItalicLexeme_DoesNotStartsAt()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "text"),
                new Lexeme(LexemeType.Underlining, "__")
            };
            tagItalicExtractor.StartsAt(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeFalse();
        }

        [Test]
        public void DoesNotStartsAt_InItalic()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "___"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "__")
            };
            tagItalicExtractor.StartsAt(new State(lexemes, 1, lexemes.Length - 2, TagType.BoldItalic))
                .Should().BeFalse();
        }

        [Test]
        public void NoPairedTag_HasNotClosed()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "__")
            };
            tagItalicExtractor.HasClosed(new State(lexemes, 2, 2), out var index)
                .Should().BeFalse();
        }

        [Test]
        public void ReturnFirstClosedIndex()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_")
            };
            tagItalicExtractor.HasClosed(new State(lexemes, 2, 4), out var index);
            index.Should().Be(2);
        }
    }
}
