using AdventOfCode;

namespace AoC2015;

public class Day10 : DayBase
{
    public override string Part1()
    {
        return LookAndSayLength(40).ToString();
    }

    private int LookAndSayLength(int steps)
    {
        var current = new byte[Input.Length];
        for (var i = 0; i < Input.Length; i++)
            current[i] = (byte)(Input[i] - '0');
        var length = current.Length;
        var next = Array.Empty<byte>();

        for (var step = 0; step < steps; step++)
        {
            if (next.Length < length * 2)
                next = new byte[length * 2];

            var write = 0;
            var read = 0;
            while (read < length)
            {
                var digit = current[read];
                var start = read;
                while (read < length && current[read] == digit)
                    read++;
                next[write++] = (byte)(read - start);
                next[write++] = digit;
            }

            (current, next) = (next, current);
            length = write;
        }
        return length;
    }

    public override string Part2()
    {
        return LookAndSayLength(50).ToString();
    }

    public override string Input => @"1321131112";
}
