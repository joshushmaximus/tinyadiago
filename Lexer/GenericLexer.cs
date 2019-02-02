using System;
using System.IO;


namespace TokenParsing
{
    public class GenericLexer<T> : IDisposable where T : IComparable, IConvertible, IFormattable
    {
                private readonly TextReader reader;
                private readonly GenericTokenDefinition<T>[] tokenDefinitions;

        private string lineRemaining;

        public int BytesLeft { get { return string.IsNullOrWhiteSpace(lineRemaining) ? 0 : lineRemaining.Length; } }

        private string readertag = string.Empty;

        public GenericToken<T> Token { get; private set; }

        

        public int LineNumber { get; private set; }

        public int Position { get; private set; }

        public GenericLexer(string sourcetag, string lexdata, GenericTokenDefinition<T>[] defs) : this(sourcetag, new System.IO.StringReader(lexdata), defs) { }
        public GenericLexer(string sourcetag, TextReader reader, GenericTokenDefinition<T>[] tokenDefinitions)
        {
            this.reader = reader;
            readertag = sourcetag;
            this.tokenDefinitions = tokenDefinitions;
            
            lineRemaining = reader.ReadToEnd();
        }

        public bool Next()
        {
            // quit when there's nothing left to parse
            if (lineRemaining.Length==0)
                return false;

            // match against first token defintion found
            for (int i  = 0; i<tokenDefinitions.Length; i++)
            {
                var def = tokenDefinitions[i];
                // attempt to match
                var matched = def.Matcher.Match(lineRemaining);
                
                // if we got a hit
                if (matched > 0)
                {
                    // track position
                    Position += matched;
                    
                    // create token from the definition
                    var thistok = def.FullToken;
                    thistok.data = lineRemaining.Substring(0, matched);
                    thistok.pos = Position;
                    thistok.line = LineNumber;
                    thistok.source = readertag;
                    thistok.isEOLToken = def.isTreatedAsLineTerminator;
                    Token = new GenericToken<T>(thistok);
                    Token.DefMatched = def;
                     
                    // remove what we matched
                    lineRemaining = lineRemaining.Substring(matched);

                    // count lines
                    if (Token.isEOLToken)
                        LineNumber++;

                    return true;
                }
            }
            // everything that is read in must match to some token
            throw new Exception(string.Format("Unable to match against any tokens at line {0} position {1} \"{2}\"",
                                              LineNumber, Position, lineRemaining));
        }



        public void Dispose()
        {
            reader.Dispose();
        }

        static void noop()
        {
            System.Threading.Thread.Sleep(0);
        }
    }

    
}
