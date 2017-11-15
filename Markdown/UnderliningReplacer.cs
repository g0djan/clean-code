using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using static Markdown.TagSegment;

namespace Markdown
{
    public class UnderliningReplacer
    {
        public string ReplaceTags(string text, List<TagSegment> tags)
        {
            var builder = new StringBuilder();
            var posToTagNames = GetTagsCoords(tags);
            var lastIndex = 0;
            posToTagNames
                .ToList()
                .ForEach(pair =>
                {
                    builder.AppendAll(GetNextPart(pair, lastIndex, text));
                    lastIndex = GetLastIndex(pair, lastIndex);
                });
            builder.Append(text.Substring(lastIndex, text.Length - lastIndex));
            return builder.ToString();
        }

        private IEnumerable<string> GetNextPart(KeyValuePair<int, string> posAndTag, int index, string text)
        {
            var length = posAndTag.Key - index;
            yield return text.Substring(index, length);
            yield return posAndTag.Value;
        }

        private int GetLastIndex(KeyValuePair<int, string> posAndTag, int lastIndex)
        {
            return posAndTag.Key + Tags.GetMd(Tags.GetTagName(posAndTag.Value)).Length;
        }

        private IEnumerable<KeyValuePair<int, string>> GetTagsCoords(List<TagSegment> tags)
        {
            return tags
                .Select(tag => tag.To2HtmlTags())
                .SelectMany(twoTags => twoTags)
                .OrderBy(kvpair => kvpair.Key);
        }
    }

    [TestFixture]
    public class UnderliningReplacer_Should
    {
        private UnderliningReplacer replacer;

        [SetUp]
        public void SetUp()
        {
            replacer = new UnderliningReplacer();
            Tags.InitNewTag(TagName.Em, "_", "<em>", "</em>");
            Tags.InitNewTag(TagName.Strong, "__", "<strong>", "</strong>");
            Tags.InitNewTag(TagName.StrongEm, "___", "<strong><em>", "</em></strong>");
        }

        [Test]
        public void ReplaceTags_SimpleEmInText()
        {
            replacer.ReplaceTags("_simple_", new List<TagSegment>{new TagSegment("_", 0, 7)}).Should().Be("<em>simple</em>");
        }

        [Test]
        public void ReplaceTags_SimpleStrongText()
        {
            replacer
                .ReplaceTags("__simple__", new List<TagSegment> {new TagSegment("__", 0, 8)})
                .Should()
                .Be("<strong>simple</strong>");
        }

        [Test]
        public void ReplaceTags_SeveralTags()
        {
            replacer
                .ReplaceTags("_t1_t2__t3__", new List<TagSegment>
                {
                    new TagSegment("_", 0, 3),
                    new TagSegment("__", 6, 10)
                })
                .Should()
                .Be("<em>t1</em>t2<strong>t3</strong>");
        }

        [Test]
        public void ReplaceTags_WithInactiveUnderlining()
        {
            replacer
                .ReplaceTags("_t1__t2_", new List<TagSegment>
                {
                    new TagSegment("_", 0, 7),
                })
                .Should()
                .Be("<em>t1__t2</em>");
        }
    }
}