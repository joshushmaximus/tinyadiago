using System;
using System.Collections.Generic;
using System.Linq;
using TokenParsing;

namespace TinyAdiago
{
    public class MusicTokenDefs
    {
        #region song -> music token definitions

        public static GenericTokenDefinition<MusicToken>[] Defs(DebugDelegate d)
        {
            var instdefs = GetInstrumentDefs(d, true, true);
            var ignorecase = true;
            var tonedefs = GenericTokenHelper.getdefs<MusicToken>(
                Tuple.Create(@"//.*", MusicToken.Comment, ignorecase),
                Tuple.Create(tokh.word("x[0-9]+"), MusicToken.Repeater, ignorecase),
                Tuple.Create(@"BPM\s*[0-9]{1,3}", MusicToken.BPM, ignorecase),
                Tuple.Create(@"KEY\s*[0-9]{1,2}", MusicToken.ScoreKey, ignorecase),

                Tuple.Create(tokh.word(@"qr|rq"), MusicToken.QuarterNoteRest, false),
                Tuple.Create(tokh.word(@"wr|rw"), MusicToken.WholeNoteRest, false),
                Tuple.Create(tokh.word(@"hr|rh"), MusicToken.HalfNoteRest, false),
                Tuple.Create(@"r", MusicToken.Rest, false),

                Tuple.Create(@"[0-9][A-G][dDmMfFsS#]?", MusicToken.ChordScale, false),
                Tuple.Create(@"[A-G][dDmMfFsS#]?", MusicToken.Chord, false),
                Tuple.Create(@"[0-9][a-g][fFsS#]?", MusicToken.NoteScale, false),
                Tuple.Create(@"[a-g][fFsS#]?", MusicToken.Note, false),
                Tuple.Create(@"w", MusicToken.WholeNote, false),
                Tuple.Create(@"h", MusicToken.HalfNote, false),
                Tuple.Create(@"q", MusicToken.QuarterNote, false),

                Tuple.Create(@"[0]?[.]?\d+", MusicToken.Number, ignorecase),
                Tuple.Create(@"\+", MusicToken.MakeChord, ignorecase),
                Tuple.Create(tokh.word(@"\."), MusicToken.QuarterNoteRest, false),
                Tuple.Create(tokh.word("track"),MusicToken.Track,ignorecase),
                Tuple.Create(@"\s+", MusicToken.WhiteSpace, ignorecase),
                Tuple.Create(@",", MusicToken.WhiteSpace, ignorecase),
                Tuple.Create(@"\S*", MusicToken.CatchAll, ignorecase)

                );
            instdefs.AddRange(tonedefs);

            return instdefs.ToArray();
        }

        #endregion

        #region instrument token defs

        public static List<GenericTokenDefinition<MusicToken>> GetInstrumentDefs(DebugDelegate d, bool includeguide = true, bool includegeneral = true)
        {
            List<GenericTokenDefinition<MusicToken>> insttoks = new List<GenericTokenDefinition<MusicToken>>();
            if (includeguide)
            {
                foreach (var inst in InstrumentHelper.getenums<InstrumentGuide>().Select(i => i.ToString()))
                {
                    var exactinst = inst; // inst.ToUpperInvariant();
                    var tokdef = GenericTokenHelper.getdef<MusicToken>(exactinst, MusicToken.Instrument);
                    insttoks.Add(tokdef);
                }
            }
            if (includegeneral)
            {
                foreach (var inst in InstrumentHelper.getenums<InstrumentsGeneral>().Select(i => i.ToString()))
                {
                    var exactinst = inst; // inst.ToUpperInvariant();
                    var tokdef = GenericTokenHelper.getdef<MusicToken>(exactinst, MusicToken.Instrument);
                    insttoks.Add(tokdef);
                }
            }
            return insttoks;
        }

        #endregion
    }
}
