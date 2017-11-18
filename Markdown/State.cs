using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Markdown
{
    public class State
    {
        private Lexeme[] Lexemes { get; }
        public int Start { get; }
        public int End { get; }
        private BitArray Mask { get; }

        public State(Lexeme[] lexemes, int start, int end, BitArray mask)
        {
            if (start > end)
                throw new ArgumentException();
            Lexemes = lexemes;
            Start = start;
            End = end;
            Mask = mask;
        }
        public State(Lexeme[] lexemes, int start, int end)
        {
            if (start > end)
                throw new ArgumentException();
            Lexemes = lexemes;
            Start = start;
            End = end;
            Mask = new BitArray(4);
        }

        public Lexeme GetLexeme(int index) => Lexemes[index];

        public bool IsInTag(TagType type)
        {
            return Mask[(int) type];
        }

        public State SwitchTag(TagType type)
        {
            var newMask = new BitArray(Mask){ [(int) type] = !Mask[(int) type]};
            return new State(Lexemes, Start, End, newMask);
        }

        public State OnSegment(int start, int end) => new State(Lexemes, start, end, new BitArray(Mask));
    }

    [TestFixture]
    public class State_Should
    {
        [Test]
        public void StartCanNotToBeGreaterThanEnd()
        {
            Assert.Throws<ArgumentException>(() => new State(new Lexeme[1],1, 0, new BitArray(1)));
        }
        
        [Test]
        public void TestIsInTag()
        {
            var state = new State(new Lexeme[1], 0, 0, new BitArray(new[] { false, true , false, false}));
            (state.IsInTag(TagType.Bold) && !state.IsInTag(TagType.Italic)).Should().BeTrue();
        }

        [Test]
        public void SwitchTag_ChangeBoldItalic()
        {
            var state = new State(new Lexeme[1], 0, 0);
            state.SwitchTag(TagType.BoldITalic).IsInTag(TagType.BoldITalic).Should().BeTrue();
        }
    }
}