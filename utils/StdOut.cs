// Shameless GPT translation of https://algs4.cs.princeton.edu/code/edu/princeton/cs/algs4/StdOut.java.html

namespace utils;

using System;
using System.Globalization;
using System.IO;
using System.Text;

public static class StdOut
{
    private static readonly Encoding Encoding = new UTF8Encoding(false);
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
    private static readonly TextWriter Out;

    static StdOut()
    {
        Console.OutputEncoding = Encoding;
        Out = Console.Out;
    }

    public static void Println() => Out.WriteLine();
    public static void Println(object? x) => Out.WriteLine(x);
    public static void Println(bool x) => Out.WriteLine(x);
    public static void Println(char x) => Out.WriteLine(x);
    public static void Println(double x) => Out.WriteLine(x.ToString(Culture));
    public static void Println(float x) => Out.WriteLine(x.ToString(Culture));
    public static void Println(int x) => Out.WriteLine(x);
    public static void Println(long x) => Out.WriteLine(x);
    public static void Println(short x) => Out.WriteLine(x);
    public static void Println(byte x) => Out.WriteLine(x);

    public static void Print() => Out.Flush();
    public static void Print(object? x) { Out.Write(x); Out.Flush(); }
    public static void Print(bool x) { Out.Write(x); Out.Flush(); }
    public static void Print(char x) { Out.Write(x); Out.Flush(); }
    public static void Print(double x) { Out.Write(x.ToString(Culture)); Out.Flush(); }
    public static void Print(float x) { Out.Write(x.ToString(Culture)); Out.Flush(); }
    public static void Print(int x) { Out.Write(x); Out.Flush(); }
    public static void Print(long x) { Out.Write(x); Out.Flush(); }
    public static void Print(short x) { Out.Write(x); Out.Flush(); }
    public static void Print(byte x) { Out.Write(x); Out.Flush(); }

    public static void Printf(string format, params object[] args)
    {
        Out.Write(string.Format(Culture, format, args));
        Out.Flush();
    }

    public static void Printf(CultureInfo culture, string format, params object[] args)
    {
        Out.Write(string.Format(culture, format, args));
        Out.Flush();
    }
}

