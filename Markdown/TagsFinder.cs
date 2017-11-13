using System;
using System.Collections;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public class TagsFinder
    {
        public TagsFinder()
        {
        }

        public Tag GetFirstTagOnSegment(TextTagsState state)
        {
            int? openedIndex, closedIndex;
            TagName? tagName;
            while (true)
            {
                openedIndex = GetIndexTagBracket(state, IsOpenedTag, null, false);
                if (!openedIndex.HasValue)
                    return null;
                state = state.ChangeSegment(openedIndex.Value, state.End);
                tagName = GetTagName(state, IsOpenedTag);
                var indexInTag = openedIndex.Value + Tag.GetMd(tagName.Value).Length;
                if (indexInTag > state.End)
                    return null;
                state = state.ChangeSegment(indexInTag, state.End);
                closedIndex = GetIndexTagBracket(state, IsClosedTag, tagName, true);
                if (!closedIndex.HasValue || IsEmptyInsideTag(closedIndex, openedIndex, tagName))
                    continue;
                break;
            }
            return new Tag(tagName.Value, openedIndex.Value, closedIndex.Value);
        }

        private static bool IsEmptyInsideTag(int? closedIndex, int? openedIndex, TagName? tagName)
        {
            return closedIndex.Value - openedIndex.Value < Tag.GetMd(tagName.Value).Length + 1;
        }

        private TagName? GetTagName(TextTagsState state, Func<TagName, TextTagsState, bool> condition)
        {
            return Enum
                .GetValues(typeof(TagName))
                .Cast<TagName?>()
                .FirstOrDefault(tagName =>
                    condition(tagName.Value, state));
        }

        private int? GetIndexTagBracket(
            TextTagsState state,
            Func<TagName, TextTagsState, bool> condition,
            TagName? expectedTagName,
            bool isEqual)
        {
            return Enumerable
                .Range(state.Start, state.End - state.Start + 1)
                .Cast<int?>()
                .FirstOrDefault(i =>
                    Equals(GetTagName(state.ChangeSegment(i.Value, state.End), condition), expectedTagName) == isEqual);
        }

        private bool IsOpenedTag(TagName tag, TextTagsState state)
        {
            var indexInTag = state.Start + Tag.GetMd(tag).Length;
            return IsItLooksLikeTag(tag, state) &&
                indexInTag - 1 < state.End && 
                state.Text[indexInTag] != ' ' &&
                IsNotPrevSymbolUnderlining(state) &&
                !char.IsDigit(state.Text[indexInTag]);
        }

        private bool IsClosedTag(TagName tag, TextTagsState state)
        {
            return IsItLooksLikeTag(tag, state) &&
                state.Start > 0 &&
                state.Text[state.Start - 1] != ' ' &&
                IsNotPrevSymbolUnderlining(state) &&
                !char.IsDigit(state.Text[state.Start - 1]);
        }

        private bool IsNotPrevSymbolUnderlining(TextTagsState state)
        {
            return state.Start == 0 || 
                    state.Start > 0 &&
                   (state.Text[state.Start - 1] != '_' ||
                    state.Start > 1 &&
                    state.Text[state.Start - 2] == '\\');
        }

        private bool IsItLooksLikeTag(TagName tag, TextTagsState state)
        {
            if (state.Start + Tag.GetMd(tag).Length - 1 > state.End || IsDisabledEscape(state))
                return false;
            if (state.Start + 4 <= state.End && state.Text.Substring(state.Start, 4) == "____")
                return false;
            var potentialTag = state.Text.Substring(state.Start, Tag.GetMd(tag).Length);
            return Tag.GetMd(tag) == potentialTag && 
                !state.IsInTag(TagName.Em) && 
                !state.IsInTag(TagName.StrongEm);
        }

        private bool IsDisabledEscape(TextTagsState state) =>
            state.Start > 0 && state.Text[state.Start - 1] == '\\';
    }

    [TestFixture]
    public class TagsFinder_Should
    {
        private TagsFinder tagsFinder;

        [SetUp]
        public void SetUp()
        {
            tagsFinder = new TagsFinder();
        }

        private void TestGetFirstTagOnSegment(string text, Tag expectedTag)
        {
            var state = new TextTagsState(text, 0, text.Length - 1);
            tagsFinder.GetFirstTagOnSegment(state).Should().Be(expectedTag);
        }

        [TestCase(TagName.Em, 0, 2)]
        [TestCase(TagName.Strong, 0, 3)]
        public void FindFirstTag_InBeginingAndOutOfTags(TagName expectedTagName, int expectedX, int expectedY)
        {
            var md = Tag.GetMd(expectedTagName);
            var text = string.Format("{0}t{0}t", md);
            var state = new TextTagsState(text, 0, text.Length - 1);
            tagsFinder.GetFirstTagOnSegment(state).Should().Be(new Tag(expectedTagName, expectedX, expectedY));
        }

        [Test]
        public void FindFirstTag_DisabledEscapeSymbols_AreNotTags() =>
            TestGetFirstTagOnSegment("\\_isnottag\\_", null);

        [Test]
        public void DisabledEscapeSymbolsInTag() => 
            TestGetFirstTagOnSegment("_\\_isnottag\\__", new Tag(TagName.Em, 0, "_\\_isnottag\\__".Length - 1));

        [Test]
        public void EmInStrong_Works()
        {
            var text = "__t_t_t__";
            var state = new TextTagsState(text, 2, text.Length - 2);
            tagsFinder.GetFirstTagOnSegment(state).Should().Be(new Tag(TagName.Em, 3, 5));
        }

        [Test]
        public void StrongAroundEm_Works() => 
            TestGetFirstTagOnSegment("__t_t_t__", new Tag(TagName.Strong, 0, "__t_t_t__".Length - 2));

        [Test]
        public void StrongInEm_DoesNotWork()
        {
            var text = "_t__t__t_";
            var state = new TextTagsState(new BitArray(new []{false, false, true}), text, 1, text.Length - 2);
            tagsFinder.GetFirstTagOnSegment(state).Should().Be(null);
        }

        [Test]
        public void BetweenOpenAndClosedTags_Should_BeNonEmptyText() => 
            TestGetFirstTagOnSegment("__", null);

        [TestCase("t_t", ExpectedResult = null)]
        [TestCase("t__t", ExpectedResult = null)]
        public Tag TagMustClosed(string text)
        {
            var state = new TextTagsState(text, 0, text.Length - 1);
            return tagsFinder.GetFirstTagOnSegment(state);
        }

        [Test]
        public void OpenTag_MustNotHaveSpacesAfter() => 
            TestGetFirstTagOnSegment("t_ t_", null);

        [Test]
        public void CloseTag_MustNotHaveSpacesBefore() => 
            TestGetFirstTagOnSegment("t_t _", null);

        [Test]
        public void UnderliningsTouchesDigitAreNotTags() => 
            TestGetFirstTagOnSegment("digits_12_3", null);

        [Test]
        public void TripleUnderliningIsStrongEmTag() => 
            TestGetFirstTagOnSegment("t___t t___t", new Tag(TagName.StrongEm, 1, 7));

        [Test]
        public void SeveralOpenedTags() => 
            TestGetFirstTagOnSegment("t__t_t_", new Tag(TagName.Em, 4, 6));

        [Test]
        public void EmIsNotSubTagOfStrong() => 
            TestGetFirstTagOnSegment("t__t_", null);

        [Test]
        public void ThreeDifferentUnderlining_IsNotTags() => 
            TestGetFirstTagOnSegment("_a___a__", null);

        [Test]
        public void MoreThan3Underlinings_IsNotTags() => 
            TestGetFirstTagOnSegment("____a____", null);

        [Test]
        public void InactiveEmInStrongEm()
        {
            var text = "___a_a_a___";
            var state = new TextTagsState(new BitArray(new[]{true, false, false}), text, 3, text.Length - 4);
            tagsFinder.GetFirstTagOnSegment(state).Should().BeNull();
        }
    }
}