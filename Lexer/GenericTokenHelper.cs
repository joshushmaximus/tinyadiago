using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyAdiago;

namespace TokenParsing
{
    public class GenericTokenHelper
    {
        #region stream factory = parse loop

        /// <summary>
        /// get a list of generic tokens from definitions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <param name="defs"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> GetStream<T>(string tag, string data, GenericTokenDefinition<T>[] defs,
    DebugDelegate d) where T : IComparable, IConvertible, IFormattable

        { return GetStream<T>(tag, data, defs, d, null); }

        public static List<GenericToken<T>> GetStream<T>(string tag, string data, GenericTokenDefinition<T>[] defs,
            DebugDelegate d, Prac.API.Int32Delegate progress)
    where T : IComparable, IConvertible, IFormattable
        {
            List<GenericToken<T>> toks = new List<GenericToken<T>>();

            GenericLexer<T> lex = new GenericLexer<T>(tag, data, defs);
            int size = lex.BytesLeft;
            int lastprog = 0;

            while (lex.Next())
            {
                var tok = new GenericToken<T>(lex.Token);
                tok.id = toks.Count;
                toks.Add(tok);
                // compute progress
                if (progress != null)
                {
                    var completed = size - lex.BytesLeft;
                    var pct = (double)completed / size;
                    var pctint = (int)(pct * 100);
                    if (pctint > lastprog)
                    {
                        lastprog = pctint;
                        progress(pctint);
                    }
                }
            }

            return toks;

        }

        #endregion

        #region token defs

        /// <summary>
        /// get all the token types used in a list of token definitions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defs"></param>
        /// <returns></returns>
        public static List<T> Defs2Types<T>(GenericTokenDefinition<T>[] defs)
            where T : IComparable, IConvertible, IFormattable
        {
            List<T> list = new List<T>();
            foreach (var def in defs)
                if (def.isValid && !list.Contains(def.Type))
                    list.Add(def.Type);
            return list;
        }

        /*
        /// <summary>
        /// get a bunch of token definitions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defs"></param>
        /// <returns></returns>
        public static GenericTokenDefinition<T>[] getdefs<T>(params Tuple<string,T>[] defs)
            where T : IComparable, IConvertible, IFormattable
        {
            List<GenericTokenDefinition<T>> finaldefs = new List<GenericTokenDefinition<T>>();
            foreach (var def in defs)
                finaldefs.Add(GenericTokenHelper.getdef<T>(def.Item1, def.Item2));
            return finaldefs.ToArray();
        }
        */
    

        public static GenericTokenDefinition<T>[] getdefs<T>(params Tuple<string, T, bool>[] defs)
    where T : IComparable, IConvertible, IFormattable
        {
            List<GenericTokenDefinition<T>> finaldefs = new List<GenericTokenDefinition<T>>();
            foreach (var def in defs)
                finaldefs.Add(GenericTokenHelper.getdef<T>(def.Item1, def.Item2,def.Item3));
            return finaldefs.ToArray();
        }

        /// <summary>
        /// get a bunch of defs as an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tokdefs"></param>
        /// <returns></returns>
        public static GenericTokenDefinition<T>[] getdefs<T>(params GenericTokenDefinition<T>[] tokdefs)
            where T : IComparable, IConvertible, IFormattable
        {
            return tokdefs;
        }



        /// <summary>
        /// build a token definition focused on collecting specific word tokens (or simple multi word tokens).
        /// ie prefixes regex with: (^|\s) suffixes with: \s
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regex"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GenericTokenDefinition<T> getword<T>(string regex, T type)
    where T : IComparable, IConvertible, IFormattable
        {
            regex = word(regex);
            return getdef<T>(regex, type, false);
        }

        //public const string REGEXP_PUNC = @"\p{P}";
        public const string REGEXP_START = @"(^|\s)";
        public const string REGEXP_END = @"(\s|$)";

        /// <summary>
        /// build a regexp focused on collecting simple word tokens (or simple multi-word tokens)
        ///  ie prefixes regex with: (^|\s) suffixes with: \s
        /// </summary>
        /// <param name="wordregex"></param>
        /// <returns></returns>
        public static string word(string wordregex)
        {
            const string start = REGEXP_START;
            const string end = REGEXP_END;
            if (!wordregex.StartsWith(start))
                wordregex = start + wordregex;
            if (!wordregex.EndsWith(end))
                wordregex = wordregex + end;
            return wordregex;
        }

        /// <summary>
        /// build a getword token with a bunch of simple tokens or-d together
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="or_regexes"></param>
        /// <returns></returns>
        public static GenericTokenDefinition<T> getwords<T>(T type, params string[] or_regexes)
    where T : IComparable, IConvertible, IFormattable
        {
            var regexp = "(" + string.Join("|", or_regexes) + ")";
            return getword<T>(regexp, type);
        }

        /// <summary>
        /// get a token definition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regex"></param>
        /// <param name="type"></param>
        /// <param name="iseol"></param>
        /// <returns></returns>
        public static GenericTokenDefinition<T> getdef<T>(string regex, T type, bool isignorecase = true, bool iseol = false)
             where T : IComparable, IConvertible, IFormattable
        {
            return new GenericTokenDefinition<T>(regex, new GenericToken<T>(type),isignorecase,iseol);
        }

        #endregion

        #region token factories


        /// <summary>
        /// return invalid token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static GenericToken<T> GetInvalid<T>() where T : IComparable, IConvertible, IFormattable { return default(GenericToken<T>); }

        #endregion

        #region stream transposition

        /// <summary>
        /// copy a stream of tokens
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="org"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> CopyStream<T>(List<GenericToken<T>> org)
            where T : IComparable, IConvertible, IFormattable
        {
            var tmp = new GenericToken<T>[org.Count];
            Array.Copy(org.ToArray(), tmp, org.Count);
            var copy = new List<GenericToken<T>>(tmp);
            return copy;

        }

        /// <summary>
        /// gets a quick list of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<T> GetList<T>(params T[] items) { return new List<T>(items); }

        /// <summary>
        /// gets an array from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T[] GetArray<T>(params T[] items)
        {
            return items.ToArray();
        }


        /// <summary>
        /// get only raw data from a list of tokens
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static List<string> ToData<T>(List<GenericToken<T>> stream) where T : IComparable, IConvertible, IFormattable
        {
            List<string> data = new List<string>();
            foreach (var t in stream)
            {
                data.Add(t.data);
            }
            return data;
        }


        /// <summary>
        /// split a stream by a particular token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spliton"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static List<List<GenericToken<T>>> Split<T>(T spliton,List<GenericToken<T>> stream) where T : IComparable, IConvertible, IFormattable
        {
            return Split<T>(false, stream, spliton);
        }
        /// <summary>
        /// split stream by particular tokens
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="splitons"></param>
        /// <returns></returns>
        public static List<List<GenericToken<T>>> Split<T>(List<GenericToken<T>> stream,params T[] splitons ) where T : IComparable, IConvertible, IFormattable
        {
            return Split<T>(false, stream, splitons);
        }
        /// <summary>
        /// split stream by particular tokens
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spliton"></param>
        /// <param name="keepsplittok"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static List<List<GenericToken<T>>> Split<T>(bool keepsplittok, List<GenericToken<T>> stream, params T[] splitons ) 
            where T : IComparable, IConvertible, IFormattable
        {
            List<List<GenericToken<T>>> post = new List<List<GenericToken<T>>>();
            List<GenericToken<T>> cur = new List<GenericToken<T>>();
            List<T> splits = new List<T>(splitons);

            foreach (var tok in stream)
            {
                if (splits.Contains(tok.type))
                {
                    if (keepsplittok)
                        cur.Add(tok);
                    post.Add(cur);
                    cur = new List<GenericToken<T>>();
                }
                else
                    cur.Add(tok);
            }
            if (cur.Count > 0)
                post.Add(cur);

            return post;
        }

        /// <summary>
        /// map one set of tokens into another set
        /// (any token in stream not matching incoming side of the map will be excluded)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        /// <returns></returns>
        public static List<GenericToken<U>> MapTokens<U, T>(List<GenericToken<T>> stream, List<T> incoming, List<U> outgoing)
            where T : IComparable, IConvertible, IFormattable
            where U : IComparable, IConvertible, IFormattable
        {
            if (incoming.Count != outgoing.Count)
                throw new Exception("incoming and outgoing token maps must match (have same length).");

            // prep result of the map
            List<GenericToken<U>> f = new List<GenericToken<U>>(stream.Count);

            for (int i = 0; i<stream.Count; i++)
            {
                var t = stream[i];
                // get index of matching incoming token
                var idx = incoming.IndexOf(t.type);
                // exclude anything not incoming map
                if (idx<0)
                    continue;
                // otherwise get the mapped token
                var mappedtoken = outgoing[idx];
                // copy incoming token
                var copy = new GenericToken<U>(mappedtoken, t.data);
                // save it
                f.Add(copy);

            }


            return f;
        }

        /// <summary>
        /// reindex id field of every token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        public static void ReIndexTokens<T>(ref List<GenericToken<T>> stream) where T : IComparable, IConvertible, IFormattable
        {
            ReIndexTokens<T>(ref stream, 0);
        }
        /// <summary>
        /// reindex id field of every token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="starttokenidx"></param>
        public static void ReIndexTokens<T>(ref List<GenericToken<T>> stream, int starttokenidx) where T : IComparable, IConvertible, IFormattable
        {
            for (int i = 0; i < stream.Count; i++)
            {
                var tok = stream[i];
                tok.id = i + starttokenidx;
                stream[i] = tok;
            }
        }

        /// <summary>
        /// keep only specified tokens in stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="keeponly"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> Filter_Keep<T>(List<GenericToken<T>> stream, params T[] keeponly)
            where T : IComparable, IConvertible, IFormattable
        {
            List<GenericToken<T>> f = new List<GenericToken<T>>(stream.Count);
            var keep = new List<T>(keeponly);
            foreach (var t in stream)
            {
                if (keep.Contains(t.type))
                    f.Add(t);
            }

            return f;
        }

        /// <summary>
        /// exclude specified tokens from stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> Filter_Exclude<T>(List<GenericToken<T>> stream, params T[] exclude)
    where T : IComparable, IConvertible, IFormattable
        {
            List<GenericToken<T>> f = new List<GenericToken<T>>(stream.Count);
            var keep = new List<T>(exclude);
            foreach (var t in stream)
            {
                if (!keep.Contains(t.type))
                    f.Add(t);
            }

            return f;
        }

        #endregion

        #region token stream display

        /// <summary>
        /// get a string of key/value pairs of token/type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static string ToIdString<T>(List<GenericToken<T>> toks) where T : IComparable, IConvertible, IFormattable
        {
            return ToIdString<T>(toks, " ");
        }
        /// <summary>
        /// get a string of key/value pairs of token/type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="delim"></param>
        /// <returns></returns>
        public static string ToIdString<T>(List<GenericToken<T>> toks, string delim) where T : IComparable, IConvertible, IFormattable
        {
                        StringBuilder sb = new StringBuilder();
            var usedelim = delim.Length>0;
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                sb.Append(tok.data + "=");
                sb.Append(tok.type.ToString());
                
                if (usedelim)
                    sb.Append(delim);
            }
            return sb.ToString();
        }

        /// <summary>
        /// converts tokens from tok1,tok2,TOK3 to Tok1Tok2Tok3
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static string ToUcString<T>(List<GenericToken<T>> toks) where T : IComparable, IConvertible, IFormattable
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                if (!tok.isValid)
                    continue;
                var d = tok.data;
                // upper case first letter
                var first = new string(new char[] { d[0] }).ToUpperInvariant();
                var full = first + d.Substring(1, d.Length - 1).ToLowerInvariant();
                sb.Append(full);
            }
            return sb.ToString();
        }

        /// <summary>
        /// convert generic tokens back to source string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static string ToString<T>(List<GenericToken<T>> toks) where T : IComparable, IConvertible, IFormattable
        {
            return ToString<T>(toks, string.Empty);
        }
        /// <summary>
        /// convert generic tokens back to source string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static string ToString<T>(List<GenericToken<T>> toks, string delim) where T : IComparable, IConvertible, IFormattable
        {
            StringBuilder sb = new StringBuilder();
            var usedelim = !string.IsNullOrEmpty(delim);
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                sb.Append(tok.data);
                if (usedelim)
                    sb.Append(delim);
            }
            return sb.ToString();
        }

        /// <summary>
        /// convert tokens back to string and clear the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="delim"></param>
        /// <returns></returns>
        public static string Tok2Str_Clear<T>(ref List<GenericToken<T>> toks, string delim) where T : IComparable, IConvertible, IFormattable
        {
            var str = tokh.ToString<T>(toks, delim);
            toks.Clear();
            return str;
        }

        /// <summary>
        /// convert tokens back to string and clear the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static string Tok2Str_Clear<T>(ref List<GenericToken<T>> toks) where T : IComparable, IConvertible, IFormattable
        {
            var str = tokh.ToString<T>(toks);
            toks.Clear();
            return str;
        }

        /// <summary>
        /// take a token stream, split it by a particular token type into new streams, and convert each stream into a string, result as array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="spliton"></param>
        /// <returns></returns>
        public static string[] Tok2SplitStr_Clear<T>(ref List<GenericToken<T>> toks, T spliton) where T : IComparable, IConvertible, IFormattable
        {
            List<string> strs = new List<string>();
            List<GenericToken<T>> cur = new List<GenericToken<T>>();
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                if (tok.type.CompareTo(spliton) == 0)
                {
                    strs.Add(tokh.Tok2Str_Clear<T>(ref cur));
                }
                else
                    cur.Add(tok);
            }
            toks.Clear();
            return strs.ToArray();
        }

        /// <summary>
        /// take a token stream, split it by specific data into new streams, and convert each stream into a string, result as array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="spliton"></param>
        /// <returns></returns>
        public static string[] Tok2SplitStr_Clear<T>(ref List<GenericToken<T>> toks, string spliton) where T : IComparable, IConvertible, IFormattable
        {
            List<string> strs = new List<string>();
            List<GenericToken<T>> cur = new List<GenericToken<T>>();
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                if (tok.data == spliton)
                {
                    strs.Add(tokh.Tok2Str_Clear<T>(ref cur));
                }
                else
                    cur.Add(tok);
            }
            toks.Clear();
            return strs.ToArray();
        }

        /// <summary>
        /// add token to list, convert list of tokens back to string and clear the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static string Tok2Str_AddClear<T>(ref List<GenericToken<T>> toks, GenericToken<T> tok) where T : IComparable, IConvertible, IFormattable
        {
            toks.Add(tok);
            var str = tokh.ToString<T>(toks);
            toks.Clear();
            return str;
        }


        #endregion

        #region token fetchers

        /// <summary>
        /// ensure that there is only one token of each type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> RemoveDuplicateTypes<T>(List<GenericToken<T>> toks)
            where T : IComparable, IConvertible, IFormattable
        {
            List<GenericToken<T>> f = new List<GenericToken<T>>(toks.Count);
            List<T> types = new List<T>();
            foreach (var tok in toks)
            {
                if (types.Contains(tok.type))
                    continue;
                types.Add(tok.type);
                f.Add(tok);
            }
            return f;
        }


        /// <summary>
        /// take a token id and get token index in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int GetTokenIdx<T>(List<GenericToken<T>> toks, GenericToken<T> tok)
    where T : IComparable, IConvertible, IFormattable
        {
            for (int i = 0; i < toks.Count; i++)
            {
                var cmp = toks[i];
                if (cmp.id == tok.id)
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// take a token id and get token index in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int Id2TokenIdx<T>(List<GenericToken<T>> toks, int id)
    where T : IComparable, IConvertible, IFormattable
        {
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                if (tok.id == id)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// take a token position and get token index in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int Position2TokenIdx<T>(List<GenericToken<T>> toks, int pos)
            where T : IComparable, IConvertible, IFormattable
        {
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                if (tok.pos == pos)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// get tokens between two tokens
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> GetTokensBetween<T>(List<GenericToken<T>> toks, GenericToken<T> start, GenericToken<T> end) where T : IComparable, IConvertible, IFormattable
        {
            var sidx = GetTokenIdx<T>(toks, start);
            var eidx = GetTokenIdx<T>(toks, end);
            return GetTokensBetween<T>(toks, sidx, eidx);
        }
        /// <summary>
        /// get tokens between two positions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="startatidx"></param>
        /// <param name="endidx"></param>
        /// <returns></returns>
        public static List<GenericToken<T>> GetTokensBetween<T>(List<GenericToken<T>> toks, int startatidx, int endidx) where T : IComparable, IConvertible, IFormattable
        {
            List<GenericToken<T>> slice = new List<GenericToken<T>>(endidx - startatidx);
            for (int i = startatidx; i < endidx; i++)
            {
                slice.Add(toks[i]);
            }
            return slice;
        }



        /// <summary>
        /// get next token of a particular type, if not found returns last (or first) token in stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="startatidx"></param>
        /// <param name="lookbefore"></param>
        /// <param name="onlyallow"></param>
        /// <returns></returns>
        public static GenericToken<T> GetNextOrLastToken_Of<T>(List<GenericToken<T>> toks, int startatidx, bool lookbefore, params T[] onlyallow)
    where T : IComparable, IConvertible, IFormattable
        {
            var tok = GetNextToken_Of<T>(toks, startatidx, lookbefore, onlyallow);
            if ((tok == null) || !tok.isValid)
            {
                if (lookbefore)
                    return toks[0];
                else
                    return toks[toks.Count - 1];
            }
            return tok;
        }

        /// <summary>
        /// get next token of a particular type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="startatidx"></param>
        /// <param name="lookbefore"></param>
        /// <param name="onlyallow"></param>
        /// <returns></returns>
        public static GenericToken<T> GetNextToken_Of<T>(List<GenericToken<T>> toks, int startatidx, bool lookbefore, params T[] onlyallow)
    where T : IComparable, IConvertible, IFormattable
        {
            var inc = lookbefore ? -1 : 1;
            var curtokenidx = startatidx;
            var max = toks.Count;
            var filter = new List<T>(onlyallow);
            while (((curtokenidx + inc) >= 0) && (curtokenidx + inc < max))
            {
                var nextidx = curtokenidx + inc;
                var next = toks[nextidx];
                curtokenidx = nextidx;
                if (!filter.Contains(next.type))
                    continue;
                return next;

            }
            return GetInvalid<T>();
        }


        /// <summary>
        /// get next token that is not some token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="startat"></param>
        /// <param name="lookbefore"></param>
        /// <param name="excludetypes"></param>
        /// <returns></returns>
        public static GenericToken<T> GetNextToken_Skip<T>(List<GenericToken<T>> toks, int startat, bool lookbefore, params T[] excludetypes) 
            where T : IComparable, IConvertible, IFormattable
        {
            var inc = lookbefore ? -1 : 1;
            var curtokenidx = startat;
            var max = toks.Count;
            var filter = new List<T>(excludetypes);
            while (((curtokenidx + inc)>=0) && (curtokenidx+inc<max))
            {
                var nextidx = curtokenidx + inc;
                var next = toks[nextidx];
                curtokenidx = nextidx;
                if (filter.Contains(next.type))
                    continue;
                return next;
            
            }
            return GetInvalid<T>();
        }

        #endregion

        #region tests


        /// <summary>
        /// check a token stream to see if definitions are producing the expected type match for some data in the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toks"></param>
        /// <param name="expectdata"></param>
        /// <param name="expecttype"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool isTokenDefOk<T>(List<GenericToken<T>> toks, string expectdata, T expecttype, DebugDelegate d)
             where T : IComparable, IConvertible, IFormattable
        {
            for (int i = 0; i < toks.Count; i++)
            {
                var tok = toks[i];
                if (tok.data.Trim().ToLower() == expectdata.ToLower())
                {
                    if (tok.isToken(expecttype))
                        continue;
                    if (d != null)
                        d(i.ToString() + " token: " + tok.ToString() + " MISMATCH expected: " + expecttype.ToString());
                    return false;

                }
            }
            return true;
        }

        /// <summary>
        /// identifies whether a stream is *likely* filter or complete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool isCompleteStream<T>(List<GenericToken<T>> stream) where T : IComparable, IConvertible, IFormattable
        {
            return isCompleteStream<T>(stream, 1000);
        }
        /// <summary>
        /// identifies whether a stream is *likely* filter or complete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool isCompleteStream<T>(List<GenericToken<T>> stream, int maxchecks) where T : IComparable, IConvertible, IFormattable
        {
            
            for (int i = 0; i < stream.Count; i++)
            {
                if (i >= maxchecks)
                    return true;
                if (stream[i].id != i)
                    return false;
            }
            return true;
        }

        #endregion

    }

    public class tokh : GenericTokenHelper { }
}
