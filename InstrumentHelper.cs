using System;
using System.Collections.Generic;
using System.Linq;
using Sanford.Multimedia.Midi;


namespace TinyAdiago
{
    public class InstrumentHelper
    {
        public static ChannelMessage Parse<I>(TokenParsing.GenericToken<MusicToken> tok, I defaultinstrument, ref string name, int midichan = 0) where I : struct, IConvertible { return (tok == null) || !tok.isValid ? Parse<I>(string.Empty, defaultinstrument, ref name) : Parse<I>(tok.data,defaultinstrument, ref name,midichan); }
        public static ChannelMessage Parse<I>(string instrument , I defaultinstrument, ref string name, int midichan = 0) where I : struct, IConvertible
        {
            name = string.Empty;
            if (string.IsNullOrWhiteSpace(instrument))
                return GetNew<I>(defaultinstrument, midichan);
            InstrumentGuide i1;
            InstrumentsGeneral i2;
            if (Enum.TryParse<InstrumentGuide>(instrument, true, out i1))
            {
                name = instrument;
                return GetNew(i1, midichan);
            }
            else if (Enum.TryParse<InstrumentsGeneral>(instrument, true, out i2))
            {
                name = instrument;
                return GetNew(i2, midichan);
            }
            name = defaultinstrument.ToString();
            return GetNew<I>(defaultinstrument,midichan);
        }
        public static ChannelMessage GetNew<I>(I instrument, int channel = 0) where I : struct, IConvertible
        {
            var pcnum = Convert.ToInt32(instrument);
            return GetNew(pcnum,channel);
        }
        public static ChannelMessage GetNew(int program_change_number, int channel = 0)
        {
            return new ChannelMessage(ChannelCommand.ProgramChange, channel, program_change_number);
        }

        /// <summary>
        /// get all enums of a given type
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TEnum[] getenums<TEnum>() where TEnum : struct, IConvertible
        {
            List<TEnum> vals = new List<TEnum>();
            var tmps = Enum.GetValues(typeof(TEnum));
            foreach (var tmp in tmps)
            {
                var itmp = (int)tmp;
                if (itmp > 0)
                    vals.Add((TEnum)tmp);
            }
            return vals.ToArray();
        }
    }
}
