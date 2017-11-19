using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    static class UnderliningTagChecker
    {
        private const int MinTagsCountBtwOpenAndCloseTags = 2;

        public static bool IsThereStartsOpenedTag(State state) => 
            state.End - state.Start >= MinTagsCountBtwOpenAndCloseTags && 
            !(HasSpacesAfterOpenedTag(state) || HasDigitAfterOpenedTag(state));

        public static bool IsThereStartsClosedTag(State state) =>
            !(HasSpacesBeforeClosedTag(state) || HasDigitBeforeClosedTag(state));

        private static bool HasSpacesAfterOpenedTag(State state) => 
            state.Start < state.End && state.GetLexeme(state.Start + 1).Content[0] == ' ';

        private static bool HasDigitAfterOpenedTag(State state) =>
            state.Start < state.End && char.IsDigit(state.GetLexeme(state.Start + 1).Content[0]);

        private static bool HasSpacesBeforeClosedTag(State state) => 
            state.Start > 0 && state.GetLexeme(state.Start - 1).Content.Last() == ' ';

        private static bool HasDigitBeforeClosedTag(State state) => 
            state.Start > 0 && char.IsDigit(state.GetLexeme(state.Start - 1).Content.Last());
    }

    [TestFixture]
    public class UnderliningTagChecker_Should
    {
        [TestCase(' ', TestName = "Space")]
        [TestCase('1', TestName = "Digit")]
        public void NoOpenTag_BeforeSymbol(char symbol)
        {
            var lexemes = new []
            {
                new Lexeme(LexemeType.Underlining, "_"),
                new Lexeme(LexemeType.Text, symbol + "text")
            };
            UnderliningTagChecker
                .IsThereStartsOpenedTag(new State(lexemes, 0, lexemes.Length - 1))
                .Should().BeFalse();
        }

        [TestCase(' ', TestName = "Space")]
        [TestCase('1', TestName = "Digit")]
        public void NoClosedTag_AfterSymbol(char symbol)
        {
            var lexemes = new[]
            {
                new Lexeme(LexemeType.Text, "text" + symbol),
                new Lexeme(LexemeType.Underlining, "_")
            };
            UnderliningTagChecker
                .IsThereStartsClosedTag(new State(lexemes, 1, lexemes.Length - 1))
                .Should().BeFalse();
        }

        [Test]
        public void SoClose_StartAndEndInState()
        {
            UnderliningTagChecker
                .IsThereStartsOpenedTag(new State(new Lexeme[2], 0, 1))
                .Should().BeFalse();
        }
    }
}
