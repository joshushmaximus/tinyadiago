using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyAdiago
{
    public class note
    {
        public static string get(int tone) { Type t; Enum en;  return get(tone,out t, out en); }
        public static string get(int tone, out Type oct, out Enum enumnote)
        {
            var octint = (int) ((double)tone / 12);
            var nnote = tone % 12;
            enumnote = c4.c;
            oct = typeof(object);
            
            if (octint == 0)
                oct = typeof(cNeg);
            else if (octint == 1)
                oct = typeof(c0);
            else if (octint == 2)
                oct = typeof(c1);
            else if (octint == 3)
                oct = typeof(c2);
            else if (octint == 4)
                oct = typeof(c3);
            else if (octint == 5)
                oct = typeof(c4);
            else if (octint == 6)
                oct = typeof(c5);
            else if (octint == 7)
                oct = typeof(c6);
            else if (octint == 8)
                oct = typeof(c7);
            else if (octint == 9)
                oct = typeof(c8);
            else if (octint == 10)
                oct = typeof(c9);

            enumnote = (Enum)Enum.ToObject(oct, tone);
            return enumnote.ToString()+ "["+oct.Name+"]";
        }
    }
    

    


    public enum cNeg
    {
        c = 0,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c0
    {
        None = 0,
        c = 12,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c1
    {
        None = 0,
        c = 24,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c2
    {
        None = 0,
        c = 36,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }


    public enum c3
    {
        None = 0,
        c = 48,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    

    public enum c4
    {
        None = 0,
        c = 60,
        cs,
        d ,
        ds,
        e,
        f,
        fs,
        g,
        gs ,
        A,
        As,
        b,
    }

    public enum c5
    {
        None = 0,
        c = 72,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c6
    {
        None = 0,
        c = 84,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c7
    {
        None = 0,
        c = 96,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c8
    {
        None = 0,
        c = 108,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum c9
    {
        None = 0,
        c = 120,
        cs,
        d,
        ds,
        e,
        f,
        fs,
        g,
        gs,
        A,
        As,
        b,
    }

    public enum Cx
    {
        None = 0,
        C = 48, // c4?
        D = 50,
        E = 52,
        F,
        G,
        A,
        B,
    }

    public enum Cy
    {
        None = 0,
        C = 60,
        D = 62,
        E = 64,
        F,
        G,
        A,
        B,
    }
}
