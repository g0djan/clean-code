using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public static class LexemeExtractor
    {
        private static bool StartsAt(int index, string text)
        {
            return index == 0 || 
                GetLexemeType(text[index - 1], IsEscaped(index - 1, text)) != GetLexemeType(text[index], IsEscaped(index, text));
        }

        private static bool IsEscaped(int index, string text)
        {
            if (index == 0)
                return false;
            return text[index - 1] == '\\';
        }

        private static LexemeType GetLexemeType(char c, bool isEscaped)
        {
            if (!isEscaped && c == '_')
                return LexemeType.Underlining;
            return LexemeType.Text;
        }

        private static Lexeme ExtractLexeme(string text, int start, int end) => 
            new Lexeme(GetLexemeType(text[start], false), 
                text.Substring(start, end - start + 1).Replace("\\_", "_"));

        public static Lexeme[] GetAllLexemes(string text)
        {
            var last = 0;
            return Enumerable.Range(1, text.Length - 1)
                .Where(index => StartsAt(index, text))
                .Select(index =>
                {
                    var prev = last;
                    last = index;
                    return ExtractLexeme(text, prev, last - 1);
                })
                .ToArray()
                .Concat(new []{ExtractLexeme(text, last, text.Length - 1)})
                .ToArray();
        }
    }

    [TestFixture]
    public class LexemeExtractor_Should
    {
        [TestCase("4OP[]!  1@$$%b23^", LexemeType.Text, TestName = "Text")]
        [TestCase("_", LexemeType.Underlining, TestName = "Underlining")]
        public void GetAllLexems_OnlyOneTagType(string markdown, LexemeType type)
        {
            LexemeExtractor.GetAllLexemes(markdown).Should()
                .BeEquivalentTo(new Lexeme(type, markdown));
        }

        [Test]
        public void TextWithEscaped()
        {
            LexemeExtractor.GetAllLexemes("a\\___bcd").Should()
                .BeEquivalentTo(new Lexeme(LexemeType.Text, "a_"),
                    new Lexeme(LexemeType.Underlining, "__"),
                    new Lexeme(LexemeType.Text, "bcd"));
        }
    }
}