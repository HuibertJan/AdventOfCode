using AdventOfCode;

namespace AoC2015;

public class Day10 : DayBase
{
    public override string Part1()
    {
        return LookAndSayLength(40).ToString();
    }

    private static readonly string[] Elements =
    [
        "22", "13112221133211322112211213322112", "312211322212221121123222112",
        "111312211312113221133211322112211213322112", "1321132122211322212221121123222112",
        "3113112211322112211213322112", "111312212221121123222112", "132112211213322112",
        "31121123222112", "111213322112", "123222112", "3113322112", "1113222112", "1322112",
        "311311222112", "1113122112", "132112", "3112", "1112", "12", "3113112221133112",
        "11131221131112", "13211312", "31132", "111311222112", "13122112", "32112", "11133112",
        "131112", "312", "13221133122211332", "31131122211311122113222", "11131221131211322113322112",
        "13211321222113222112", "3113112211322112", "11131221222112", "1321122112", "3112112",
        "1112133", "12322211331222113112211", "1113122113322113111221131221", "13211322211312113211",
        "311322113212221", "132211331222113112211", "311311222113111221131221", "111312211312113211",
        "132113212221", "3113112211", "11131221", "13211", "3112221", "1322113312211",
        "311311222113111221", "11131221131211", "13211321", "311311", "11131", "1321133112",
        "31131112", "111312", "132", "311332", "1113222", "13221133112", "3113112221131112",
        "111312211312", "1321132", "311311222", "11131221133112", "1321131112", "311312", "11132",
        "13112221133211322112211213322113", "312211322212221121123222113",
        "111312211312113221133211322112211213322113", "1321132122211322212221121123222113",
        "3113112211322112211213322113", "111312212221121123222113", "132112211213322113",
        "31121123222113", "111213322113", "123222113", "3113322113", "1113222113", "1322113",
        "311311222113", "1113122113", "132113", "3113", "1113", "13", "3"
    ];

    private static readonly int[][] Decays =
    [
        [0], [71, 90, 0, 19, 2], [1], [31, 19, 2], [3], [4], [5], [6], [7], [8], [9], [60, 10], [11], [12],
        [66, 13], [14], [15], [16], [17], [18], [66, 90, 0, 19, 26], [20], [21], [22], [23, 13], [24], [25],
        [29, 26], [27], [28], [62, 19, 88, 0, 19, 29], [66, 30], [31, 10], [32], [33], [34], [35], [36],
        [37, 91], [38, 0, 19, 42], [67, 39], [40], [41], [62, 19, 42], [66, 43], [44], [45], [46], [47], [48],
        [60, 49], [62, 19, 50], [66, 51], [52], [53], [54], [55], [56, 0, 19, 26], [57], [58], [59],
        [60, 19, 29], [61], [62, 19, 26], [66, 63], [64], [65], [66, 60], [67, 19, 26], [68], [69], [70],
        [71, 90, 0, 19, 73], [72], [31, 19, 73], [74], [75], [76], [77], [78], [79], [80], [60, 81], [82],
        [83], [66, 84], [85], [86], [87], [88], [89], [90]
    ];

    private int LookAndSayLength(int steps)
    {
        Span<long> counts = stackalloc long[Elements.Length];
        Span<long> next = stackalloc long[Elements.Length];
        counts.Clear();

        var single = Array.IndexOf(Elements, Input);
        if (single >= 0)
            counts[single] = 1;
        else
            steps = SeedCounts(counts, steps);

        for (var step = 0; step < steps; step++)
        {
            next.Clear();
            for (var e = 0; e < counts.Length; e++)
            {
                if (counts[e] == 0)
                    continue;
                foreach (var d in Decays[e])
                    next[d] += counts[e];
            }
            next.CopyTo(counts);
        }

        long length = 0;
        for (var e = 0; e < counts.Length; e++)
            length += counts[e] * Elements[e].Length;
        return (int)length;
    }

    private int SeedCounts(Span<long> counts, int steps)
    {
        var start = Input;
        while (Decompose(start) is null && steps > 0)
        {
            start = Say(start);
            steps--;
        }
        foreach (var e in Decompose(start)!)
            counts[e]++;
        return steps;
    }

    private static string Say(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length * 2);
        for (var i = 0; i < s.Length;)
        {
            var j = i;
            while (j < s.Length && s[j] == s[i])
                j++;
            sb.Append(j - i).Append(s[i]);
            i = j;
        }
        return sb.ToString();
    }

    private static int[]? Decompose(string s)
    {
        var from = new int[s.Length + 1];
        Array.Fill(from, -1);
        var element = new int[s.Length + 1];
        from[0] = 0;
        for (var pos = 0; pos < s.Length; pos++)
        {
            if (from[pos] < 0)
                continue;
            for (var e = 0; e < Elements.Length; e++)
            {
                var end = pos + Elements[e].Length;
                if (end <= s.Length && from[end] < 0 && string.CompareOrdinal(s, pos, Elements[e], 0, Elements[e].Length) == 0)
                {
                    from[end] = pos;
                    element[end] = e;
                }
            }
        }
        if (from[s.Length] < 0)
            return null;

        var result = new List<int>();
        for (var pos = s.Length; pos > 0; pos = from[pos])
            result.Add(element[pos]);
        result.Reverse();
        return [.. result];
    }

    public override string Part2()
    {
        return LookAndSayLength(50).ToString();
    }

    public override string Input => @"1321131112";
}
