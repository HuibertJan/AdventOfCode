using AdventOfCode;

namespace AoC2015;

public class Day9 : DayBase
{
    public override string Part1()
    {
        return ShortestAndLongestRoute().Shortest.ToString();
    }

    private (int Shortest, int Longest) ShortestAndLongestRoute()
    {
        var indices = new Dictionary<string, int>();
        var edges = new List<(int From, int To, int Distance)>();
        foreach (var line in Input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split(' ');
            var from = indices.TryAdd(parts[0], indices.Count) ? indices.Count - 1 : indices[parts[0]];
            var to = indices.TryAdd(parts[2], indices.Count) ? indices.Count - 1 : indices[parts[2]];
            edges.Add((from, to, int.Parse(parts[4])));
        }

        var n = indices.Count;
        var distances = new int[n, n];
        foreach (var (from, to, distance) in edges)
        {
            distances[from, to] = distance;
            distances[to, from] = distance;
        }

        // Held-Karp: min[mask, i] / max[mask, i] = best path visiting 'mask' and ending at i.
        var full = (1 << n) - 1;
        var min = new int[1 << n, n];
        var max = new int[1 << n, n];
        for (var mask = 1; mask <= full; mask++)
        {
            for (var i = 0; i < n; i++)
            {
                if ((mask & (1 << i)) == 0)
                    continue;
                var previous = mask & ~(1 << i);
                if (previous == 0)
                    continue;

                var best = int.MaxValue;
                var worst = int.MinValue;
                for (var j = 0; j < n; j++)
                {
                    if ((previous & (1 << j)) == 0)
                        continue;
                    best = Math.Min(best, min[previous, j] + distances[j, i]);
                    worst = Math.Max(worst, max[previous, j] + distances[j, i]);
                }
                min[mask, i] = best;
                max[mask, i] = worst;
            }
        }

        var shortest = int.MaxValue;
        var longest = int.MinValue;
        for (var i = 0; i < n; i++)
        {
            shortest = Math.Min(shortest, min[full, i]);
            longest = Math.Max(longest, max[full, i]);
        }
        return (shortest, longest);
    }

    public override string Part2()
    {
        return ShortestAndLongestRoute().Longest.ToString();
    }

    public override string Input => @"AlphaCentauri to Snowdin = 66
AlphaCentauri to Tambi = 28
AlphaCentauri to Faerun = 60
AlphaCentauri to Norrath = 34
AlphaCentauri to Straylight = 34
AlphaCentauri to Tristram = 3
AlphaCentauri to Arbre = 108
Snowdin to Tambi = 22
Snowdin to Faerun = 12
Snowdin to Norrath = 91
Snowdin to Straylight = 121
Snowdin to Tristram = 111
Snowdin to Arbre = 71
Tambi to Faerun = 39
Tambi to Norrath = 113
Tambi to Straylight = 130
Tambi to Tristram = 35
Tambi to Arbre = 40
Faerun to Norrath = 63
Faerun to Straylight = 21
Faerun to Tristram = 57
Faerun to Arbre = 83
Norrath to Straylight = 9
Norrath to Tristram = 50
Norrath to Arbre = 60
Straylight to Tristram = 27
Straylight to Arbre = 81
Tristram to Arbre = 90";
}
