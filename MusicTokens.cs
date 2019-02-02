using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokenParsing;

namespace TinyAdiago
{
    public enum MusicToken
    {
        None = 0,
        Comment,
        Repeater,
        ScoreKey,
        Instrument,
        BPM,
        ChordScale,
        Chord,
        NoteScale,
        Note,
        MakeChord,
        Number,
        Duration,
        Rest,
        WholeNoteRest,
        /// <summary>
        /// relative half note rest
        /// </summary>
        HalfNoteRest,
        /// <summary>
        /// relative quarter note rest
        /// </summary>
        QuarterNoteRest,
        //EigthNoteRest,
        //SixteenthNoteRest,
        WholeNote,
        /// <summary>
        /// relative half note
        /// </summary>
        HalfNote,
        /// <summary>
        /// relative quarter note
        /// </summary>
        QuarterNote,
        //EigthNote,
        //SixteenthNote,
        Track,
        VolumeInc,
        CatchAll,
        WhiteSpace,

    }

}
