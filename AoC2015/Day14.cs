using AdventOfCode;

namespace AoC2015;

public class Day14 : DayBase
{
    private const int RaceSeconds = 2503;

    public override string Part1()
    {
        return ParseReindeer().Max(r => Distance(r, RaceSeconds)).ToString();
    }

    private readonly record struct Reindeer(int Speed, int Fly, int Rest);

    private List<Reindeer> ParseReindeer()
    {
        var reindeer = new List<Reindeer>();
        foreach (var line in Input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split(' ');
            reindeer.Add(new Reindeer(int.Parse(parts[3]), int.Parse(parts[6]), int.Parse(parts[13])));
        }
        return reindeer;
    }

    private static int Distance(Reindeer r, int seconds)
    {
        var cycle = r.Fly + r.Rest;
        var flying = seconds / cycle * r.Fly + Math.Min(seconds % cycle, r.Fly);
        return flying * r.Speed;
    }

    public override string Part2()
    {
        var reindeer = ParseReindeer();
        var n = reindeer.Count;
        var distances = new int[n];
        var points = new int[n];

        for (var second = 0; second < RaceSeconds; second++)
        {
            var lead = 0;
            for (var i = 0; i < n; i++)
            {
                var r = reindeer[i];
                if (second % (r.Fly + r.Rest) < r.Fly)
                    distances[i] += r.Speed;
                lead = Math.Max(lead, distances[i]);
            }
            for (var i = 0; i < n; i++)
            {
                if (distances[i] == lead)
                    points[i]++;
            }
        }
        return points.Max().ToString();
    }

    public override string Input => @"Vixen can fly 8 km/s for 8 seconds, but then must rest for 53 seconds.
Blitzen can fly 13 km/s for 4 seconds, but then must rest for 49 seconds.
Rudolph can fly 20 km/s for 7 seconds, but then must rest for 132 seconds.
Cupid can fly 12 km/s for 4 seconds, but then must rest for 43 seconds.
Donner can fly 9 km/s for 5 seconds, but then must rest for 38 seconds.
Dasher can fly 10 km/s for 4 seconds, but then must rest for 37 seconds.
Comet can fly 3 km/s for 37 seconds, but then must rest for 76 seconds.
Prancer can fly 9 km/s for 12 seconds, but then must rest for 97 seconds.
Dancer can fly 37 km/s for 1 seconds, but then must rest for 36 seconds.";
}
