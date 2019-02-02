using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokenParsing;
using Sanford.Multimedia.Midi;

namespace TinyAdiago
{
    public static class Tokens2Song
    {
        public static string ToPrettySong(this List<GenericToken<MusicToken>> score, bool isshowcomments = true, string delim = @"\r\n")
        {
            var tmp = isshowcomments ? score : score.Where(n => n.type != MusicToken.Comment).ToList();
            var pretty = Toks2Msgs(null, tmp);
            List<string> chunks = new List<string>();
            foreach (var chunk in pretty.Take(4))
                chunks.Add(chunk);
            var song = string.Join(delim, chunks);
            return song;

        }

        static List<string> Toks2Msgs(DebugDelegate d, List<GenericToken<MusicToken>> score, int defaultkey = 5, int defaultms_note = Adiago.NOTE_DEFAULT_DUR_MS)
        {
            
            List<string> staff = new List<string>();
            // strip out undefined stuff
            //var score = GenericTokenHelper.Filter_Exclude(songstream, MusicToken.Comment, MusicToken.CatchAll, MusicToken.WhiteSpace);

            var last = new GenericToken<MusicToken>(MusicToken.None);
            int curkey = 5, tokidx = 0, chordcount = 0, notecount = 0;

            while (tokidx < score.Count)
            {
                var tok = score[tokidx];
                GenericToken<MusicToken> peek = (tokidx + 1) < score.Count ? score[tokidx + 1] : GenericTokenHelper.GetInvalid<MusicToken>();
                var ischord = (peek != null) && (peek.type == MusicToken.MakeChord);
                switch (tok.type)
                {
                    case MusicToken.Instrument:
                        {
                            string iname = string.Empty;
                            InstrumentHelper.Parse<InstrumentGuide>(tok, InstrumentGuide.AcousticGrandPiano, ref iname);
                            staff.Add("Instrument: " + iname);
                            tokidx++;
                        }
                        break;
                    case MusicToken.NoteScale:
                    case MusicToken.Note:
                    case MusicToken.QuarterNoteRest:
                        {
                            if (ischord)
                            {
                                // get entire chord
                                var lastnote = GenericTokenHelper.GetNextToken_Skip(score, tokidx, false, MusicToken.MakeChord, MusicToken.Note, MusicToken.NoteScale);
                                var lastnoteidx = lastnote == null ? score.Count - 1 : lastnote.id;
                                var chord = score.GetRange(tokidx, lastnoteidx - tokidx + 1).Where(n => (n.type == MusicToken.Note) || (n.type == MusicToken.NoteScale)).ToList();
                                List<string> info = new List<string>();
                                for (int cn = 0; cn < chord.Count; cn++)
                                {
                                    var note = chord[cn].data;
                                    if (cn == 0)
                                        note = note.ToUpper();
                                    info.Add(note);
                                }
                                staff.Add(string.Join(string.Empty, info));
                                tokidx += lastnoteidx + 1;
                            }
                            else
                            {
                                var note = (tok.type == MusicToken.QuarterNoteRest) ? "," : tok.data;
                                staff.Add(note);
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
            return staff;

        }

    }
}
