namespace Markdown
{
    public class Tag
    {
        public TagType Type { get; }
        public string Content { get; }
        public bool IsOpened { get; }           

        public Tag(TagType type, string content, bool isOpened)
        {
            Type = type;
            Content = content;
            IsOpened = isOpened;
        }
    }
}