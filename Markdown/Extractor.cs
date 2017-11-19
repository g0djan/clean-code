using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public static class Extractor
    {
        public static IEnumerable<Tag> GetAllTags(State state)
        {
            var lastClosedTagIndex = state.Start - 1;
            for (var i = state.Start; i <= state.End; i++)
            {
                foreach (TagType type in Enum.GetValues(typeof(TagType)))
                {
                    if (type == TagType.None) break;
                    var extractor = new TagExtractor(type);
                    if (extractor.StartsAt(state.ChangeSegment(i, state.End)) &&
                        extractor.HasClosed(state.ChangeSegment(i + 2, state.End), out var closedIndex))
                    {
                        if (i - lastClosedTagIndex > 1)
                            yield return new Tag(TagType.None, lastClosedTagIndex + 1, i - 1);
                        yield return extractor.ExtractTag(state.ChangeSegment(i + 1, closedIndex.Value - 1));
                        lastClosedTagIndex = closedIndex.Value;
                        i = closedIndex.Value;
                    }
                }
            }
            if (state.End - lastClosedTagIndex > 0)
                yield return new Tag(TagType.None, lastClosedTagIndex + 1, state.End);
        }
    }

    [TestFixture]
    public class TagsExtractor_Should
    {
        [Test]
        public void OnlyNoneTag()
        {
            Extractor.GetAllTags(new State(new[] {new Lexeme(LexemeType.Text, "txt")}, 0, 0))
                .Should().BeEquivalentTo(new Tag(TagType.None, 0, 0));
        }

        [Test]
        public void GetAllTags_OnlyItalic()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "some text"),
                new Lexeme(LexemeType.Underlining, "_")
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.Italic, 1, lexemes.Length - 2));
        }

        [Test]
        public void GetAllTags_WithOverLengthUnderlinings()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Text, "kekopen"),
                new Lexeme(LexemeType.Underlining, "____"),
                new Lexeme(LexemeType.Text, "kekclosed")
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.None, 0, lexemes.Length - 1));
        }

        [Test]
        public void GetAllTags_ItalicWorkInBold()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "k"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "t"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "k"),
                new Lexeme(LexemeType.Underlining, "__"),
            };
            var state = new State(lexemes, 1, lexemes.Length - 2, new BitArray(new[] {false, true, false, false}));
            Extractor.GetAllTags(state)
                .Should().BeEquivalentTo(new Tag(TagType.None, 1, 1),
                    new Tag(TagType.Italic, 3, 3),
                    new Tag(TagType.None, 5, 5));
        }

        [Test]
        public void GetAllTags_BoldDoesNotWorkInItalic()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "k"),
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "t"),
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "k"),
                new Lexeme(LexemeType.Underlining, "_"),
            };
            var state = new State(lexemes, 1, lexemes.Length - 2, new BitArray(new[] {true, false, false, false}));
            Extractor.GetAllTags(state)
                .Should().BeEquivalentTo(new Tag(TagType.None, 1, lexemes.Length - 2));
        }

        [TestCase(' ', TestName = "When have space after")]
        [TestCase('1', TestName = "When have digit after")]
        public void OpenedTags_MustNotHaveForbidenCharAfter(char forbidenChar)
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, forbidenChar + "a"),
                new Lexeme(LexemeType.Underlining, "_")
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.None, 0, lexemes.Length - 1));
        }

        [TestCase(' ', TestName = "When have space before")]
        [TestCase('1', TestName = "When have digit before")]
        public void ClosedTags_MustNotHaveForbidenCharBefore(char forbidenChar)
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a" + forbidenChar),
                new Lexeme(LexemeType.Underlining, "_")
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.None, 0, lexemes.Length - 1));
        }

        [Test]
        public void TripleUnderlining_IsBoldItalicTag()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "___"),
                new Lexeme(LexemeType.Text, "text"),
                new Lexeme(LexemeType.Underlining, "___")
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.BoldItalic, 1, lexemes.Length - 2));
        }

        [Test]
        public void PassEmbeededTags()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "__"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "a"),
                new Lexeme(LexemeType.Underlining, "__")
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.Bold, 1, lexemes.Length - 2));
        }

        [Test]
        public void SeveralTags()
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "text"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "text"),
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, "text"),
                new Lexeme(LexemeType.Underlining, "_"),
            };
            Extractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.Italic, 1, 1),
                    new Tag(TagType.None, 3, 3),
                    new Tag(TagType.Italic, 5, 5));
        }
    }
}