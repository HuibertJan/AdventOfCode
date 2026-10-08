using AdventOfCode;

namespace AoC2015;

public class Day17 : DayBase
{
    private const int Liters = 150;

    public override string Part1()
    {
        var ways = CountWays();
        var total = 0;
        for (var count = 0; count < ways.GetLength(0); count++)
            total += ways[count, Liters];
        return total.ToString();
    }

    // ways[count, liters] = number of combinations using exactly 'count' containers summing to 'liters'.
    private int[,] CountWays()
    {
        var containers = Input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToArray();
        var n = containers.Length;
        var ways = new int[n + 1, Liters + 1];
        ways[0, 0] = 1;

        foreach (var size in containers)
            for (var count = n; count >= 1; count--)
                for (var liters = Liters; liters >= size; liters--)
                    ways[count, liters] += ways[count - 1, liters - size];

        return ways;
    }

    public override string Part2()
    {
        var ways = CountWays();
        for (var count = 1; count < ways.GetLength(0); count++)
        {
            if (ways[count, Liters] > 0)
                return ways[count, Liters].ToString();
        }
        return "0";
    }

    public override string Input => @"43
3
4
10
21
44
4
6
47
41
34
17
17
44
36
31
46
9
27
38";
}
