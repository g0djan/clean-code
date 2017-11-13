using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Primitives;
using NUnit.Framework;

namespace Markdown
{
    public class TextTagsState
    {
        private readonly BitArray mask;
        public readonly string Text;

        public readonly int Start;
        public readonly int End;
        private readonly Dictionary<TagName, int> tagsNumbers = new Dictionary<TagName, int>
        {
            [TagName.Em] = 2,
            [TagName.Strong] = 1,
            [TagName.StrongEm] = 0
        };

        public TextTagsState(string text, int start, int end)
        {
            Text = text;
            Start = start;
            End = end;
            mask = new BitArray(3);
        }

        public TextTagsState(BitArray mask, string text, int start, int end)
        {
            this.mask = mask;
            Text = text;
            Start = start;
            End = end;
        }

        public TextTagsState SwitchTag(TagName name)
        {
            var bitArray = new BitArray(mask);
            bitArray[tagsNumbers[name]] = !mask[tagsNumbers[name]];
            return new TextTagsState(bitArray, Text, Start, End);
        }

        public bool IsInTag(TagName name) => mask[tagsNumbers[name]];

        public TextTagsState ChangeSegment(int start, int end)
        {
            if (start < 0 || end < 0 || start >= Text.Length || end >= Text.Length || start > end)
                throw new ArgumentException();
            return new TextTagsState(mask, Text, start, end);
        }
    }

    [TestFixture]
    public class TextTagsState_Should
    {
        private TextTagsState textTagsState;

        [SetUp]
        public void SetUp()
        {
            textTagsState = new TextTagsState(new BitArray(new[] { false, true, false }), "", 0, 0);
        }


        [TestCase(TagName.Strong, TestName = "TagName.Strong true in bitmask '10'",ExpectedResult = true)]
        [TestCase(TagName.Em, TestName = "TagName.Em false in bitmask '10'", ExpectedResult = false)]
        public bool TestIsInTag(TagName tagName) => textTagsState.IsInTag(tagName);

        [Test]
        public void SwitchTag_InverseBitInMask()
        {
            var expectedState = !textTagsState.IsInTag(TagName.Em);

            textTagsState = textTagsState.SwitchTag(TagName.Em);

            textTagsState.IsInTag(TagName.Em).Should().Be(expectedState);
        }
    }
}