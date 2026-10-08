using AdventOfCode;

namespace AoC2015;

public class Day24 : DayBase
{
    public override string Part1() => Solve(3).ToString();

    private long Solve(int groups)
    {
        var weights = Input
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .OrderByDescending(w => w)
            .ToArray();
        var target = weights.Sum() / groups;

        for (var size = 1; size <= weights.Length; size++)
        {
            var best = long.MaxValue;
            Search(weights, 0, size, target, 1, ref best);
            if (best != long.MaxValue)
            {
                return best;
            }
        }

        throw new InvalidOperationException("No configuration found.");
    }

    private static void Search(int[] weights, int start, int remaining, int target, long product, ref long best)
    {
        if (remaining == 0)
        {
            if (target == 0 && product < best)
            {
                best = product;
            }
            return;
        }

        if (product >= best) return;

        for (var i = start; i <= weights.Length - remaining; i++)
        {
            if (weights[i] > target) continue;
            Search(weights, i + 1, remaining - 1, target - weights[i], product * weights[i], ref best);
        }
    }

    public override string Part2()
    {
        throw new NotImplementedException();
    }

    public override string Input => @"1
3
5
11
13
17
19
23
29
31
37
41
43
47
53
59
67
71
73
79
83
89
97
101
103
107
109
113";
}
