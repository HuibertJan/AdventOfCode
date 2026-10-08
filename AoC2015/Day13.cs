using AdventOfCode;

namespace AoC2015;

public class Day13 : DayBase
{
    public override string Part1()
    {
        return OptimalHappiness(ParseHappiness()).ToString();
    }

    private int[,] ParseHappiness()
    {
        var indices = new Dictionary<string, int>();
        var entries = new List<(int Person, int Neighbor, int Change)>();
        foreach (var line in Input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.TrimEnd('.').Split(' ');
            var person = indices.TryAdd(parts[0], indices.Count) ? indices.Count - 1 : indices[parts[0]];
            var neighbor = indices.TryAdd(parts[10], indices.Count) ? indices.Count - 1 : indices[parts[10]];
            var change = int.Parse(parts[3]);
            entries.Add((person, neighbor, parts[2] == "gain" ? change : -change));
        }

        // Pair happiness is symmetric: both directions summed.
        var n = indices.Count;
        var pair = new int[n, n];
        foreach (var (person, neighbor, change) in entries)
        {
            pair[person, neighbor] += change;
            pair[neighbor, person] += change;
        }
        return pair;
    }

    // Held-Karp over a cycle: fix person 0 as the start, max[mask, i] = best path from 0 through 'mask' ending at i.
    private static int OptimalHappiness(int[,] pair)
    {
        var n = pair.GetLength(0);
        var full = (1 << n) - 1;
        var max = new int[1 << n, n];
        for (var mask = 0; mask <= full; mask++)
            for (var i = 0; i < n; i++)
                max[mask, i] = int.MinValue;
        max[1, 0] = 0;

        for (var mask = 1; mask <= full; mask += 2)
        {
            for (var i = 0; i < n; i++)
            {
                var current = max[mask, i];
                if (current == int.MinValue)
                    continue;
                for (var j = 1; j < n; j++)
                {
                    var bit = 1 << j;
                    if ((mask & bit) != 0)
                        continue;
                    var candidate = current + pair[i, j];
                    if (candidate > max[mask | bit, j])
                        max[mask | bit, j] = candidate;
                }
            }
        }

        var best = int.MinValue;
        for (var i = 1; i < n; i++)
            best = Math.Max(best, max[full, i] + pair[i, 0]);
        return best;
    }

    public override string Part2()
    {
        var pair = ParseHappiness();
        var n = pair.GetLength(0);
        var withMe = new int[n + 1, n + 1];
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                withMe[i, j] = pair[i, j];
        return OptimalHappiness(withMe).ToString();
    }

    public override string Input => @"Alice would gain 2 happiness units by sitting next to Bob.
Alice would gain 26 happiness units by sitting next to Carol.
Alice would lose 82 happiness units by sitting next to David.
Alice would lose 75 happiness units by sitting next to Eric.
Alice would gain 42 happiness units by sitting next to Frank.
Alice would gain 38 happiness units by sitting next to George.
Alice would gain 39 happiness units by sitting next to Mallory.
Bob would gain 40 happiness units by sitting next to Alice.
Bob would lose 61 happiness units by sitting next to Carol.
Bob would lose 15 happiness units by sitting next to David.
Bob would gain 63 happiness units by sitting next to Eric.
Bob would gain 41 happiness units by sitting next to Frank.
Bob would gain 30 happiness units by sitting next to George.
Bob would gain 87 happiness units by sitting next to Mallory.
Carol would lose 35 happiness units by sitting next to Alice.
Carol would lose 99 happiness units by sitting next to Bob.
Carol would lose 51 happiness units by sitting next to David.
Carol would gain 95 happiness units by sitting next to Eric.
Carol would gain 90 happiness units by sitting next to Frank.
Carol would lose 16 happiness units by sitting next to George.
Carol would gain 94 happiness units by sitting next to Mallory.
David would gain 36 happiness units by sitting next to Alice.
David would lose 18 happiness units by sitting next to Bob.
David would lose 65 happiness units by sitting next to Carol.
David would lose 18 happiness units by sitting next to Eric.
David would lose 22 happiness units by sitting next to Frank.
David would gain 2 happiness units by sitting next to George.
David would gain 42 happiness units by sitting next to Mallory.
Eric would lose 65 happiness units by sitting next to Alice.
Eric would gain 24 happiness units by sitting next to Bob.
Eric would gain 100 happiness units by sitting next to Carol.
Eric would gain 51 happiness units by sitting next to David.
Eric would gain 21 happiness units by sitting next to Frank.
Eric would gain 55 happiness units by sitting next to George.
Eric would lose 44 happiness units by sitting next to Mallory.
Frank would lose 48 happiness units by sitting next to Alice.
Frank would gain 91 happiness units by sitting next to Bob.
Frank would gain 8 happiness units by sitting next to Carol.
Frank would lose 66 happiness units by sitting next to David.
Frank would gain 97 happiness units by sitting next to Eric.
Frank would lose 9 happiness units by sitting next to George.
Frank would lose 92 happiness units by sitting next to Mallory.
George would lose 44 happiness units by sitting next to Alice.
George would lose 25 happiness units by sitting next to Bob.
George would gain 17 happiness units by sitting next to Carol.
George would gain 92 happiness units by sitting next to David.
George would lose 92 happiness units by sitting next to Eric.
George would gain 18 happiness units by sitting next to Frank.
George would gain 97 happiness units by sitting next to Mallory.
Mallory would gain 92 happiness units by sitting next to Alice.
Mallory would lose 96 happiness units by sitting next to Bob.
Mallory would lose 51 happiness units by sitting next to Carol.
Mallory would lose 81 happiness units by sitting next to David.
Mallory would gain 31 happiness units by sitting next to Eric.
Mallory would lose 73 happiness units by sitting next to Frank.
Mallory would lose 89 happiness units by sitting next to George.";
}
