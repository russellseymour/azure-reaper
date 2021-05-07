using System;
using System.Text;

namespace Azure.Reaper.Lib.Extensions
{
    public static class StringBuilderExtensions
    {
        public static void AppendDelim(this StringBuilder sb, string text, string delim = "", bool writeEmptyString = true)
        {
            if (writeEmptyString || !String.IsNullOrWhiteSpace(text))
            {
                if (sb.Length != 0)
                {
                    sb.Append(delim + text);
                }
                else
                {
                    sb.Append(text);
                }
            }
        }
    }
}