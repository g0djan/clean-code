using System;

namespace Markdown
{
    public class Lexeme
    {
        public LexemeType Type { get; }
        public string Content { get; }

        public Lexeme(LexemeType type, string content)
        {
            Content = content;
            Type = type;
        }

        public Lexeme Concat(Lexeme lexeme)
        {
            throw new NotImplementedException();
        }
    }
}