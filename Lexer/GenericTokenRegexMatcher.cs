using System.Text.RegularExpressions;

namespace TokenParsing
{
    public sealed class RegexMatcher : IMatcher
    {
        private readonly Regex regex;
        public RegexMatcher(string regex, bool isignorecase = true)
        {
            var opts = RegexOptions.CultureInvariant;
            if (isignorecase)
                opts |= RegexOptions.IgnoreCase;
            this.regex = new Regex(string.Format("^{0}", regex), opts );
        }

        public int Match(string text)
        {
            var m = regex.Match(text);
            return m.Success ? m.Length : 0;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }

    public interface IMatcher
    {
        /// <summary>
        /// Return the number of characters that this "regex" or equivalent
        /// matches.
        /// </summary>
        /// <param name="text">The text to be matched</param>
        /// <returns>The number of characters that matched</returns>
        int Match(string text);
    }
}
