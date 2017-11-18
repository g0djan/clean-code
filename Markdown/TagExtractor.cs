using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public class TagExtractor
    {
        private static bool StartsAt(State state, out TagType tagType)
        {
            if (IsThereNoTag(state))
            {
                tagType = TagType.None;
                return false;
            }
            tagType = (TagType) (state.GetLexeme(state.start).Content.Length - 1);
            return true;
        }

        private static bool IsThereNoTag(State state)
        {
            const int maxUnderliningLength = 3;
            const int minDistanceBtwOpenClose = 2;
            var lexeme = state.GetLexeme(state.start);
            return lexeme.Content.Length > maxUnderliningLength ||
                   lexeme.Type == LexemeType.Text ||
                   state.IsInTag(TagType.Italic) ||
                   state.IsInTag(TagType.BoldITalic) ||
                   state.end - state.start < minDistanceBtwOpenClose ||
                   HasSpacesAfterOpenedTag(state) ||
                   HasDigitAfterOpenedTag(state);
        }

        private static bool HasSpacesAfterOpenedTag(State state) =>
            state.GetLexeme(state.start + 1).Content[0] == ' ';

        private static bool HasSpacesBeforeClosedTag(Lexeme lexeme) => lexeme.Content.Last() == ' ';

        private static bool HasDigitAfterOpenedTag(State state) =>
            char.IsDigit(state.GetLexeme(state.start + 1).Content[0]);

        private static bool HasDigitBeforeClosedTag(Lexeme lexeme) => char.IsDigit(lexeme.Content.Last());

        private static bool HasClosedTag(State state, TagType openedType, out int closedIndex)
        {
            for (var i = state.start; i <= state.end; i++)
                if (state.GetLexeme(i).Type == LexemeType.Underlining &&
                    (TagType) (state.GetLexeme(i).Content.Length - 1) == openedType &&
                    !HasSpacesBeforeClosedTag(state.GetLexeme(i - 1)) &&
                    !HasDigitBeforeClosedTag(state.GetLexeme(i - 1)))
                {
                    closedIndex = i;
                    return true;
                }
            closedIndex = -1;
            return false;
        }

        private static Tag ExtractTag(State state, TagType tagType)
        {
            return new Tag(tagType, state.start + 1, state.end - 1);
        }

        public static IEnumerable<Tag> GetAllTags(State state)
        {
            var lastClosedTagIndex = state.start - 1;
            for (var i = state.start; i <= state.end; i++)
                if (StartsAt(state.OnSegment(i, state.end), out var tagType) &&
                    HasClosedTag(state.OnSegment(i + 2, state.end), tagType, out var closedIndex))
                {
                    if (i - lastClosedTagIndex > 1)
                        yield return new Tag(TagType.None, lastClosedTagIndex + 1, i - 1);
                    yield return ExtractTag(state.OnSegment(i, closedIndex), tagType);
                    lastClosedTagIndex = closedIndex;
                    i = closedIndex;
                }
            if (state.end - lastClosedTagIndex > 0)
                yield return new Tag(TagType.None, lastClosedTagIndex + 1, state.end);
        }
    }

    [TestFixture]
    public class TagsExtractor_Should
    {
        [Test]
        public void OnlyNoneTag()
        {
            TagExtractor.GetAllTags(new State(new[] {new Lexeme(LexemeType.Text, "txt")}, 0, 0))
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
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
            TagExtractor.GetAllTags(state)
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
            TagExtractor.GetAllTags(state)
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.BoldITalic, 1, lexemes.Length - 2));
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
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
            TagExtractor.GetAllTags(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeEquivalentTo(new Tag(TagType.Italic, 1, 1),
                    new Tag(TagType.None, 3, 3),
                    new Tag(TagType.Italic, 5, 5));
        }
    }
}