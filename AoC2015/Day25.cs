using AdventOfCode;

namespace AoC2015;

public class Day25 : DayBase
{
    public override string Part1()
    {
        var numbers = System.Text.RegularExpressions.Regex.Matches(Input, @"\d+")
            .Select(m => long.Parse(m.Value))
            .ToArray();
        var row = numbers[0];
        var column = numbers[1];

        var diagonal = row + column - 1;
        var index = diagonal * (diagonal - 1) / 2 + column;

        const long modulus = 33554393;
        var factor = (long)System.Numerics.BigInteger.ModPow(252533, index - 1, modulus);
        return (20151125 * factor % modulus).ToString();
    }

    public override string Part2()
    {
        throw new NotImplementedException();
    }

    public override string Input => @"To continue, please consult the code grid in the manual.  Enter the code at row 2981, column 3075.";
}
