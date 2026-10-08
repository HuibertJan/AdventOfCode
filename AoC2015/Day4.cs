using System.Security.Cryptography;
using System.Text;
using AdventOfCode;

namespace AoC2015;

public class Day4 : DayBase
{
    public override string Part1() => FindLowestNumber(0xF0).ToString();

    public override string Part2() => FindLowestNumber(0xFF).ToString();

    private int FindLowestNumber(byte thirdByteMask)
    {
        var key = Encoding.ASCII.GetBytes(Input);
        Span<byte> buffer = stackalloc byte[key.Length + 20];
        key.CopyTo(buffer);
        Span<byte> hash = stackalloc byte[16];

        for (var number = 1; ; number++)
        {
            number.TryFormat(buffer[key.Length..], out var written);
            MD5.HashData(buffer[..(key.Length + written)], hash);

            if (hash[0] == 0 && hash[1] == 0 && (hash[2] & thirdByteMask) == 0)
            {
                return number;
            }
        }
    }

    public override string Input => @"yzbqklnj";
}
