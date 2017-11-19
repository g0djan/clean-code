using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                   UnderliningTagChecker.IsThereStartsOpenedTag(state) &&
                   !IsInItalicTag(state);
        }

        public bool HasClosed(State state, out int? index)
        {
            var length = state.End - state.Start + 1;
            index = Enumerable.Range(state.Start, length)
                .Cast<int?>()
                .FirstOrDefault(i => 
                state.GetLexeme(i.Value).Content == MarkDown &&
                UnderliningTagChecker.IsThereStartsClosedTag(state.ChangeSegment(i.Value, state.End)) &&
                !IsInItalicTag(state.ChangeSegment(i.Value, state.End)));
            return index != null;
        }

        public Tag ExtractTag(State state) => 
            new Tag(Type, state.Start, state.End);

        private bool IsInItalicTag(State state) => 
            state.IsInTag(TagType.Italic) || state.IsInTag(TagType.BoldItalic);
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
            var mask = new BitArray(new[] {false, false, true, false});
            tagItalicExtractor.StartsAt(new State(lexemes, 1, lexemes.Length - 2, mask))
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
