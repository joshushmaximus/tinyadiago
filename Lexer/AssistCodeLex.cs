using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prac.Core;
using Prac.API;
using System.IO;

namespace GleanCommon
{
    
    public class AssistCodeLex
    {
        private static TokenDefinition[] gettokdefs()
        {
            
            // build token definitions
            var tokdefs = new TokenDefinition[] { 
                new TokenDefinition(@"[@]?([""'])(?:\\\1|.)*?\1", new Token(tokentype.quotedstring)),
                new TokenDefinition(@"//.*", new Token(tokentype.comment)),
                new TokenDefinition(@"[-+]?\d*\.\d+([eE][-+]?\d+)?", new Token(tokentype.var_float)),
                new TokenDefinition(@"[-+]?\d*\.\d+([eE][-+]?\d+m)?", new Token(tokentype.var_decimal)),
                new TokenDefinition(@"[-+]?\d+", new Token(tokentype.var_int)),
                new TokenDefinition(@"#t", new Token(tokentype.var_bool_true)),
                new TokenDefinition(@"#f", new Token(tokentype.var_bool_false)),
                new TokenDefinition(@"[*<>\?_A-Za-z->!][_0-9A-Za-z->!]*", new Token(tokentype.symbol)),
                new TokenDefinition(@"\.", new Token(tokentype.dot)),
                new TokenDefinition(@"\:", new Token(tokentype.colon)),
                new TokenDefinition(@"\,", new Token(tokentype.comma)),
                new TokenDefinition(@"[#]", new Token(tokentype.bash)),
                new TokenDefinition(@"[=]", new Token(tokentype.assign)), 

                new TokenDefinition(@"\|\|", new Token(tokentype.or)),
                new TokenDefinition(@"\&\&", new Token(tokentype.and)),
                new TokenDefinition(@"\+", new Token(tokentype.add)),
                new TokenDefinition(@"\\", new Token(tokentype.divide)),
                new TokenDefinition(@"\*", new Token(tokentype.multiply)),
                new TokenDefinition(@"\-", new Token(tokentype.subtract)),
                new TokenDefinition(@"\(", new Token(tokentype.parens_start)),
                new TokenDefinition(@"\)", new Token(tokentype.parens_end)),
                new TokenDefinition(@"\[", new Token(tokentype.bracket_start)),
                new TokenDefinition(@"\]", new Token(tokentype.bracket_end)),
                new TokenDefinition(@"[{]", new Token(tokentype.block_start)),
                new TokenDefinition(@"[}]", new Token(tokentype.block_end)),
                new TokenDefinition(@"\s", new Token(tokentype.space)),
                new TokenDefinition(@"[;]", new Token(tokentype.statement)),

            };
            return tokdefs;
        }

        private static List<Token> tokstream = new List<Token>();
        private static int primarytokencount = -1;

        private static bool createstream(string source,string raw) { return createstream(source,raw, true); }
        private static bool createstream(string source, string raw, bool clear)
        {
            // prep stream
            var tr = new System.IO.StringReader(raw);
            // prepare to tokenize it
            Lexer lex = new Lexer(source,tr, gettokdefs());
            if (clear)
            {
                primarytokencount = -1;
                tokstream.Clear();
            }
            
            // get tokens
            while (lex.Next())
            {
                var t = lex.Token;
                t.id = tokstream.Count;
                if ((primarytokencount < 0) && (tokstream.Count > 1) && (tokstream[t.id - 1].source != t.source))
                    primarytokencount = t.id;
                tokstream.Add(t);
            }
            if (primarytokencount < 0)
                primarytokencount = tokstream.Count;
            return true;
        }

        internal static void printstream(string raw) { printstream(raw, true); }
        internal static void printstream(string raw, bool ignorespace)
        {
            if (!createstream(string.Empty,raw,true))
                return;
            // print it
            for (int i = 0; i < tokstream.Count; i++)
            {
                // get token
                var tok = tokstream[i];
                // ignore tokens
                if (tok.type == tokentype.space)
                    continue;
                Console.WriteLine(tok);
            }
        }

        internal static bool istoksymbol(Token t)
        {
            return t.type == tokentype.symbol;
        }


        internal static bool getfunctionstartendidx(ref int start, out int end)
        {
            end = -1;
            int blockcounter = 0;
            bool findstart = true;
            bool findend = false;
            for (int i = start; i < tokstream.Count; i++)
            {
                // get token
                var tok = tokstream[i];
                var tt = tok.type;
                // ensure we never have negative counter
                if (blockcounter < 0)
                    throw new Exception("Invalid blockcounter when detecting function start at: " + i + " from: " + start);
                if (tt == tokentype.block_start)
                {
                    blockcounter++;
                    if (findstart)
                    {
                        start = i + 1;
                        findstart = false;

                        findend = true;
                    }
                }
                else if (findend && (tt == tokentype.block_end))
                {
                    blockcounter--;
                    if (blockcounter == 0)
                    {
                        end = i - 1;
                        break;
                    }
                }
            }
            return (end>0) && (end>start);
        }

        internal static Token[] getfunctionarguments(int starttoken)
        {
            // prepare argument token storage
            List<Token> args = new List<Token>();
            var isstarted = false;
            var isend = false;
            // process input tokens

            for (int i = starttoken; i < tokstream.Count; i++)
            {
                // get token
                var t = tokstream[i];
                // look for end
                if (isstarted)
                {
                    isend = t.type == tokentype.parens_end;
                }
                else // look for start of arguments
                {
                    isstarted = t.type == tokentype.parens_start;
                }
                // if it's an argument, save it
                if (isstarted)
                    args.Add(t);
                // quit if end
                if (isend)
                    break;
            }
            return args.ToArray();

        }
        /// <summary>
        /// get function body tokens (function name and argument list not included)
        /// </summary>
        /// <param name="starttoken"></param>
        /// <returns></returns>
        internal static Token[] getfunctiontokens(int starttoken)
        {
            // get block
            int endidx;
            if (!getfunctionstartendidx(ref starttoken, out endidx))
                return null;
            // get slice of tokens
            var ftoks = new Token[endidx - starttoken];
            tokstream.CopyTo(starttoken, ftoks, 0, ftoks.Length);
            return ftoks;
        }

        internal static string gettokencode(Token[] ftoks)
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (var tok in ftoks)
            {
                if (tok.isLine)
                    sb.AppendLine();
                else
                    sb.Append(tok.data);
            }
            // get code
            var code = sb.ToString();
            // retab result
            code = h.retab(code);
            return code;
        }


        internal static string getclassname(string astfuncname)
        {
            return "RD_"+astfuncname.Replace("Get", string.Empty);
        }

        internal static Token getprevioussymbol(Token t) { return getprevioussymbol(t.id); }
        internal static Token getprevioussymbol(int startidx)
        {
            for (int i = startidx-1; (i >= 0); i--)
            {
                var t = tokstream[i];
                if (t.type == tokentype.symbol)
                    return t;

            }
            return new Token();
        }

        internal static Token getnextsymbol(Token t) { return getnextsymbol(t.id,1); }
        internal static Token getnextsymbol(Token t, int next) { return getnextsymbol(t.id, next); }

        internal static Token getnextsymbol(int startidx, int next)
        {
            int found = 0;
            for (int i = startidx+1; i<tokstream.Count; i++)
            {
                var t = tokstream[i];
                if (t.type == tokentype.symbol)
                {
                    found++;
                    if (found==next)
                        return t;
                }

            }
            return new Token();
        }

        /// <summary>
        /// call or declaration
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static bool issymbolmethod(Token t)
        {
            // start past token idx
            var start = t.id + 1;

            int pcount = 0;
            bool blockready = false;
            bool argstart = false;
            for (int i = start; i < tokstream.Count; i++)
            {
                // get next token
                var n = tokstream[i];
                // skip any spaces
                if (n.isWhiteSpace)
                    continue;
                // next must be left parents
                var isparen = (n.type == tokentype.parens_start);
                if (!argstart && !isparen)
                    return false;
                else if (isparen)
                {
                    argstart = true;
                    pcount++; // count parentesis
                    // get next token
                    continue;
                }
                // count end blocks
                if (argstart && !blockready)
                {
                    // skip any symbols
                    if (n.isSymbol || n.isStringConstant)
                        continue;
                    // skip any commas
                    if (n.type == tokentype.comma)
                        continue;
                    // count right parens
                    if (n.type == tokentype.parens_end)
                    {
                        pcount--;
                        continue;
                    }
                    
                }
                blockready = argstart && (pcount == 0);
                if (blockready)
                {
                    if (n.isWhiteSpace)
                        continue;
                    // need method/function block start
                    if (n.type != tokentype.block_start)
                        return false;
                    return true;
                }
                

            }
            return false;
        }

        internal static bool issymbolvoidargumentmethod(Token t)
        {
            // start past token idx
            var start = t.id+1;
            
            for (int i = start; i < tokstream.Count; i++)
            {
                // get next token
                var n = tokstream[i];
                // skip any spaces
                if (n.isSpace)
                    continue;
                // next must be right parents
                if (n.type != tokentype.parens_start)
                    return false;
                // advance
                i++;
                n = tokstream[i];
                // skip any more spaces
                if (n.isSpace)
                    continue;
                // next must be left parens
                if (n.type != tokentype.parens_end)
                    break;
                return true;
            }
            return false;
        }

        internal static bool issymbolpresent(string symbol, params Token[] tokenlist)
        {

            foreach (var t in tokenlist)
                if (t.isSymbol && (t.data == symbol))
                    return true;
            return false;
        }

        static System.CodeDom.Compiler.CodeDomProvider csp = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#");
        internal static bool isreservedsymbol(string symbol)
        {
            return !csp.IsValidIdentifier(symbol);
        }

        internal static List<string> GetSourceSymbols(string sourcetype)
        {
            // get type
            var t = Type.GetType(sourcetype);
            // prepare name list
            List<string> names = new List<string>();
            // get all fields
            var ms = t.GetFields(System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.FlattenHierarchy | 
                System.Reflection.BindingFlags.Static);

            // add to names
            foreach (var m in ms)
                names.Add(m.Name);

            // get all static methods
            var fis = t.GetMethods(System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Static | 
                System.Reflection.BindingFlags.FlattenHierarchy);
            // add to names
            foreach (var m in fis)
                names.Add(m.Name);
            return names;
        }

        internal static Token[] rewrite_nonlocalsymbols(Token[] sourcetokens, string originalsymbol_location) { return rewrite_nonlocalsymbols(sourcetokens, originalsymbol_location, originalsymbol_location); }
        internal static Token[] rewrite_nonlocalsymbols(Token[] sourcetokens, string originalsymbol_location, string finallocation)
        {
            // get destination class symbols
            var classsym = RD_AssistTemplate.GetClassSymbols();
            // get source symbols
            var sourcesym = GetSourceSymbols(originalsymbol_location);
            // process every source symbol
            for (int i = 0; i < sourcetokens.Length; i++)
            {
                var t = sourcetokens[i];
                if (!t.isSymbol)
                    continue;
                // get symbol
                var sym = t.data;

                // ignore reserved words
                if (isreservedsymbol(sym))
                    continue;
                // ignore if symbol is local to class
                if (classsym.Contains(sym))
                    continue;
                // re-write any from sourceclass
                if (sourcesym.Contains(sym))
                {
                    t.data = finallocation + "." + t.data;
                    sourcetokens[i] = t;
                    continue;
                }
                // other symbols should be already defined as local
            }
            return sourcetokens;
        }

        internal static List<Token> readtofirstof(int starttok, tokentype tt)
        {
            List<Token> toks = new List<Token>();
            for (int i = starttok; i < tokstream.Count; i++)
            {
                // get token
                var t = tokstream[i];
                // save it
                toks.Add(t);
                // see if this is our end token
                if (t.type == tt)
                    break;
            }
            return toks;
        }

        static Token[] getlasttokens(int start, int lookback)
        {
            Token[] toks = new Token[lookback];
            tokstream.CopyTo(start-lookback, toks, 0, lookback);
            return toks;
        }

        static bool issymbolassignment(Token symbol)
        {
            if (!symbol.isSymbol)
                return false;
            for (int i = symbol.id+1; i < tokstream.Count; i++)
            {
                var t = tokstream[i];
                // skip whitespace
                if (t.isWhiteSpace)
                    continue;
                return t.isAssign;
            }
            return false;
        }

        internal static Token[] gethelperconstants(Token[] ftoks, GenericTracker<Token[]> const2toks)
        {
            List<Token> usedconstants = new List<Token>();
            // keep track of what has been added
            bool[] constadded = new bool[const2toks.Count];

            // go through every function token
            foreach (var tok in ftoks)
            {
                // skip anything that is not a symbol
                if (!tok.isSymbol)
                    continue;
                // see if this token is a constant
                int idx = const2toks.getindex(tok.data);
                // it's not, so we don't need to declare it's implementation
                if (idx < 0)
                    continue;
                else if (constadded[idx]) // it's already been added
                    continue;
                else // otherwise we likely need to define it
                {
                    // define it
                    usedconstants.AddRange(const2toks[idx]);
                    // add new line
                    usedconstants.Add(new Token( tokentype.line));
                    // mark it as defined
                    constadded[idx] = true;
                }

            }

            // return all the likely helpers
            return usedconstants.ToArray();
        }

        internal static string gethelpercode(string fname, GenericTracker<List<Token>> functionname2body)
        {
            var fidx = functionname2body.getindex(fname);
            var stublessimplementation = getstublessfunction(fidx, functionname2body);
            var funccode = gettokencode(stublessimplementation.ToArray());
            return funccode;
            
        }

        internal static GenericTracker<Token[]> getcodeconstants()
        {
            GenericTracker<Token[]> constname2consttoks = new GenericTracker<Token[]>();
            // keep track of block level
            int blocklevel = 0;
            const int classglobal = 2;
            // go through every token
            for (int i = 0; i < tokstream.Count; i++)
            {
                // get token
                var t = tokstream[i];
                // ignore quoted strings
                if (t.type == tokentype.quotedstring)
                    continue;
                // ignore new lines 
                if (t.isLine)
                    continue;
                // count blocks
                if (t.type == tokentype.block_end)
                    blocklevel--;
                else if (t.type == tokentype.block_start)
                    blocklevel++;
                // only look for constants at class level
                if (blocklevel == classglobal)
                {
                    // ignore non symbols
                    if (!t.isSymbol)
                        continue;
                    // look for any constant
                    if (t.data != "const")
                        continue;
                    // ensure it's an access modifier
                    //if (ptok.data != "public")
                    //    continue;
                    // get next token for the name
                    var n = getnextsymbol(t,2);
                    // ensure it's an assignment
                    if (!issymbolassignment(n))
                        continue;
                    // save tokens until the end of the constant
                    var ctoks = readtofirstof(t.id, tokentype.statement);
                    // save it
                    constname2consttoks.addindex(n.data,ctoks.ToArray());
                    // advance the pointer
                    i += (ctoks.Count - (i-t.id));
                    //var lasttoks = getlasttokens(i, 70);
                    //Console.WriteLine(n.ToString() + "\t" + lasttoks.Length);
                }
            }
            return constname2consttoks;
        }



        static List<string> getallsymbolspresent(Token[] toks) { List<string> syms = new List<string>(); return getallsymbolspresent(toks, syms,null); }
        static List<string> getallsymbolspresent(Token[] toks, List<string> syms, Prac.API.GenericTrackerI gt)
        {
            
            foreach (var t in toks)
                if (t.isSymbol && !syms.Contains(t.data))
                {
                    if (gt==null)
                        syms.Add(t.data);
                    else if (gt.getindex(t.data)>=0)
                        syms.Add(t.data);
                }
            return syms;
        }

        internal static bool transform_assist_helpers(List<string> sourcetypes, GenericTracker<Token[]> const2tokens, ref GenericTracker<List<Token>> function2functoks)
        {
            

            // get destination class symbols
            var templateclasssym = RD_AssistTemplate.GetClassSymbols();
            // process every token
            for (int i = 0; i < tokstream.Count; i++)
            {
                // get our building blocks
                var tok = tokstream[i];
                // look for symbols
                if (!tok.isSymbol)
                    continue;
                // ignore constructors
                if (sourcetypes.Contains(tok.data))
                    continue;
                // skip methods we have overridden in destination class  (eg GetSymbolDecimal)
                if (templateclasssym.Contains(tok.data))
                    continue;
                // get previous token symbol
                var ptok = getprevioussymbol(tok);
                // verify we return an assist
                if (ptok.data != "AssistI")
                    continue;
                // update other states
                //bool isvoidmethod = issymbolvoidargumentmethod(tok);
                //if (isvoidmethod)
                //    continue;
                bool ismethod = issymbolmethod(tok);

                


                // take the non-void static methods which return assists and place them as helpers
                if (ismethod)
                {
                    // get name of function
                    var fname = tok.data;
                    // get function tokens including the argument list (but not the static token)
                    var allfunction = new List<Token>();
                    // save this name
                    var ftokens = getfunctiontokens(i);
                    // get the constants used by this helper
                    var consttoks = gethelperconstants(ftokens, const2tokens);
                    // build entire function starting with return arguments
                    allfunction.Add(new Token( tokentype.symbol,"AssistI"));
                    allfunction.Add(new Token( tokentype.space, " "));
                    // name
                    allfunction.Add(tok); 
                    // arguments
                    allfunction.AddRange(getfunctionarguments(i)); 
                    // re-write any functions not present in destination
                    //var rewrittenbody = rewrite_nonlocalsymbols(ftokens, sourcetype);
                    // body
                    allfunction.Add(new Token(tokentype.line, h.line())); 
                    allfunction.Add(new Token(tokentype.block_start, "{"));
                    allfunction.Add(new Token(tokentype.line, h.line()));
                    // constants
                    allfunction.AddRange(consttoks);
                    // code
                    allfunction.AddRange(ftokens);
                    // end body
                    allfunction.Add(new Token(tokentype.line, h.line())); 
                    allfunction.Add(new Token(tokentype.block_end, "}"));
                    allfunction.Add(new Token(tokentype.line, h.line())); 
                    // get code from these modified tokens
                    //var bodycode = gettokencode(allfunction.ToArray());
                    // see if we have some code already
                    int tokencodeidx = function2functoks.getindex(tok.data);
                    
                    // if not, save it
                    if (tokencodeidx < 0)
                    {

                        tokencodeidx = function2functoks.addindex(tok.data, allfunction);
                    }
                    else // otherwise it's an overload, append it
                    {
                        function2functoks[tokencodeidx].AddRange(allfunction);
                    }

                }
            }



            return true;
        }

        static GenericTracker<List<Token>> getstublessftable(GenericTracker<List<Token>> function2functoks)
        {
            var tmp = function2functoks.ToArray();
            var stubtokens = new GenericTracker<List<Token>>(tmp.Length);
            for (int i = 0; i < tmp.Length; i++)
                stubtokens.addindex(function2functoks.getlabel(i), function2functoks[i]);
            // now process every function stub
            for (int i = 0; i < function2functoks.Count; i++)
            {
                // always pass in the stubs
                var stubless = getstublessfunction(i, stubtokens);
                // update the primary list with the stubless result
                function2functoks[i] = stubless;

            }
            return function2functoks;
        }

        static int GetTokenSymCount(string sym, Token[] toks)
        {
            int c = 0;
            foreach (var t in toks)
                if (t.isSymbol && (sym == t.data))
                    c++;
            return c;
        }

        static int GetFunctionDefCount(string sym, string returntype, Token[] toks)
        {
            int c = 0;
            foreach (var t in toks)
                if (t.isSymbol && (sym == t.data))
                {
                    var ptok = getprevioussymbol(t);
                    if (ptok.isSymbol && (ptok.data==returntype))
                        c++;
                }
            return c;
        }

        static List<Token> getstublessfunction(int fidx, GenericTracker<List<Token>> stubs)
        {
            // get copy of the current stub
            var ftoks = new List<Token>(stubs[fidx]);
            // get this function's symbol
            var fsym = new string[] { stubs.getlabel(fidx) };
            // prepare a list of all symbols slurped into ftoken table
            var syms = new List<string>(fsym);
            bool allsymbolsslurped = false;
            do
            {
                var lastsymcount = syms.Count;
                // look for all new symbols
                syms = getallsymbolspresent(ftoks.ToArray(), syms, stubs);
                // if found, slurp tokens and rerun until no new symbols' tokens are slurped
                for (int i = lastsymcount; i < syms.Count; i++)
                {
                    // get new symbol to slurp
                    var newfuncsym = syms[i];
                    // grab the tokens for this symbol
                    var newtoks = stubs[newfuncsym];
                    // slurp it into this function's implementation
                    ftoks.AddRange(newtoks);

                }
                // update continue state
                allsymbolsslurped = lastsymcount == syms.Count;
                // continue until all needed symbols are available in this substream
            } while (!allsymbolsslurped);
            return ftoks;

        }





        internal static bool transform_assist_calls(string groupname, string sourcetype, GenericTracker<List<Token>> symbolname2functiontoks, GenericTracker<Token[]> const2code, out string code, out List<string> typenames)
        {
            code = string.Empty;
            // get all assists in question
            var masterlist = AssistHelper.getavailableassists_names();
            var mastercopy = masterlist.ToArray();
            for (int i = 0; i < masterlist.Count; i++)
                masterlist[i] = masterlist[i].Replace(sourcetype+":", string.Empty);
            // strip out their location

            int okcount = 0;
            int classes = 0;
            typenames = new List<string>();
            // process every primary source token
            for (int i = 0; i < primarytokencount; i++)
            {
                // get our building blocks
                var tok = tokstream[i];
                // see if we want it
                if (!tok.isSymbol)
                    continue;
                if (sourcetype.Contains(tok.data))
                    continue;
                // get previous token symbol
                var ptok = getprevioussymbol(tok);
                // verify we return an assist
                if (ptok.data != "AssistI")
                    continue;
                // update other states
                bool voidarguments = issymbolvoidargumentmethod(tok);
                var masteridx = masterlist.IndexOf(tok.data);
                bool userfacing = (masteridx >= 0);
                bool hasname = userfacing && (AssistHelper.getassist(mastercopy[masteridx],null).AssistName!="GleanCommon.Assist");
                // verify our current token is a void method that is user-facing
                if (voidarguments && userfacing && hasname) 
                {
                    // get name
                    var fname = tok.data;
                    // get any helper code which might be called by our function
                    var helpercode = gethelpercode(fname,symbolname2functiontoks);
                    // get new class name (that will inherit from RD_AssistTemplate)
                    var classname = getclassname(fname);
                    // get the pretty name
                    var prettyname = GleanHelper.getsafevarname(AssistHelper.getassist(mastercopy[masteridx],null).AssistName);
                    // insert assist code RD class of the same name (eg GetOffsetTracker or taBollinger) 
                    var classdefinition = RD_AssistTemplate.GetTemplateClass(classname,prettyname, "return "+fname+"();",helpercode, false, false);
                    code += classdefinition;
                    // count it
                    classes++;
                    okcount++;
                    typenames.Add(classname);
                }
                
            }






            return true;
        }

        private static string getsourcetype(string basens, string file)
        {
            return basens + "." + Path.GetFileNameWithoutExtension(file);
        }

        private static List<string> getallsourcetypes(string basens, string[] inputfiles)
        {
            List<string> sts = new List<string>(inputfiles.Length);
            foreach (var inf in inputfiles)
                sts.Add(getsourcetype(basens, inf));
            return sts;

        }
    
        public static bool GenRDTemplates(string outputtemplatefilename, string basenamespace, params string[] inputfiles) { string tmp; return GenRDTemplates(outputtemplatefilename, basenamespace, out tmp, inputfiles); }
        public static bool GenRDTemplates(string outputtemplatefilename,string basenamespace,out string code, params string[] inputfiles)
        {
            code = string.Empty;
            // tokenize data
            bool tokok = true;
            for (int i = 0; i < inputfiles.Length; i++)
            {
                var source = inputfiles[i];
                var inputcode = h.getfile(source,null);
                tokok &= createstream(source, inputcode, i == 0);
            }

            if (!tokok)
                return false;

            // generate constants
            GenericTracker<Token[]> const2consttoks = getcodeconstants();

            // generate helpers

            GenericTracker<List<Token>> function2ftoks = new GenericTracker<List<Token>>();
            bool helperok = transform_assist_helpers(getallsourcetypes(basenamespace,inputfiles), const2consttoks, ref function2ftoks);

            
            // generate classes
            string assistcode;
            List<string> typenames;
            var assistok = transform_assist_calls(outputtemplatefilename, getsourcetype(basenamespace, inputfiles[0]), 
                function2ftoks, const2consttoks, out assistcode, out typenames);

            if (helperok && assistok)
            {
                // generate method to get all the types as params
                var getallmethod = gencode_gettypes_fromnames(outputtemplatefilename, typenames);

                // add to global RD listing class
                var rdlisting_class = TargetMap.GetClass(outputtemplatefilename, string.Empty, getallmethod);

                // wrap it all together
                code = RD_AssistTemplate.GetTemplateFile(rdlisting_class + assistcode);

                // save class if required
                bool savefile = !string.IsNullOrWhiteSpace(outputtemplatefilename);
                //var folder = h.gf();
                var folder = Environment.CurrentDirectory + "\\";
                var fn = folder + outputtemplatefilename + ".cs";
                if (savefile && !string.IsNullOrWhiteSpace(code) && h.setfile(fn, code))
                {
                    
                }
                 

                return true;

            }

            return false;

        }

        const string GETTYPEFUNCTION = "GetAllTypes";
        public static string gencode_gettypes_fromnames(string name,List<string> names)
        {
            // build the type list
            CodeGen tl = new CodeGen();
            const string ts = "types";
            const string ns = "names";
            tl.stmt("var "+ts+" = new List<Type>()");
            tl.stmt("var "+ns+" = new string[] { \""+string.Join("\",\"",names.ToArray())+"\"}");
            tl.line("foreach (var n in "+ns+")");
            tl.block_start();
            tl.com("get type");
            tl.stmt("var t = Type.GetType(\"GleanCommon.\"+n)");
            tl.com("save it");
            tl.stmt("if (t!=null) "+ts+".Add(t)");
            tl.block_end();
            tl.stmt("return " + ts);
            tl.line();
            // wrap function around it
            var code = TargetMap.GetMethod(GETTYPEFUNCTION+"_"+name,"static List<Type>", string.Empty, false,tl);
            code = h.retab(code);
            return code;
        }
        
    }

   

    



}
