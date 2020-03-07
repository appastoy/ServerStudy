using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Neti.CodeGenerator
{
    public static class StringExtensions
    {
        static readonly Regex emptyLineRegex = new Regex(@"^[\t ]+\r?\n", RegexOptions.Compiled | RegexOptions.Multiline);

        public static string Join(this IEnumerable<string> strings)
        {
            return string.Join(string.Empty, strings);
        }

        public static string Join(this IEnumerable<string> strings, string seperator)
        {
            return string.Join(seperator, strings);
        }

        public static string JoinWithLine(this IEnumerable<string> strings)
        {
            return string.Join(Environment.NewLine, strings);
        }

        public static string InsertAfterEachLine(this string text, string insertText)
        {
            return text.Replace(Environment.NewLine, $"{Environment.NewLine}{insertText}");
        }

        public static string TrimEmptyLine(this string text)
        {
            return emptyLineRegex.Replace(text, Environment.NewLine);
        }
    }
}
