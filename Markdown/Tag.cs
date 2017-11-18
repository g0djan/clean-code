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
            if (obj == null)
            {
                return false;
            }
            
            if (GetType() != obj.GetType())
            {
                return false;
            }
            var tag = (Tag)obj;
            return Type.Equals(tag.Type) && Start.Equals(tag.Start) && End.Equals(tag.End);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Start;
                hashCode = (hashCode * 397) ^ End;
                return hashCode;
            }
        }
    }
}