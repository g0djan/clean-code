namespace Markdown
{
    public interface ITagExtractor
    {
        bool StartsAt(State state);
        bool HasClosed(State state, out int index);
        Tag ExtractTag(State state);
    }
}