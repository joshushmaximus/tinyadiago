using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia.Midi.UI;

namespace TinyAdiago
{
    public delegate void DebugDelegate(string msg);
    public class Adiago
    {

        #region constants
        const string PROGRAM = "TinyAdiago";
        public const int NOTE_DEFAULT_DUR_MS = 500;

        public const int NOTE_DEFAULT_VEL = 127;

        #endregion

        #region main console interface

        public static void Main(string[] args)
        {
            setd(Console.WriteLine);

            debug(PROGRAM + " current directory: " + Environment.CurrentDirectory+" args: "+args.Length);
            int c = 0;
            bool dohelp = false;
            foreach (var arg in args)
            {
                if (Test==null)
                    Test = new OutputDevice(0);
                if (ParsePlay(Console.WriteLine,arg,ref c))
                {

                }
                else if (Premixed.PlayName(_d,Test,arg))
                {
                    c++;
                }
                else if (Help.Help.isHelp(arg))
                {
                    Help.Help.GetHelp(Console.WriteLine, arg);
                }
                else
                {
                    debug("No song data at: " + arg + ", no Premixed song named: " + arg);
                    dohelp = true;
                }
            }
            if ((args.Length==0) || dohelp)
            {
                Help.Help.GetHelp(Console.WriteLine, Help.HelpType.General);

            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                debug("Enter to continue...");
                Console.ReadLine();
            }

        }

        #endregion

        #region txt file score parsing

        public static bool ParsePlay(DebugDelegate d, string song_file) { int c = 0; return ParsePlay(d,song_file, ref c); }
        static bool ParsePlay(DebugDelegate d, string song_file, ref int c, bool issimpleparse = false)
        {
            setd(d);
            if (ParseMusicTokens.exists(ref song_file))
            {
                //var song = issimpleparse ? Parse.FromFile(d, song_file) : ParseMusicTokens.FromFile(d, song_file);
                var song = ParseMusicTokens.FromFile(d, song_file); 
                if (song.Count > 0)
                {
                    Test.PlayMsgs(true, song);
                    c++;
                }
                else
                {
                    debug("No song data obtainable from: " + song_file);
                }
                return true;
            }
            return false;
        }

        #endregion

        #region  boilerplate helpers

        public static OutputDevice Test = new OutputDevice(0);

        static void sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        static DebugDelegate _d = null;
        static void setd(DebugDelegate d)
        {
            if (_d != d)
                _d = d;
        }
        static void debug(string msg)
        {
            if ((_d != null) && (_d != debug))
                _d(msg);
        }


        #endregion

    }




    
}
