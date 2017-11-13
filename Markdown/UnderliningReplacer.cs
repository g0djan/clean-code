using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using static Markdown.Tag;

namespace Markdown
{
    public class UnderliningReplacer
    {
        public string ReplaceTags(string text, List<Tag> tags)
        {
            var builder = new StringBuilder();
            var posToTagNames = GetTagsCoords(tags);
            var lastIndex = 0;
            posToTagNames
                .ToList()
                .ForEach(pair =>
                {
                    AddNextPart(builder, pair, lastIndex, text);
                    lastIndex = GetLastIndex(pair, lastIndex);
                });
            builder.Append(text.Substring(lastIndex, text.Length - lastIndex));
            return builder.ToString();
        }

        private void AddNextPart(StringBuilder builder, KeyValuePair<int, string> posAndTag, int index, string text)
        {
            var length = posAndTag.Key - index;
            builder.Append(text.Substring(index, length));
            builder.Append(posAndTag.Value);
        }

        private int GetLastIndex(KeyValuePair<int, string> posAndTag, int lastIndex)
        {
            return posAndTag.Key + GetMd(GetTagName(posAndTag.Value)).Length;
        }

        private IEnumerable<KeyValuePair<int, string>> GetTagsCoords(List<Tag> tags)
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
        }

        [Test]
        public void ReplaceTags_SimpleEmInText()
        {
            replacer.ReplaceTags("_simple_", new List<Tag>{new Tag("_", 0, 7)}).Should().Be("<em>simple</em>");
        }

        [Test]
        public void ReplaceTags_SimpleStrongText()
        {
            replacer
                .ReplaceTags("__simple__", new List<Tag> {new Tag("__", 0, 8)})
                .Should()
                .Be("<strong>simple</strong>");
        }

        [Test]
        public void ReplaceTags_SeveralTags()
        {
            replacer
                .ReplaceTags("_t1_t2__t3__", new List<Tag>
                {
                    new Tag("_", 0, 3),
                    new Tag("__", 6, 10)
                })
                .Should()
                .Be("<em>t1</em>t2<strong>t3</strong>");
        }

        [Test]
        public void ReplaceTags_WithInactiveUnderlining()
        {
            replacer
                .ReplaceTags("_t1__t2_", new List<Tag>
                {
                    new Tag("_", 0, 7),
                })
                .Should()
                .Be("<em>t1__t2</em>");
        }
    }
}