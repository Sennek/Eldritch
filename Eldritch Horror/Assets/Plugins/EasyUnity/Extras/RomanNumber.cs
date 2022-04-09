using NoxLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct RomanNumber : IEquatable<RomanNumber>, IByteListAppendable
{
    public const int MinValue = 0;
    public const int MaxValue = 3999;

    public static string[][] RomanNumerals { get; } = new string[][]
    {
            new string[] {"", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"},
            new string[] {"", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC"},
            new string[] {"", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM"},
            new string[] {"", "M", "MM", "MMM" },
    };

    public static Dictionary<char, ushort> RomanValues { get; } = new Dictionary<char, ushort>()
        {
            {'M', 1000},{'m', 1000},
            {'D', 500}, {'d', 500},
            {'C', 100}, {'c', 100},
            {'L', 50},  {'l', 50},
            {'X', 10},  {'x', 10},
            {'V', 5},   {'v', 5},
            {'I', 1},   {'i', 1},
        };

    private readonly short arabic;

    public RomanNumber(byte b) => arabic = b;
    public RomanNumber(short s) => arabic = s;
    public RomanNumber(ushort us) => arabic = (short)us;
    public RomanNumber(int i) => arabic = (short)i;
    public RomanNumber(uint ui) => arabic = (short)ui;
    public RomanNumber(long l) => arabic = (short)l;
    public RomanNumber(ulong ul) => arabic = (short)ul;
    public RomanNumber(float f) => arabic = (short)f;
    public RomanNumber(double d) => arabic = (short)d;
    public RomanNumber(decimal m) => arabic = (short)m;
    public RomanNumber(string s) => arabic = FromRoman(s);
    public RomanNumber(ByteArray ba) => arabic = ba.GetShort();

    private static string ToRoman(short number)
    {
        StringBuilder sb = new StringBuilder();

        char[] reverseArabic = number.ToString().Reverse().ToArray();
        int i = reverseArabic.Length;

        while (i-- > 0) sb.Append(RomanNumerals[i][int.Parse(reverseArabic[i].ToString())]);

        return sb.ToString();
    }

    private static short FromRoman(string roman)
    {
        int arabic = 0;
        char[] romanArray = roman.ToArray();

        for (int i = 0; i < romanArray.Length; i++)
        {
            if (RomanValues.TryGetValue(romanArray[i], out ushort current))
            {
                if (i < romanArray.Length - 1 && RomanValues.TryGetValue(romanArray[i + 1], out ushort next) && next > current)
                    arabic -= current;
                else arabic += current;
            }
            else return 0;
        }

        return (short)arabic;
    }

    public ByteList AppendBytes(ByteList bl) => bl.Append(arabic);

    public override string ToString() => ToRoman(arabic);


    public static explicit operator byte(RomanNumber rn) => (byte)rn.arabic;

    public static explicit operator short(RomanNumber rn) => rn.arabic;

    public static explicit operator ushort(RomanNumber rn) => (ushort)rn.arabic;

    public static explicit operator int(RomanNumber rn) => rn.arabic;

    public static explicit operator uint(RomanNumber rn) => (uint)rn.arabic;

    public static explicit operator long(RomanNumber rn) => rn.arabic;

    public static explicit operator ulong(RomanNumber rn) => (ulong)rn.arabic;

    public static explicit operator float(RomanNumber rn) => rn.arabic;

    public static explicit operator double(RomanNumber rn) => rn.arabic;

    public static explicit operator decimal(RomanNumber rn) => rn.arabic;


    public static explicit operator RomanNumber(byte b) => new RomanNumber(b);

    public static explicit operator RomanNumber(short b) => new RomanNumber(b);

    public static explicit operator RomanNumber(ushort b) => new RomanNumber(b);

    public static explicit operator RomanNumber(int b) => new RomanNumber(b);

    public static explicit operator RomanNumber(uint b) => new RomanNumber(b);

    public static explicit operator RomanNumber(long b) => new RomanNumber(b);

    public static explicit operator RomanNumber(ulong b) => new RomanNumber(b);

    public static explicit operator RomanNumber(float f) => new RomanNumber(f);

    public static explicit operator RomanNumber(double d) => new RomanNumber(d);

    public static explicit operator RomanNumber(decimal m) => new RomanNumber(m);

    public static explicit operator RomanNumber(string s) => new RomanNumber(s);

    public static RomanNumber operator +(RomanNumber r1, int i) => new RomanNumber(r1.arabic + i);
    public static RomanNumber operator -(RomanNumber r1, int i) => new RomanNumber(r1.arabic - i);
    public static RomanNumber operator *(RomanNumber r1, int i) => new RomanNumber(r1.arabic * i);
    public static RomanNumber operator /(RomanNumber r1, int i) => new RomanNumber(r1.arabic / i);

    public static RomanNumber operator +(RomanNumber r1, RomanNumber r2) => new RomanNumber(r1.arabic + r2.arabic);
    public static RomanNumber operator -(RomanNumber r1, RomanNumber r2) => new RomanNumber(r1.arabic - r2.arabic);
    public static RomanNumber operator *(RomanNumber r1, RomanNumber r2) => new RomanNumber(r1.arabic * r2.arabic);
    public static RomanNumber operator /(RomanNumber r1, RomanNumber r2) => new RomanNumber(r1.arabic / r2.arabic);

    public static bool operator ==(RomanNumber left, RomanNumber right) => left.Equals(right);
    public static bool operator !=(RomanNumber left, RomanNumber right) => !(left == right);
    public override bool Equals(object obj) => obj is RomanNumber number && Equals(number);
    public bool Equals(RomanNumber other) => arabic == other.arabic;
    public override int GetHashCode() => -465275023 + arabic.GetHashCode();
}