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
        private Lexeme[] lexemes;
        public int start { get; }
        public int end { get; }
        private BitArray mask;

        public State(Lexeme[] lexemes, int start, int end, BitArray mask)
        {
            if (start > end)
                throw new ArgumentException();
            this.lexemes = lexemes;
            this.start = start;
            this.end = end;
            this.mask = mask;
        }
        public State(Lexeme[] lexemes, int start, int end)
        {
            if (start > end)
                throw new ArgumentException();
            this.lexemes = lexemes;
            this.start = start;
            this.end = end;
            mask = new BitArray(4);
        }

        public Lexeme GetLexeme(int index) => lexemes[index];

        public bool IsInTag(TagType type)
        {
            return mask[(int) type];
        }

        public State SwitchTag(TagType type)
        {
            var newMask = mask;
            newMask[(int) type] = !mask[(int) type];
            return new State(lexemes, start, end, newMask);
        }

        public State OnSegment(int start, int end) => new State(lexemes, start, end, new BitArray(mask));
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
    }
}