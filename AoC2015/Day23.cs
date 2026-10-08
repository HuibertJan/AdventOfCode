using AdventOfCode;

namespace AoC2015;

public class Day23 : DayBase
{
    public override string Part1() => Run(0).ToString();

    private long Run(long initialA)
    {
        var program = Input
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        var registers = new Dictionary<string, long> { ["a"] = initialA, ["b"] = 0 };
        var pointer = 0;

        while (pointer >= 0 && pointer < program.Length)
        {
            var instruction = program[pointer];
            switch (instruction[0])
            {
                case "hlf": registers[instruction[1]] /= 2; pointer++; break;
                case "tpl": registers[instruction[1]] *= 3; pointer++; break;
                case "inc": registers[instruction[1]]++; pointer++; break;
                case "jmp": pointer += int.Parse(instruction[1]); break;
                case "jie": pointer += registers[instruction[1]] % 2 == 0 ? int.Parse(instruction[2]) : 1; break;
                case "jio": pointer += registers[instruction[1]] == 1 ? int.Parse(instruction[2]) : 1; break;
                default: throw new InvalidOperationException($"Unknown instruction: {instruction[0]}");
            }
        }

        return registers["b"];
    }

    public override string Part2() => Run(1).ToString();

    public override string Input => @"jio a, +19
inc a
tpl a
inc a
tpl a
inc a
tpl a
tpl a
inc a
inc a
tpl a
tpl a
inc a
inc a
tpl a
inc a
inc a
tpl a
jmp +23
tpl a
tpl a
inc a
inc a
tpl a
inc a
inc a
tpl a
inc a
tpl a
inc a
tpl a
inc a
tpl a
inc a
inc a
tpl a
inc a
inc a
tpl a
tpl a
inc a
jio a, +8
inc b
jie a, +4
tpl a
inc a
jmp +2
hlf a
jmp -7";
}
