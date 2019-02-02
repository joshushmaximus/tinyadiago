using System;
using TokenParsing;
//using GleanCommon;
using System.Collections.Generic;
using TinyAdiago;

namespace TokenParsing
{
    /// <summary>
    /// hold user registration data created from emailregistration control
    /// </summary>
    public class RegisterDB
    {
        public string Program = string.Empty;
        public string FirstName = string.Empty;
        public string LastName = string.Empty;
        public string Email = string.Empty;
        public string MID = string.Empty;
        public string Code = string.Empty;
        public DateTime ParseDate = DateTime.MinValue;
        public bool isParseDateValid { get { return ParseDate != DateTime.MinValue; } }
        public bool isValidEmail { get { return !string.IsNullOrWhiteSpace(Email); } }
        public bool isValidProgram { get { return !string.IsNullOrWhiteSpace(Program); } }
        public bool isValid { get { return isParseDateValid && isValidEmail && isValidProgram; } }

        
        public static RegisterDB Create(string program, string first, string last, string email, string mid, string code)
        {
            var db = new RegisterDB();
            db.Email = email;
            db.Program = program;
            db.FirstName = first;
            db.LastName = last;
            db.MID = mid;
            db.Code = code;
            db.ParseDate = DateTime.Now;
            return db;
        }

        public const string CURRENTDB_NAME = "register_emaildb.20150608.txt";

        public static List<string> CSVHeaders() { return CSVHeaders(false); }
        public static List<string> CSVHeaders(bool useall)
        {
            List<string> headers = new List<string>();
            headers.AddRange(new string[] { "email", "first", "last", "mid" });
            if (useall)
                headers.AddRange(new string[] { "code", "program", "parsedate" });
            return headers;
        }

        public static List<List<string>> ToCSV(Prac.API.GenericTracker<RegisterDB> db) { return ToCSV(false, db); }
        public static List<List<string>> ToCSV(bool useall, Prac.API.GenericTracker<RegisterDB> db)
        {
            
            List<List<string>> all = new List<List<string>> ();
            for (int i = 0; i < db.Count; i++)
            {
                var user = db[i];
                List<string> row = new List<string>();
                row.AddRange(new string[] { user.Email, user.FirstName, user.LastName, user.MID });
                if (useall)
                {
                    row.AddRange(new string[] { user.Code, user.Program, user.ParseDate.ToShortDateString() });
                }
                all.Add(row);
            }
            return all;
        }

        public static Prac.API.GenericTracker<RegisterDB> Parse(string data, DebugDelegate d, Prac.API.Int32Delegate progress)
        {
            Prac.API.GenericTracker<RegisterDB> db = new Prac.API.GenericTracker<RegisterDB>();
            // get token defs 
            var defs = RegisterDB.GetDefs().ToArray();
            // get token stream
            var stream = tokh.GetStream<register_toks>(string.Empty, data, defs, d,progress);
            // strip out everything we don't care about
            var final = tokh.Filter_Keep<register_toks>(stream, 
                                register_toks.endrecord, register_toks.key_reg_code, register_toks.key_reg_email,
                                register_toks.key_reg_firstname, register_toks.key_reg_lastname, register_toks.key_reg_mid, register_toks.key_registration_program,
                                register_toks.quotedstring);
            RegisterDB cur = new RegisterDB();
            int startrecidx = 0;
            for (int i = 0; i<final.Count; i++)
            {
                var tok = final[i];
                var next = i<final.Count-1 ? final[i+1] : new GenericToken<register_toks>( register_toks.none);
                switch (tok.type)
                {
                    case register_toks.key_reg_code:
                        {
                            if (next.isToken(register_toks.quotedstring))
                                cur.Code = next.data.Replace("'",string.Empty);
                        }
                        break;
                    case register_toks.key_reg_email:
                        {
                            if (next.isToken(register_toks.quotedstring))
                                cur.Email = next.data.Replace("'", string.Empty);
                        }
                        break;
                    case register_toks.key_registration_program:
                        {
                            if (next.isToken(register_toks.quotedstring))
                                cur.Program = next.data.Replace("'", string.Empty);
                        }
                        break;
                    case register_toks.key_reg_mid:
                        {
                            if (next.isToken(register_toks.quotedstring))
                                cur.MID = next.data.Replace("'", string.Empty);
                        }
                        break;
                    case register_toks.key_reg_firstname:
                        {
                            if (next.isToken(register_toks.quotedstring))
                                cur.FirstName = next.data.Replace("'", string.Empty);
                        }
                        break;
                    case register_toks.key_reg_lastname:
                        {
                            if (next.isToken(register_toks.quotedstring))
                                cur.LastName = next.data.Replace("'", string.Empty);
                        }
                        break;
                    case register_toks.endrecord:
                        {
                            cur.ParseDate = DateTime.Now;
                            if (cur.isValid)
                            {
                                db.addindex(cur.Email, cur);
                                cur = new RegisterDB();
                            }
                            else
                            {
                                if (d != null)
                                {
                                    var toks = tokh.ToString<register_toks>(tokh.GetTokensBetween<register_toks>(final, startrecidx, i));
                                    d("error between: " + startrecidx + "-" + i + " data: " + toks);
                                }
                                startrecidx = i + 1;
                            }
                        }
                        break;
                        
                }

            }
            return db;
        }

        public static List<GenericTokenDefinition<register_toks>> GetDefs()
        {
            List<GenericTokenDefinition<register_toks>> defs = new List<GenericTokenDefinition<register_toks>>();
            defs.Add(tokh.getdef<register_toks>(@"[']registration_program[']", register_toks.key_registration_program));
            defs.Add(tokh.getdef<register_toks>(@"[']reg_firstname[']", register_toks.key_reg_firstname));
            defs.Add(tokh.getdef<register_toks>(@"[']reg_lastname[']", register_toks.key_reg_lastname));
            defs.Add(tokh.getdef<register_toks>(@"[']reg_email[']", register_toks.key_reg_email));
            defs.Add(tokh.getdef<register_toks>(@"[']reg_mid[']", register_toks.key_reg_mid));
            defs.Add(tokh.getdef<register_toks>(@"[']reg_code[']", register_toks.key_reg_code));

            defs.Add(tokh.getdef<register_toks>(@"[@]?([""'])(?:\\\1|.)*?\1", register_toks.quotedstring));

            defs.Add(tokh.getdef<register_toks>(@"[;]", register_toks.endrecord));
            defs.Add(tokh.getdef<register_toks>(@"[{]", register_toks.startdata));
            defs.Add(tokh.getdef<register_toks>(@"[}]", register_toks.enddata));
            defs.Add(tokh.getdef<register_toks>(@"[=][>]", register_toks.keyvaluepair));
            defs.Add(tokh.getdef<register_toks>(@"[=]", register_toks.assign));
            defs.Add(tokh.getdef<register_toks>(@"[$]VAR1", register_toks.dummyvar));

            defs.Add(tokh.getdef<register_toks>(@"[*<>\?_A-Za-z->!][_0-9A-Za-z->!]*", register_toks.symbol));
            
            defs.Add(tokh.getdef<register_toks>(@"\s+", register_toks.space));
            defs.Add(tokh.getdef<register_toks>(@"[,]", register_toks.comma));
            defs.Add(tokh.getdef<register_toks>(@"\S*", register_toks.catchall));
            


            return defs;
        }
    }

    public enum register_toks
    {
        // common
        none,
        space,
        symbol,
        catchall,
        statement,
        quotedstring,
        newline,
        comma,

        // custom register db stuff
        endrecord,
        assign,
        dummyvar,
        startdata,
        enddata,
        keyvaluepair,
        // keys
        key_registration_program,
        key_reg_firstname,
        key_reg_lastname,
        key_reg_email,
        key_reg_mid,
        key_reg_code,

    }
}
