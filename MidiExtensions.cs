using System;
using System.Collections.Generic;
using System.Linq;
using Sanford.Multimedia.Midi;


namespace TinyAdiago
{
    public static class MidiOutExts
    {
        public static bool isShowNotes = true;

        public static void PlayNoteSeq<T>(this OutputDevice dev, params T[] tones) where T : struct, IConvertible { PlayNoteSeq(dev, tones); }
        public static void PlayNoteSeq(this OutputDevice dev, params int[] tones) { PlayNoteSeq(dev, tones, new int[0], Adiago.NOTE_DEFAULT_DUR_MS); }
        public static void PlayNoteSeq<T>(this OutputDevice dev, T[] tones, int defaultdur = 500) where T : struct, IConvertible { PlayNoteSeq(dev, tones, new int[0], defaultdur); }
        public static void PlayNoteSeq(this OutputDevice dev, int[] tones, int defaultdur = 500) { PlayNoteSeq(dev, tones, new int[0], defaultdur); }
        public static void PlayNoteSeq<T>(this OutputDevice dev, T[] tones, int[] durations, int defaultdur = Adiago.NOTE_DEFAULT_DUR_MS)
            where T : struct, IConvertible
        {
            int[] realtones = new int[tones.Length];
            for (int i = 0; i < tones.Length; i++)
                realtones[i] = Convert.ToInt32(tones[i]);

            PlayNoteSeq(dev, realtones, durations, defaultdur);
        }
        public static void PlayNoteSeq(this OutputDevice dev, int[] tones, int[] durations, int defaultdur = Adiago.NOTE_DEFAULT_DUR_MS)
        {
            if (durations.Length == 0)
            {
                durations = new int[tones.Length];
                for (int i = 0; i < durations.Length; i++)
                    durations[i] = defaultdur;
            }
            for (int t = 0; t < tones.Length; t++)
                dev.PlayNote(tones[t], durations[t]);
            dev.stop();
        }
        public static void PlayChord<T>(this OutputDevice dev, params T[] chordnotes) where T : struct, IConvertible { PlayChord<T>(dev, Adiago.NOTE_DEFAULT_DUR_MS, chordnotes); }
        public static void PlayChord<T>(this OutputDevice dev, int duration, params T[] chordnotes) where T : struct, IConvertible
        {
            if (dev.IsDisposed)
            {
                dev = null;
                dev = new OutputDevice(0);
            }
            if (dev == null)
            {
                dev = new OutputDevice(0);
            }
            if (isShowNotes)
                Console.WriteLine(string.Join(",", chordnotes.Select(cn => cn.ToString()).ToArray()));
            foreach (var note in chordnotes)
            {
                var tone = Convert.ToInt32(note);
                dev.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, tone, 127));
            }
            sleep(duration);
            foreach (var note in chordnotes)
            {
                var tone = Convert.ToInt32(note);
                dev.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, tone, 0));
            }
            sleep(0);
        }
        public static void PlayNotes<T>(this OutputDevice dev, params T[] notes) where T : struct, IConvertible { PlayNotes<T>(dev, Adiago.NOTE_DEFAULT_DUR_MS, notes); }
        public static void PlayNotes<T>(this OutputDevice dev, int durationms, params T[] notes) where T : struct, IConvertible
        {
            foreach (var note in notes)
            {
                var tone = Convert.ToInt32(note);
                if (isShowNotes)
                    Console.WriteLine(note);
                PlayNote(dev, tone, durationms);
            }
        }
        public static void PlayNote(this OutputDevice dev, int tone, int durationms)
        {

            dev.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, tone, 127));
            sleep(durationms);

            dev.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, tone, 0));

        }

        public static void PlayParse(this OutputDevice dev, DebugDelegate d, params string[] songparts)
        {
            var song = string.Join(Environment.NewLine, songparts);
            var msgs = ParseMusicTokens.Quick(d, song, "TestParse");
            PlayMsgs(dev, msgs);
        }

        public static void PlayMsgs(this OutputDevice dev, List<ChannelMessage> msgs) { dev.PlayMsgs(true, msgs.ToArray()); }
        public static void PlayMsgs(this OutputDevice dev, ChannelMessage[] msgs) { dev.PlayMsgs(true, msgs); }
        public static void PlayMsgs(this OutputDevice dev, bool isautostop, List<ChannelMessage> msgs) { dev.PlayMsgs(isautostop, msgs.ToArray()); }
        public static void PlayMsgs(this OutputDevice dev, bool isautostop, params ChannelMessage[] msgs)
        {
            if (dev.IsDisposed)
            {
                dev = null;
                dev = new OutputDevice(0);
            }
            if (dev == null)
            {
                dev = new OutputDevice(0);
            }
            msgs = ArrangeTracks(msgs);
            foreach (var msg in msgs)
            {
                dev.Send(msg);
                if (msg.DeltaFrames > 0)
                    sleep(msg.DeltaFrames);
            }
            if (isautostop)
                dev.stop();
        }

        static ChannelMessage[] ArrangeTracks(params ChannelMessage[] msgs)
        {
            List<ChannelMessage> arranged = new List<ChannelMessage>();

            var idxs = GetTrackIdxs(msgs);
            if (idxs.Count < 2)
                return msgs;
            var nexttracktok = idxs.ToArray();

            // on every track, read messages until there is a duration
            int msgc = 0;
            while (msgc<msgs.Length)
            {
                for (int t = 0; t<idxs.Count; t++)
                {
                    var ttokidx = nexttracktok[t];
                    if (ttokidx >= msgs.Length)
                        break;
                    // skip if this track is complete
                    if ((t+1<idxs.Count) && (ttokidx >= idxs[t+1]))
                        continue;
                    var ttok = msgs[ttokidx];
                    arranged.Add(ttok);
                    nexttracktok[t]++;
                    if (++msgc >= msgs.Length)
                        break;
                }
            }

            return arranged.ToArray();
        }

        static List<int> GetTrackIdxs(ChannelMessage[] msgs)
        {
            // index every device change
            var idxs = new List<int>();
            var trackid = -1;
            for (int i = 0; i < msgs.Length; i++)
            {
                if (trackid != msgs[i].MidiChannel)
                {
                    idxs.Add(i);
                    trackid = msgs[i].MidiChannel;
                }
            }
            return idxs;
        }

        /// <summary>
        /// set an instrument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dev"></param>
        /// <param name="instrument"></param>
        public static void SetInstrument<T>(this OutputDevice dev, T instrument) where T : struct, IConvertible
        {
            dev.Send(InstrumentHelper.GetNew<T>(instrument));
        }

        /// <summary>
        /// change to instrument
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="program_change_number"></param>
        public static void SetInstrument(this OutputDevice dev, int program_change_number = 1)
        {
            //via: https://www.midi.org/specifications/item/gm-level-1-sound-set
            dev.Send(InstrumentHelper.GetNew(program_change_number));
        }



        public static void stop(this OutputDevice dev)
        {
            if (dev != null)
            {
                try
                {
                    dev.Reset();
                }
                catch { }
                if (dev != null)
                {
                    dev.Dispose();
                    dev = null;
                }
            }
            else
            {
                dev = null;
                try
                {
                    dev = new OutputDevice(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error creating new midi device0: " + ex.Message + ex.StackTrace);
                }
            }

        }
        static void sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
    }

}
