using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;

namespace TinyAdiago
{
    public delegate void PremixDel(DebugDelegate d, OutputDevice dev);
    public class Premixed
    {
        public static bool PlayName(DebugDelegate d, OutputDevice dev, params string[] names)
        {
            bool isfound = true;
            PremixDel mix = null;
            foreach (var name in names)
            {
                var un = name.ToLower();
                if (un.Contains("chop") || un.Contains("stick"))
                    mix = Chopsticks;
                else if (un.Contains("135"))
                    mix = TriadChord;
                else if (un.Contains("parse") || un.Contains("test"))
                    mix = TestParse;
                else if (un.Contains("bamba"))
                    mix = LaBamBa;
                else if (un.Contains("song"))
                    mix = ElliGouldingYourSong;
                else if (un.Contains("chord"))
                    mix = TestChord;
                else if (un.Contains("dur"))
                    mix = TestDurations;
                else if (un.Contains("track"))
                    mix = TestTracks;
                else if (un.Contains("mulder"))
                    mix = TrackDur;
                else if (un.Contains("key"))
                    mix = TestKey;
                else if (un.Contains("hello"))
                    mix = HelloMix;

                else
                    isfound = false;
            }
            if (isfound)
            {
                
                PlayMix(d, dev, mix);

            }
            return isfound;
        }

        static void PlayMix(DebugDelegate d, OutputDevice dev, PremixDel mix)
        {
            setd(d);
            debug("Playing pre-mixed song/test: " + mix.Method.Name);
#if DEBUG
#else
            try
#endif
            {
                mix(d, dev);
                try
                {
                    dev.stop();
                }
                catch { }

            }
#if DEBUG
#else
            catch (Exception ex)
            {
                debug("Error playing: " + mix.Method.Name + ", err: " + ex.Message + ex.StackTrace);
            }
#endif
        }

        public static void HelloMix(DebugDelegate d, OutputDevice dev)
        {
            // play every mix
            PremixDel[] hello_mixes = new PremixDel[]
            {
                TestParse,
                TriadChord,
                Chopsticks,
                TestChord,
                TestDurations,
                TestTracks,
                TrackDur,
                TestKey,
            };
            foreach (var mix in hello_mixes)
                PlayMix(d, dev, mix);
        }

        public static void TestDurations(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            // note durations
            dev.PlayParse(d, @"cw cw wr wr c1 c1 ch ch hr hr cq cq cq cq qr qr qr qr");

        }

        public static void TrackDur(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            // note durations
            dev.PlayParse(d, @"strings cw cw wr wr c1 c1 ch ch hr hr cq cq cq cq qr qr qr qr "+
                "track bass cw cw wr wr c1 c1 ch ch hr hr cq cq cq cq qr qr qr qr");

        }

        public static void TestTracks(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            
            dev.PlayParse(d,
                @"strings F,Fm,Ff,G,Gm,Gf,A,Am,Af,B,Bm,Bf,C,Cm,Cf,D,Dm,Df,E,Em,Ef," +
                "track bass F,Fm,Ff,G,Gm,Gf,A,Am,Af,B,Bm,Bf,C,Cm,Cf,D,Dm,Df,E,Em,Ef,"
                );

        }

        public static void TestKey(DebugDelegate d, OutputDevice dev)
        {
            setd(d);

            dev.PlayParse(d,
                @"strings F,Fm,Ff,G,Gm,Gf,A,Am,Af,B,Bm,Bf,C,Cm,Cf,D,Dm,Df,E,Em,Ef," +
                "track key3 bass F,Fm,Ff,G,Gm,Gf,A,Am,Af,B,Bm,Bf,C,Cm,Cf,D,Dm,Df,E,Em,Ef,"
                );

        }



        public static void TestChord(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            dev.PlayParse(d, @"F,Fm,Ff,G,Gm,Gf,A,Am,Af,B,Bm,Bf,C,Cm,Cf,D,Dm,Df,E,Em,Ef,");

        }

        public static void TestParse(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            debug("Note parse test...");
            dev.PlayParse(d,"//hello world","c,d,e,f,g,a,b,6c");
            
            debug("Chord parse test...");
            dev.stop();
            //dev.PlayChord<c5>(c5.c, c5.e, c5.g);
            dev.PlayParse(d, "bpm30"," c+e+g");
            debug("All simple parse tests ok.");
            //Console.ReadLine();
        }

        public static void ElliGouldingYourSong(DebugDelegate d, OutputDevice dev)
        {
            // https://www.youtube.com/watch?v=D9AFMVMl9qE
            debug("Ellie Goulding - Your song (https://www.8notes.com/school/riffs/piano/ellie_goulding.asp)");
            // this song: https://www.8notes.com/school/riffs/piano/Goulding1.gif    plus this key: http://www.music-mind.com/Music/Srm0039.GIF gives:
            const string SONGNOTES = @"gm:B+G,bf:B+";
            dev.PlayParse(d,SONGNOTES);


        }

        public static void LaBamBa(DebugDelegate d, OutputDevice dev)
        {
            debug("La Bamba : http://www.dcmusicstore.com/media/catalog/product/cache/1/image/800x800/9df78eab33525d08d6e5fb8d27136e95/3/_/3_chord_songs_for_ukulele_songbook_ex.jpg");
            // using: http://www.music-mind.com/Music/Srm0039.GIF
            const string SONG = @"F,F,F,F,F+E,E,C,,,
F,F,F,F,F+E,E,C,,C,C,C,D+D,B,D,D,F,E,D,E,C,,F,F,F,F,F,E,C,C,C,C,C,C";
            dev.PlayParse(d,SONG);

        }


        public static void Chopsticks(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            dev.SetInstrument<InstrumentGuide>(InstrumentGuide.HonkyTonkPiano);
            dev.PlayNotes<c4>(300, c4.c, c4.c);
            dev.PlayNotes<c4>(1000, c4.c);
            dev.PlayNotes<c4>(300, c4.c);
            dev.PlayNotes<c3>(300, c3.b, c3.A, c3.b);
            dev.PlayNotes<c4>(300, c4.c, c4.d, c4.e, c4.e);
            dev.PlayNotes<c4>(1000, c4.e);
            dev.PlayNotes<c4>(300, c4.d, c4.c, c4.d, c4.e, c4.f);
            dev.PlayNotes<c4>(1000, c4.g);
            dev.PlayNotes<c4>(1000, c4.c);

            dev.stop();
        }


        static void TriadChord(DebugDelegate d, OutputDevice dev)
        {
            setd(d);
            // via https://www.guitarlessonworld.com/lessons/chord-construction/
            // for any major chord: https://www.guitarlessonworld.com/resources/list-of-major-scale-notes/
            debug("Playing ala: https://www.guitarlessonworld.com/lessons/chord-construction/");
            dev.SetInstrument<InstrumentGuide>(InstrumentGuide.Banjo);
            //Test.PlayChord<c4>(c4.c, c4.e, c4.g);
            dev.PlayChord<c3>(c3.c, c3.e, c3.g);
            dev.stop();
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
