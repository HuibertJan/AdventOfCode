using AdventOfCode;
using System.Numerics;

namespace AoC2015;

public class Day20 : DayBase
{
    public override string Part1()
    {
        var target = (int.Parse(Input) + 9L) / 10;
        var limit = 1;

        // A power-of-two house has divisor sum 2 * house - 1, giving a guaranteed upper bound.
        while (2L * limit - 1 < target)
        {
            limit *= 2;
        }

        var divisors = new int[(limit + 1) / 2];
        divisors[0] = 1;

        for (var prime = 3; prime <= limit / prime; prime += 2)
        {
            if (divisors[prime / 2] != 0)
            {
                continue;
            }

            for (var index = prime * prime / 2; index < divisors.Length; index += prime)
            {
                if (divisors[index] == 0)
                {
                    divisors[index] = prime;
                }
            }
        }

        // Replace each odd house's smallest prime factor with its divisor sum in ascending order.
        for (var house = 1; house <= limit; house++)
        {
            if ((house & 1) != 0 && house > 1)
            {
                var prime = divisors[house / 2];
                if (prime == 0)
                {
                    divisors[house / 2] = house + 1;
                }
                else
                {
                    var remaining = house;
                    var power = 1;
                    var sum = 1;
                    do
                    {
                        remaining /= prime;
                        power *= prime;
                        sum += power;
                    } while (remaining % prime == 0);

                    divisors[house / 2] = divisors[remaining / 2] * sum;
                }
            }

            var twos = BitOperations.TrailingZeroCount((uint)house);
            var sumOfDivisors = (long)divisors[(house >> twos) / 2] * ((1L << (twos + 1)) - 1);
            if (sumOfDivisors >= target)
            {
                return house.ToString();
            }
        }

        throw new InvalidOperationException("No house found.");
    }

    public override string Part2()
    {
        var target = (int.Parse(Input) + 10L) / 11;
        var maximum = Math.Max(1, (int)target);
        var presents = new int[Math.Min(16384, maximum)];

        for (var first = 1; first <= maximum; first += presents.Length)
        {
            var length = Math.Min(presents.Length, maximum - first + 1);
            var last = first + length - 1;
            for (var index = 0; index < length; index++)
            {
                presents[index] = first + index;
            }

            for (var visit = 2; visit <= 50; visit++)
            {
                for (int elf = (first + visit - 1) / visit, house = elf * visit; house <= last; elf++, house += visit)
                {
                    presents[house - first] += elf;
                }
            }

            for (var index = 0; index < length; index++)
            {
                if (presents[index] >= target)
                {
                    return (first + index).ToString();
                }
            }
        }

        throw new InvalidOperationException("No house found.");
    }

    public override string Input => @"33100000";
}
