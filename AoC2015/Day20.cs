using AdventOfCode;

namespace AoC2015;

public class Day20 : DayBase
{
    public override string Part1()
    {
        var target = int.Parse(Input);
        var limit = target / 10;
        var presents = new int[limit + 1];

        for (var elf = 1; elf <= limit; elf++)
        {
            for (var house = elf; house <= limit; house += elf)
            {
                presents[house] += elf * 10;
            }
        }

        for (var house = 1; house <= limit; house++)
        {
            if (presents[house] >= target)
            {
                return house.ToString();
            }
        }

        throw new InvalidOperationException("No house found.");
    }

    public override string Part2()
    {
        var target = int.Parse(Input);
        var limit = target / 11;
        var presents = new int[limit + 1];

        for (var elf = 1; elf <= limit; elf++)
        {
            for (int house = elf, visits = 0; house <= limit && visits < 50; house += elf, visits++)
            {
                presents[house] += elf * 11;
            }
        }

        for (var house = 1; house <= limit; house++)
        {
            if (presents[house] >= target)
            {
                return house.ToString();
            }
        }

        throw new InvalidOperationException("No house found.");
    }

    public override string Input => @"33100000";
}
