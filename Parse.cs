using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;

namespace TinyAdiago
{
    public delegate void DebugDelegate(string msg);
    public class Parse
    {


        const int DEFCHANNEL = 0;
        const int DEFVELOCITY = 127;

        public static bool isVerboseDebugging = false;


        public static List<ChannelMessage> FromFile(DebugDelegate d, string path, int defaultkey = 5, int defaultnote_ms = Adiago.NOTE_DEFAULT_DUR_MS)
        {
            setd(d);
            var data = getfile(path, d);
            if (string.IsNullOrWhiteSpace(data))
            {
                debug("No song data obtained at: " + path);
                return new List<ChannelMessage>();
            }
            return Quick(d, data, defaultkey, defaultnote_ms);
        }

        public static List<ChannelMessage> Quick(DebugDelegate d, string data, int defaultkey = 5, int defaultms_note = Adiago.NOTE_DEFAULT_DUR_MS)
        {
            setd(d);
            debug("Parsing song: " + data);
            var lines = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<ChannelMessage> song = new List<ChannelMessage>();
            int n = 0, c = 0;
            foreach (var line in lines)
            {
                var tonegroups = line.Split(',');
                foreach (var tonegroup in tonegroups)
                {
                    if (tonegroup.Contains("+"))
                    {
                        c++;
                        
                        var chordtones = tonegroup.Split('+');
                        v("Chord#" + c + ": " + tonegroup + " starting chord...");
                        for (int i = 0; i<chordtones.Length; i++)
                        {
                            var ct = chordtones[i];
                            n++;
                            // start chord (add any beat/delay after fully defined)
                            if (i==chordtones.Length)
                                song.AddRange(GetNote(ct, defaultkey, defaultms_note, true, false));
                            else
                                song.AddRange(GetNote(ct, defaultkey, 0, true, false));
                        }
                        
                        v("Chord#" + c + ": " + tonegroup + " ending chord...");
                        foreach (var ct in chordtones)
                        {
                            // end chord
                            song.AddRange(GetNote(ct, defaultkey, 0, false, true));
                        }
                        
                    }
                    else
                    {
                        n++;
                        song.AddRange(GetNote(tonegroup, defaultkey, defaultms_note));
                    }
                }
            }
            debug("From " + data.Length.ToString("n0") + "bytes of short notes, obtained chords: " + c.ToString("n0") + " w/notes: " + n.ToString("n0"));
            return song;
        }

        static ChannelMessage[] GetNote(string rawnote, int key, int durms, bool on = true, bool off = true)
        {
            var issilent = string.IsNullOrWhiteSpace(rawnote);
            var tone = issilent ? 0 : GetNoteVal(rawnote, key);
            var vel = issilent ? 0 : DEFVELOCITY;
            List<ChannelMessage> note = new List<ChannelMessage>();
            if (tone < 0)
            {
                return note.ToArray();
            }
            if (on)
            {
                var start = new ChannelMessage(ChannelCommand.NoteOn, DEFCHANNEL, tone, vel);
                start.DeltaFrames = durms;
                note.Add(start);
            }
            if (off)
            {
                var end = new ChannelMessage(ChannelCommand.NoteOff, DEFCHANNEL, tone, 0);
                note.Add(end);
            }
            return note.ToArray();
        }

        public static int GetNoteVal(string note, int key)
        {
            int keytone = -1;
            try
            {
                var keytype = GetKeyType(key);
                var keyval = Enum.Parse(keytype, note, true);
                keytone = Convert.ToInt32(keyval);
                v("note: " + note + " [k:" + key + "] --> " + keyval + " [" + keytone + "]");
            }
            catch (Exception ex)
            {
                debug("Error reading note, will skip note: " + note + " key: "+key+", err:" + ex.Message + ex.StackTrace);
            }
            return keytone;
        }

        static Type GetKeyType(int key)
        {
            var nk = 0;
            if (key == nk++)
                return typeof(cNeg);
            else if (key == nk++)
                return typeof(c0);
            else if (key == nk++)
                return typeof(c1);
            else if (key == nk++)
                return typeof(c2);
            else if (key == nk++)
                return typeof(c3);
            else if (key == nk++)
                return typeof(c4);
            else if (key == nk++)
                return typeof(c5);
            else if (key == nk++)
                return typeof(c6);
            else if (key == nk++)
                return typeof(c7);
            else if (key == nk++)
                return typeof(c8);
            else if (key == nk++)
                return typeof(c9);
            throw new Exception("Unsupported key: " + key + ", please implement support.");

        }

        public static bool exists(ref string fn)
        {
            if (System.IO.File.Exists(fn))
                return true;
            else if (System.IO.File.Exists(@".\Songs\"+fn))
            {
                fn = System.IO.Path.GetFullPath(@".\Songs\" + fn);
                return true;
            }
            return false;
        }


        public static string getfile(string fn, DebugDelegate d)
        {
            setd(d);
            if (!System.IO.File.Exists(fn))
            {
                debug("No file at path: " + fn);
                return string.Empty;
            }
            try
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(fn);
                string data = sr.ReadToEnd();
                try
                {
                    sr.Close();
                    sr.Dispose();
                    sr = null;
                }
                catch { }
                return data;
            }
            catch (Exception ex)
            {
                debug("error reading: " + fn + " " + ex.Message + ex.StackTrace);
            }
            return string.Empty;

        }

        public static bool setfile(string fn, string data) { return setfile(fn, data, true, null); }
        public static bool setfile(string fn, string data, bool usewriteline, DebugDelegate d)
        {
            setd(d);
            try
            {
                System.IO.StreamWriter sr = new System.IO.StreamWriter(fn, false);
                if (usewriteline)
                    sr.WriteLine(data);
                else
                    sr.Write(data);
                sr.Close();
                sr.Dispose();
                sr = null;
                return true;
            }
            catch (Exception ex)
            {
                debug("error writing file: " + fn + " " + ex.Message + ex.StackTrace);
                return false;
            }

        }

        static void v(string msg)
        {
            if (isVerboseDebugging)
                debug(msg);
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
}
