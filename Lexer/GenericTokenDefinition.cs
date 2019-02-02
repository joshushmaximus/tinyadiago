using System;
using TokenParsing;

namespace TokenParsing
{

    public class GenericTokenDefinition<T> where T : IComparable, IConvertible, IFormattable
    {
        public readonly IMatcher Matcher;
        public readonly GenericToken<T> FullToken;
        public readonly T Type;
        public readonly string Expr = string.Empty;

        public T GetToken<T>() where T : IComparable, IConvertible, IFormattable { return (T)Enum.ToObject(typeof(T), FullToken.int_type); }

        public readonly bool isTreatedAsLineTerminator = false;
        public readonly bool isIgnoreCase = true;

        public bool isValid { get { return !string.IsNullOrWhiteSpace(Expr) && (Matcher != null) && (FullToken != null); } }

        
        public GenericTokenDefinition(string regex, GenericToken<T> token, bool isignorecase = true, bool iseol = false)
        {
            isTreatedAsLineTerminator = iseol;
            isIgnoreCase = isignorecase;
            Expr = regex;
            this.Matcher = new TokenParsing.RegexMatcher(regex, isignorecase);
            this.Type = token.type;
            this.FullToken = token;
        }

        public override string ToString()
        {
            return isValid ? Expr + " => " + FullToken.type + (isTreatedAsLineTerminator ? " [EOL]" : string.Empty) : "<invalid tokdef>";
        }
    }


}
