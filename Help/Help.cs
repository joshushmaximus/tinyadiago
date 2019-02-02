using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyAdiago.Help
{
    public class Help
    {
        public static List<string> GetInstrumentNames()
        {
            List<string> inames = new List<string>();
            foreach (var inst in InstrumentHelper.getenums<InstrumentGuide>().Select(i => i.ToString()))
            {
                inames.Add(inst);
            }
            foreach (var inst in InstrumentHelper.getenums<InstrumentGuide>().Select(i => i.ToString()))
            {
                inames.Add(inst);
            }
            inames.Sort();
            return inames;


        }

        public static bool isHelp(string arg) { HelpType ht; return isHelp(arg, out ht); }
        public static bool isHelp(string arg, out HelpType ht)
        {
            ht = HelpType.None;
            if (Enum.TryParse<HelpType>(arg,true, out ht))
                return true;
            return false;
        }

        public static void GetHelp(DebugDelegate d, string helparg, bool showgeneral = false)
        {
            HelpType ht;
            if (!isHelp(helparg, out ht))
            {
                if (showgeneral)
                    GetHelp(d, HelpType.General);
                return;
            }
            GetHelp(d, ht);

                
        }
        public static void GetHelp(DebugDelegate d, HelpType type)
        {
            setd(d);
            const string cmd = "TinyAdiago";
            List<string> lines = new List<string>();
            
            switch (type)
            {
                case HelpType.General:
                    {
                        lines.Add(cmd + " Usage Help:");
                        List<string> opts = new List<string>();

                        foreach (var ht in InstrumentHelper.getenums<HelpType>())
                        {
                            if (ht == HelpType.General)
                                continue;
                            if (ht == HelpType.None)
                                continue;
                            opts.Add(ht.ToString());
                        }
                        lines.Add(cmd + " " + string.Join("|", opts));
                    }
                        
                    break;
                case HelpType.Instrument:
                    {
                        lines.Add(cmd + " Valid filename.txt instruments (instrument sound will depend onyour sound device and MIDI sound fonts):");
                        foreach (var ig in GetInstrumentNames().Take(10))
                            foreach (var i in GetInstrumentNames())
                                lines.Add(i);
                    }
                    break;
                case HelpType.Filename:
                    {
                        lines.Add(cmd + " <PathToFilename.txt");
                        lines.Add(cmd + "eg: '" + cmd + " AFS.txt'");
                    }
                    break;
                case HelpType.None:
                    break;
                default:
                    {
                        lines.Add(cmd + " " + type);
                    }
                    break;
            }
            foreach (var line in lines)
                debug(line);
        }

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
    }

    public enum HelpType
    {
        None,
        Hello,
        General,
        Filename,
        Instrument,
        Chopsticks,
        Labamba,
        Chord135,
        //Chord145,
        Scale,
        Chords,
        Duration,
        Tracks,
        Mulder,
        Key,


    }
}
