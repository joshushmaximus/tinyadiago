using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using TokenParsing;
using Prac.Core;

namespace TinyAdiago
{
    
    public class ParseMusicTokens
    {

        #region constants
        const string PROGRAM = "AdiagoTokens";
        //const int DEFCHANNEL = 0;
        const int DEFVELOCITY = 127;

        public static bool isVerboseDebugging = true;

        #endregion

        #region parse api

        public static List<ChannelMessage> Quick(DebugDelegate d, string data, string name = PROGRAM, int defaultkey = 5, int defaultms_note = Adiago.NOTE_DEFAULT_DUR_MS)
        {
            setd(d);

            var stream = getstream(name, data);
            var msgs = Toks2Msgs(d, stream, defaultkey, defaultms_note);
            return msgs;
        }

        static List<GenericToken<MusicToken>> getstream(string tag, string data)
        {
            var defs = MusicTokenDefs.Defs(debug);
            var stream = GenericTokenHelper.GetStream<MusicToken>(tag, data, defs, debug);
            return stream;
        }

        #endregion

        #region music tokens to MIDI conversion

        static List<ChannelMessage> Toks2Msgs(DebugDelegate d, List<GenericToken<MusicToken>> score, int defaultkey = 5, int defaultms_qtrnote = Adiago.NOTE_DEFAULT_DUR_MS)
        {
            StringBuilder songinfo = new StringBuilder(score.Count * 10);
            List<ChannelMessage> msgs = new List<ChannelMessage>();
            // strip out undefined stuff
            //var score = GenericTokenHelper.Filter_Exclude(songstream, MusicToken.Comment, MusicToken.CatchAll, MusicToken.WhiteSpace);

            var last = new GenericToken<MusicToken>(MusicToken.None);
            int curkey = 5, tokidx = 0, chordcount = 0, notecount = 0, track = 0, vel = Adiago.NOTE_DEFAULT_VEL;

            while (tokidx < score.Count)
            {
                var tok = score[tokidx];
                GenericToken<MusicToken> peek = (tokidx + 1) < score.Count ? score[tokidx + 1] : GenericTokenHelper.GetInvalid<MusicToken>();
                var ismakechord = (peek != null) && (peek.type == MusicToken.MakeChord);
                switch (tok.type)
                {
                    case MusicToken.Track:
                        {
                            track++;
                            tokidx++;
                            songinfo.AppendLine("Track" + track.ToString("n0")+"-->");
                        }
                        break;
                    case MusicToken.BPM:
                        {
                            var bpmdata = Util.rxr(tok.data, @"BPM\s*", string.Empty, false);
                            int tmpbpm;
                            if (int.TryParse(bpmdata, out tmpbpm))
                            {
                                songinfo.AppendLine("BPM=" + tmpbpm.ToString("F0"));
                                defaultms_qtrnote = getqtrnote_duration(tmpbpm);
                            }
                            else
                            {
                                debug(tok.ToString() + " Unknown BPM specification, will use existing default BPM");
                            }
                            tokidx++;
                        }
                        break;
                    case MusicToken.Instrument:
                        {
                            string iname = string.Empty;
                            msgs.Add(InstrumentHelper.Parse<InstrumentGuide>(tok, InstrumentGuide.AcousticGrandPiano, ref iname,track));
                            songinfo.AppendLine("Instrument: " + iname);
                            tokidx++;
                        }
                        break;
                    case MusicToken.ScoreKey:
                        {
                            var keydata = Util.rxr(tok.data, @"KEY\s*", string.Empty, false);
                            int tmpkey;
                            if (int.TryParse(keydata, out tmpkey) && (tmpkey>=0) && (tmpkey<=10))
                            {
                                songinfo.AppendLine("Key=" + tmpkey.ToString("F0"));
                                defaultkey = tmpkey;
                            }
                            else
                            {
                                debug(tok.ToString() + " Unknown key spec, using default key.");
                            }
                            tokidx++;


                        }
                        break;
                    case MusicToken.Chord:
                    case MusicToken.ChordScale:
                        {
                            // parse chord to music instructions
                            var notes = MusicChord2MusicNotes(tok, ref defaultkey, defaultms_qtrnote, ref songinfo);
                            // get midi instructions
                            StringBuilder ignore = new StringBuilder();
                            msgs.AddRange(GetChord(notes, ref defaultkey, defaultms_qtrnote, track, vel, ref ignore, ref notecount));
                            var cinfo = tok.data[0].ToString().ToUpperInvariant();
                            if (tok.data.Length > 1)
                                cinfo += tok.data.Substring(1, tok.data.Length - 2).ToLowerInvariant();
                            songinfo.Append(cinfo+ " ");
                            // next token
                            tokidx++;
                        }
                        break;
                    case MusicToken.WholeNoteRest:
                    case MusicToken.HalfNoteRest:
                    case MusicToken.QuarterNoteRest:
                        {
                            tok.data = string.Empty;
                            var dur = getnote_duration(defaultms_qtrnote, tok, peek);
                            msgs.AddRange(GetNote(tok, ref defaultkey, dur, track, vel, ref songinfo, true, true));
                            tokidx++;
                        }
                        break;
                    case MusicToken.NoteScale:
                    case MusicToken.Note:
                    
                        {
                            if (ismakechord)
                            {
                                // get entire chord
                                var chord = GetChordNotes(ref tokidx, ref score);
                                msgs.AddRange(GetChord(chord, ref defaultkey, defaultms_qtrnote, track, vel, ref songinfo, ref notecount));
                                chordcount++;
                            }
                            else
                            {
                                notecount++;
                                var dur = getnote_duration(defaultms_qtrnote, tok, peek);
                                msgs.AddRange(GetOneNote(tok, peek, ref defaultkey, dur,ref songinfo, track, vel));
                                tokidx++;
                            }
                        }
                        break;
                    default:
                        tokidx++;
                        break;


                }

                last = tok;
            }


            v("Song: " + score[0].source + " (chords=" + chordcount.ToString("n0") + ",notes=" + notecount.ToString("n0") + "): " + songinfo.ToString());
            return msgs;
        }

        #endregion

        #region chord-fetching

        static List<GenericToken<MusicToken>> GetChordNotes(ref int tokidx, ref List<GenericToken<MusicToken>> score)
        {
            var lastnote = GenericTokenHelper.GetNextToken_Skip(score, tokidx, false,
    MusicToken.MakeChord, MusicToken.Note, MusicToken.NoteScale);
            var lastnoteidx = lastnote == null ? score.Count - 1 : lastnote.id;
            var endchordidx = lastnoteidx - tokidx + 1;
            var chord = score.GetRange(tokidx, endchordidx).Where(n => (n.type == MusicToken.Note) || (n.type == MusicToken.NoteScale)).ToList();
            
            tokidx = lastnoteidx + 1;
            return chord;

        }

        static List<GenericToken<MusicToken>> GetChordTok(GenericToken<MusicToken> chord, ref int key, int durms, ref StringBuilder song)
        {
            if (!chord.isValid || ((chord.type != MusicToken.Chord) && (chord.type != MusicToken.ChordScale)))
                return new List<GenericToken<MusicToken>>();
            ChordTokens ct = ChordTokens.None;
            if (Enum.TryParse<ChordTokens>(chord.data, true, out ct))
            {
                var gtc = new GenericToken<ChordTokens>(ct, ct.ToString());
                return GetChordToks(gtc, ref key, durms, ref song);
            }

            throw new Exception("Unknown chord: " + chord.ToString() + ", please implement this chord.");
        }

        /// <summary>
        /// convert chord tokens to musictokens
        /// </summary>
        /// <param name="chord"></param>
        /// <param name="key"></param>
        /// <param name="durms"></param>
        /// <param name="song"></param>
        /// <returns></returns>
        static List<GenericToken<MusicToken>> MusicChord2MusicNotes(GenericToken<MusicToken> tok, ref int key, int durms, ref StringBuilder song)
        {
            ChordTokens chordtoken = ChordTokens.None;
            if (!tok.isValid || ((tok.type != MusicToken.Chord) && (tok.type != MusicToken.ChordScale)))
                return new List<GenericToken<MusicToken>>();
            if (!Enum.TryParse<ChordTokens>(tok.data, true, out chordtoken))
                return new List<GenericToken<MusicToken>>();

            string[] notes = new string[0];
            switch (chordtoken)
            {
                case ChordTokens.A: notes = new string[] { "a", "cs" , "e" }; break;
                case ChordTokens.Am: notes = new string[] { "a","c","e"}; break;
                case ChordTokens.Af: notes = new string[] { "af","c","ef"}; break;
                case ChordTokens.B: notes = new string[] { "b","ds","fs"}; break;
                case ChordTokens.Bm: notes = new string[] { "b","d","fs"}; break;
                case ChordTokens.Bf: notes = new string[] { "bf","d","f"}; break;
                case ChordTokens.C: notes = new string[] { "c","e","g"}; break;
                case ChordTokens.Cm: notes = new string[] { "c","ef","g"}; break;
                case ChordTokens.Cf: notes = new string[] { "c","e","gf"}; break;
                case ChordTokens.D: notes = new string[] { "d","fs","a"}; break;
                case ChordTokens.Dm: notes = new string[] { "d","f","a"}; break;
                case ChordTokens.Df: notes = new string[] { "df","f","af"}; break;
                case ChordTokens.E: notes = new string[] { "e","gs","b"}; break;
                case ChordTokens.Em: notes = new string[] { "e","g","b"}; break;
                case ChordTokens.Ef: notes = new string[] { "ef","g","bf"}; break;
                case ChordTokens.F: notes = new string[] { "f","a","c"}; break;
                case ChordTokens.Fm: notes = new string[] { "f","af","c"}; break;
                case ChordTokens.Ff: notes = new string[] { "f","af","c"}; break;
                case ChordTokens.G: notes = new string[] { "g","b","d"}; break;
                case ChordTokens.Gm: notes = new string[] { "g","bf","d"}; break;
                case ChordTokens.Gf: notes = new string[] { "gf","bf","df"}; break;
            }
            List<GenericToken<MusicToken>> toknotes = new List<GenericToken<MusicToken>>();
            foreach (var note in notes)
                toknotes.Add(new GenericToken<MusicToken>(MusicToken.Note, note));
            return toknotes;

        }

        static List<GenericToken<MusicToken>> GetChordToks(GenericToken<ChordTokens> chord, ref int key, int durms, ref StringBuilder song)
        {
            var notes = string.Empty;
            switch (chord.type)
            {
                case ChordTokens.A: notes = "a+cs+e"; break;
                case ChordTokens.Am: notes = "a+c+e"; break;
                case ChordTokens.Af: notes = "af+c+ef"; break;
                case ChordTokens.B: notes = "b+ds+fs"; break;
                case ChordTokens.Bm: notes = "b+d+fs"; break;
                case ChordTokens.Bf: notes = "bf+d+f"; break;
                case ChordTokens.C: notes = "c+e+g"; break;
                case ChordTokens.Cm: notes = "c+ef+g"; break;
                case ChordTokens.Cf: notes = "cf+ef+gf"; break;
                case ChordTokens.D: notes = "d+fs+a"; break;
                case ChordTokens.Dm: notes = "d+f+a"; break;
                case ChordTokens.Df: notes = "df+f+af"; break;
                case ChordTokens.E: notes = "e+gs+b"; break;
                case ChordTokens.Em: notes = "e+g+b"; break;
                case ChordTokens.Ef: notes = "ef+g+bf"; break;
                case ChordTokens.F: notes = "f+a+c"; break;
                case ChordTokens.Fm: notes = "f+af+c"; break;
                case ChordTokens.Ff: notes = "ff+af+cf"; break;
                case ChordTokens.G: notes = "g+b+d"; break;
                case ChordTokens.Gm: notes = "g+bf+d"; break;
                case ChordTokens.Gf: notes = "gf+bf+df"; break;
            }
            var notetoks = getstream("chord: " + chord.type.ToString(), notes);
            return notetoks;

        }

        static ChannelMessage[] GetChord(List<GenericToken<MusicToken>> chord, ref int key, int durms, int track, int vel, ref StringBuilder songinfo, ref int notecount)
        {
            List<ChannelMessage> msgs = new List<ChannelMessage>();
            StringBuilder ignorenoteinfo = new StringBuilder();
            // start chord
            int velstep = 30;
            int cvel = vel - (velstep * (chord.Count));
            for (int cn = 0; cn < chord.Count; cn++)
            {
                var note = chord[cn];
                var islastnote = cn == chord.Count - 1;
                var chorddur = islastnote ? durms : 0;
                cvel += velstep;
                msgs.AddRange(GetChordNote(note, ref key, chorddur,track,cvel, ref ignorenoteinfo, true));
                notecount++;
                songinfo.Append(cn == 0 ? note.data.ToUpper() : note.data.ToLower());
                if (islastnote)
                    songinfo.Append(" ");
            }

            // end chord
            foreach (var note in chord)
            {
                msgs.AddRange(GetChordNote(note, ref key, 0,track,0, ref ignorenoteinfo, false));
            }
            return msgs.ToArray();

        }

        static ChannelMessage[] GetChordNote(GenericToken<MusicToken> note, ref int key, int durms, int track, int vel, ref StringBuilder song, bool start = true)
        {
            return GetNote(note, ref key, durms,track, vel, ref song, start, !start);
        }




        #endregion

        #region note-fetching

        static ChannelMessage[] GetOneNote(GenericToken<MusicToken> note, GenericToken<MusicToken> next, ref int key, int default_durms, ref StringBuilder song, int track = 0, int vel = Adiago.NOTE_DEFAULT_VEL)
        {
            return GetNote(note, ref key, default_durms, track, vel, ref song, true, true);
        }

        static ChannelMessage[] GetNote(GenericToken<MusicToken> note, ref int key, int note_durms, int track, int vel, ref StringBuilder song, bool on = true, bool off = true)
        {
            if (!note.isValid || note.isEmpty)
                return new ChannelMessage[0];
            var toneraw = note.data;
            var isscalechange = false;
            if (note.type == MusicToken.NoteScale)
            {
                var scaleraw = Util.rxm(note.data, @"[0-9][a-gA-G]{1,2}");
                var scaleonly = Util.rxr(scaleraw, @"([0-9]).*", "$1");
                toneraw = note.data.Replace(scaleonly, string.Empty);
                int tmpscale = key;
                if (int.TryParse(scaleonly, out tmpscale))
                    key = tmpscale;
                else
                    throw new Exception("Unable to read scale from: " + note);
                isscalechange = true;
            }
            var flatlesstone = AdjustNoteFlats(toneraw);
            var tone = GetNoteVal(flatlesstone, key, ref song, isscalechange);
            List<ChannelMessage> notemsg = new List<ChannelMessage>();
            if (tone < 0)
            {
                return notemsg.ToArray();
            }
            if (on)
            {
                var start = new ChannelMessage(ChannelCommand.NoteOn, track, tone, vel);
                start.DeltaFrames = note_durms;
                notemsg.Add(start);
            }
            if (off)
            {
                var end = new ChannelMessage(ChannelCommand.NoteOff, track, tone, 0);
                notemsg.Add(end);
            }
            return notemsg.ToArray();
        }




        public static int GetNoteVal(string note, int key, ref StringBuilder song, bool isscalechange = true)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                song.Append(" ");
                return 0;
            }
            int keytone = -1;
#if DEBUG
#else
            try
#endif
            {
                var keytype = GetKeyType(ref key);
                var keyval = Enum.Parse(keytype, note, true);
                keytone = Convert.ToInt32(keyval);

                song.Append(note.ToUpperInvariant() + (isscalechange ? key.ToString() : string.Empty) + " ");
                //v("note: " + note + " [k:" + key + "] --> " + keyval + " [" + keytone + "]");
            }
#if DEBUG
#else
            catch (Exception ex)
            {
                debug("Error reading note, will skip note: " + note + " key: " + key + ", err:" + ex.Message + ex.StackTrace);
            }
#endif
            return keytone;
        }

        static string AdjustNoteFlats(string tone)
        {
            /*
            tone = Util.rxr(tone, "Af", "Gs", false);
            tone = Util.rxr(tone, "Bf", "As", false);
            tone = Util.rxr(tone, "Df", "Cs", false);
            tone = Util.rxr(tone, "Ef", "Ds", false);
            tone = Util.rxr(tone, "Gf", "Fs", false);
            */
            bool isignorecase = false;
            tone = Util.rxr(tone, "af", "gs", !isignorecase);
            tone = Util.rxr(tone, "bf", "as", !isignorecase);
            tone = Util.rxr(tone, "cf", "bs", !isignorecase);
            tone = Util.rxr(tone, "df", "cs", !isignorecase);
            tone = Util.rxr(tone, "ef", "ds", !isignorecase);
            tone = Util.rxr(tone, "gf", "fs", !isignorecase);
            return tone;
        }

        static Type GetKeyType(ref int key)
        {
            var nk = 0;
            var nt = typeof(Nullable);
            if (key == nk++)
                nt = typeof(cNeg);
            else if (key == nk++)
                nt = typeof(c0);
            else if (key == nk++)
                nt = typeof(c1);
            else if (key == nk++)
                nt = typeof(c2);
            else if (key == nk++)
                nt = typeof(c3);
            else if (key == nk++)
                nt = typeof(c4);
            else if (key == nk++)
                nt = typeof(c5);
            else if (key == nk++)
                nt = typeof(c6);
            else if (key == nk++)
                nt = typeof(c7);
            else if (key == nk++)
                nt = typeof(c8);
            else if (key == nk++)
                nt = typeof(c9);
            else
                throw new Exception("Unsupported key: " + key + ", please implement support.");
            return nt;

        }

#endregion

#region helpers

        static int getnote_duration(int defaultqtrdur_ms, GenericToken<MusicToken> tok, GenericToken<MusicToken> next)
        {
            if (tok.type == MusicToken.WhiteSpace)
                throw new Exception();
            var default_notefrac = 1.0;
            double dur = defaultqtrdur_ms;
            isDurAdjTok(tok, out default_notefrac);
            dur *= default_notefrac;
            // check next token override
            var duradj = 0.0;
            if ((next!=null) && next.isValid && isDurAdjTok(next, out duradj))
            {
                dur *= duradj;
            }
            return (int)dur;

        }

        static bool isDurAdjTok(GenericToken<MusicToken> tok, out double durfrac)
        {
            const double deffrac = 1.0;
            var default_notefrac = deffrac;
            var isdur = false;
            switch (tok.type)
            {
                case MusicToken.WholeNote: default_notefrac = 4; isdur = true; break;
                case MusicToken.WholeNoteRest: default_notefrac = 4; isdur = false; break;
                case MusicToken.HalfNote: default_notefrac = .5; isdur = true; break;
                case MusicToken.HalfNoteRest: default_notefrac = 2; isdur = false; break;
                case MusicToken.Rest: default_notefrac = 4; isdur = false; break;
                case MusicToken.Note: isdur = false; break;
                case MusicToken.QuarterNote: default_notefrac = .25; isdur = true; break;
                case MusicToken.QuarterNoteRest: default_notefrac = 1; isdur = false; break;
                case MusicToken.Number:
                    {
                        double tmp;
                        if (double.TryParse(tok.data, out tmp))
                        {
                            default_notefrac = tmp;
                            isdur = true;
                        }
                        else
                            throw new Exception("Unable to parse note duration: " + tok);
                    }
                    break;
            }
            
            durfrac = default_notefrac;
            return isdur;
        }

        static int getqtrnote_duration(int bpm) { return 60000 / bpm; }

        static double GetNumber(GenericToken<MusicToken> tok, double default_num = 1, bool isthrowerror = true)
        {
            var num = default_num;
            if (tok.type != MusicToken.ScoreKey)
                return num;
            double tmp;
            if (double.TryParse(tok.data, out tmp))
            {
                num = tmp;
            }
            else if (isthrowerror)
                throw new Exception("Unable to parse note duration: " + tok);
            return num;
        }

        #endregion


        #region file/io


        public static List<ChannelMessage> FromFile(DebugDelegate d, string path)
        {
            setd(d);
            var data = getfile(path, d);
            if (string.IsNullOrWhiteSpace(data))
            {
                debug("No song data obtained at: " + path);
                return new List<ChannelMessage>();
            }
            var name = System.IO.Path.GetFileNameWithoutExtension(path) + "-" + Util.ToPCDate().ToString();
            return Quick(d, data, name);
        }

        public static bool exists(ref string fn)
        {
            if (System.IO.File.Exists(fn))
                return true;
            else if (System.IO.File.Exists(@".\Songs\" + fn))
            {
                fn = System.IO.Path.GetFullPath(@".\Songs\" + fn);
                return true;
            }
            return false;
        }


        public static string getfile(string fn, DebugDelegate d)
        {
            setd(d);
            if (!exists(ref fn))
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
            if ((_d == null) || (_d != d))
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

