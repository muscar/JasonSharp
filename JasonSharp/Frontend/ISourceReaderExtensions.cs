using System;
using System.IO;
using System.Text;

namespace JasonSharp.Frontend
{
    public static class ISourceReaderExtensions
    {
        public static string ReadWhile(this ISourceReader r, Predicate<char> pred)
        {
            var sb = new StringBuilder();
            var c = r.Peek();
            while (c != -1 && pred((char) c))
            {
                sb.Append((char) r.Read());
                c = r.Peek();
            }
            return sb.ToString();
        }
    }
}

