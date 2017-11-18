namespace Markdown
{
    public class Tag
    {
        public TagType Type { get; }
        public int Start { get; }
        public int End { get; }

        public Tag(TagType type, int start, int end)
        {
            Type = type;
            Start = start;
            End = end;
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
            var tag = (Tag)obj;
            return Type.Equals(tag.Type) && Start.Equals(tag.Start) && End.Equals(tag.End);
        }
    }
}