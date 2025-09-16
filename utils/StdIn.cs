// Shameless GPT translation of https://algs4.cs.princeton.edu/code/edu/princeton/cs/algs4/StdIn.java.html

namespace utils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

public static partial class StdIn
{
    private static readonly CultureInfo Locale = CultureInfo.InvariantCulture;
    private static readonly Regex WhitespacePattern = WhitespaceRegex();

    private static readonly TextReader _reader = Console.In;
    private static readonly Queue<string> _tokenBuffer = new();

    static StdIn()
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
    }

    private static void FillBuffer()
    {
        while (_tokenBuffer.Count == 0)
        {
            var line = _reader.ReadLine();
            if (line == null) return;
            foreach (var token in WhitespacePattern.Split(line))
            {
                if (!string.IsNullOrEmpty(token))
                    _tokenBuffer.Enqueue(token);
            }
        }
    }

    public static bool IsEmpty()
    {
        FillBuffer();
        return _tokenBuffer.Count == 0;
    }

    public static bool HasNextLine() => _reader.Peek() != -1;

    public static bool HasNextChar() => _reader.Peek() != -1;

    public static string? ReadLine() => _reader.ReadLine();

    public static char ReadChar()
    {
        var c = _reader.Read();
        if (c == -1)
            throw new InvalidOperationException("No more characters available");
        return (char)c;
    }

    public static string ReadAll()
    {
        return _reader.ReadToEnd();
    }

    public static string ReadString()
    {
        FillBuffer();
        if (_tokenBuffer.Count == 0)
            throw new InvalidOperationException("No more tokens available");
        return _tokenBuffer.Dequeue();
    }

    public static int ReadInt() => int.Parse(ReadString(), Locale);
    public static double ReadDouble() => double.Parse(ReadString(), Locale);
    public static float ReadFloat() => float.Parse(ReadString(), Locale);
    public static long ReadLong() => long.Parse(ReadString(), Locale);
    public static short ReadShort() => short.Parse(ReadString(), Locale);
    public static byte ReadByte() => byte.Parse(ReadString(), Locale);

    public static bool ReadBoolean()
    {
        string token = ReadString();
        if (token.Equals("true", StringComparison.OrdinalIgnoreCase) || token == "1")
            return true;
        if (token.Equals("false", StringComparison.OrdinalIgnoreCase) || token == "0")
            return false;
        throw new InvalidOperationException($"Invalid boolean token: {token}");
    }

    public static string[] ReadAllStrings()
    {
        return WhitespacePattern.Split(ReadAll(), int.MaxValue, 0);
    }

    public static string[] ReadAllLines()
    {
        var lines = new List<string>();
        string? line;
        while ((line = _reader.ReadLine()) != null)
            lines.Add(line);
        return lines.ToArray();
    }

    public static int[] ReadAllInts()
    {
        var tokens = ReadAllStrings();
        var vals = new int[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
            vals[i] = int.Parse(tokens[i], Locale);
        return vals;
    }

    public static long[] ReadAllLongs()
    {
        var tokens = ReadAllStrings();
        var vals = new long[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
            vals[i] = long.Parse(tokens[i], Locale);
        return vals;
    }

    public static double[] ReadAllDoubles()
    {
        var tokens = ReadAllStrings();
        var vals = new double[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
            vals[i] = double.Parse(tokens[i], Locale);
        return vals;
    }

    [Obsolete("Use ReadAllInts() instead.")]
    public static int[] ReadInts() => ReadAllInts();

    [Obsolete("Use ReadAllDoubles() instead.")]
    public static double[] ReadDoubles() => ReadAllDoubles();

    [Obsolete("Use ReadAllStrings() instead.")]
    public static string[] ReadStrings() => ReadAllStrings();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
