using AdventOfCode;

namespace AoC2015;

public class Day21 : DayBase
{
    private record Item(int Cost, int Damage, int Armor);

    private static readonly Item[] Weapons =
    [
        new(8, 4, 0), new(10, 5, 0), new(25, 6, 0), new(40, 7, 0), new(74, 8, 0)
    ];

    private static readonly Item[] Armors =
    [
        new(0, 0, 0), new(13, 0, 1), new(31, 0, 2), new(53, 0, 3), new(75, 0, 4), new(102, 0, 5)
    ];

    private static readonly Item[] Rings =
    [
        new(25, 1, 0), new(50, 2, 0), new(100, 3, 0), new(20, 0, 1), new(40, 0, 2), new(80, 0, 3)
    ];

    public override string Part1()
    {
        var boss = ParseBoss();
        return Loadouts().Where(l => PlayerWins(l, boss)).Min(l => l.Cost).ToString();
    }

    private (int HitPoints, int Damage, int Armor) ParseBoss()
    {
        var values = Input
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => int.Parse(line.Split(':')[1]))
            .ToArray();
        return (values[0], values[1], values[2]);
    }

    private static IEnumerable<Item> Loadouts()
    {
        List<Item[]> ringOptions = [[]];
        for (var i = 0; i < Rings.Length; i++)
        {
            ringOptions.Add([Rings[i]]);
            for (var j = i + 1; j < Rings.Length; j++)
            {
                ringOptions.Add([Rings[i], Rings[j]]);
            }
        }

        foreach (var weapon in Weapons)
        foreach (var armor in Armors)
        foreach (var rings in ringOptions)
        {
            Item[] items = [weapon, armor, .. rings];
            yield return new Item(items.Sum(i => i.Cost), items.Sum(i => i.Damage), items.Sum(i => i.Armor));
        }
    }

    private static bool PlayerWins(Item player, (int HitPoints, int Damage, int Armor) boss)
    {
        const int playerHitPoints = 100;
        var playerHit = Math.Max(1, player.Damage - boss.Armor);
        var bossHit = Math.Max(1, boss.Damage - player.Armor);
        var turnsToKillBoss = (boss.HitPoints + playerHit - 1) / playerHit;
        var turnsToKillPlayer = (playerHitPoints + bossHit - 1) / bossHit;
        return turnsToKillBoss <= turnsToKillPlayer;
    }

    public override string Part2()
    {
        var boss = ParseBoss();
        return Loadouts().Where(l => !PlayerWins(l, boss)).Max(l => l.Cost).ToString();
    }

    public override string Input => @"Hit Points: 104
Damage: 8
Armor: 1";
}
