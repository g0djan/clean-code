using System.Collections.Generic;
using System.Text;

namespace Markdown
{
    public static class StringBuilderExtensions
    {
        public static void AppendAll(this StringBuilder builder, IEnumerable<string> lines)
        {
            foreach (var line in lines)
                builder.Append(line);
        }
    }
}