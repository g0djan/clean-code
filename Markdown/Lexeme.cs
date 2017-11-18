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

        public override bool Equals(object obj)
        {
            // STEP 1: Check for null
            if (obj == null)
            {
                return false;
            }

            // STEP 3: equivalent data types
            if (GetType() != obj.GetType())
            {
                return false;
            }
            var lexeme = (Lexeme) obj;
            return Type.Equals(lexeme.Type) && Content.Equals(lexeme.Content);
        }

    }
}