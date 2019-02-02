using System;

namespace TokenParsing
{
    public delegate TokenI GetTokenIDel();
    public delegate GenericToken<T> GetGenericTokenDel<T>() where T : IComparable, IConvertible, IFormattable;

    public interface TokenI
    {
        int id { get; set; }
        int line { get; set; }
        int pos { get; set; }
        string data { get; set; }
        string source { get; set; }
        int int_type { get; }
        bool isToken(int cmp);
        bool isEOLToken { get; set; }
        
    }

    public class GenericToken<T> :TokenI where T : IComparable, IConvertible, IFormattable
    {
        public int id { get; set; }
        public int line { get; set; }
        public int pos { get; set; }
        public bool isOriginalToken { get { return (line >= 0) && (pos >= 0); } }
        public bool isEmpty {  get { return string.IsNullOrWhiteSpace(data);  } }
        public bool isValid { get { return !isEmpty && (int_type != 0); } }
        public string data { get; set; }
        public T type;
        public string source { get; set; }
        bool _iseol = false;
        public bool isEOLToken { get { return _iseol; }  set { _iseol = value; } }
        public GenericTokenDefinition<T> DefMatched = default(GenericTokenDefinition<T>);
        public bool isMatchValid {  get { return (DefMatched != null); } }

        public int int_type { get { return type.ToInt32(System.Globalization.CultureInfo.InvariantCulture); }  }

        /// <summary>
        /// returns true if token matches any of the comparison tokens
        /// </summary>
        /// <param name="cmps"></param>
        /// <returns></returns>
        public bool isToken(params T[] cmps) 
        {
            foreach (var cmp in cmps)
                if (type.CompareTo(cmp) == 0)
                    return true;
            return false;
        }
        public bool isToken(int cmp) { return int_type == cmp; }


        public GenericToken(GenericToken<T> t)
        {
            source = t.source;
            id = t.id;
            line = t.line;
            pos = t.pos;
            data = t.data;
            type = t.type;
            _iseol = t._iseol;
            DefMatched = t.DefMatched;
        }

        public GenericToken(T ttype)
        {
            source = string.Empty;
            id = -1;
            data = string.Empty;
            pos = -1;
            line = -1;
            type = ttype;
            _iseol = false;
        }

        public GenericToken(T ttype, string tok) : this(ttype, tok, false) { }
        public GenericToken(T ttype, string tok, bool iseol)
        {
            source = string.Empty;
            id = -1;
            data = tok;
            pos = -1;
            line = -1;
            type = ttype;
            _iseol = iseol;
        }


        public override string ToString()
        {
            bool hasdata = !string.IsNullOrWhiteSpace(data);
            return source + ":" + type.ToString() + (hasdata ? " = " + data : string.Empty) + " @ " + id + "/" + line;
        }


        
    }





}
