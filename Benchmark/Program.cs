using AoC2015;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<DoBenchmarks>(args: args);
    }
}

[MemoryDiagnoser]
public class DoBenchmarks
{
    private Day01 _day01 = new();
    private Day02 _day02 = new();
    private Day03 _day03 = new();
    private Day04 _day04 = new();
    private Day05 _day05 = new();
    private Day06 _day06 = new();
    private Day07 _day07 = new();
    private Day08 _day08 = new();
    private Day09 _day09 = new();
    private Day10 _day10 = new();
    private Day11 _day11 = new();
    private Day12 _day12 = new();
    private Day13 _day13 = new();
    private Day14 _day14 = new();
    private Day15 _day15 = new();
    private Day16 _day16 = new();
    private Day17 _day17 = new();
    private Day18 _day18 = new();
    private Day19 _day19 = new();
    private Day20 _day20 = new();
    private Day21 _day21 = new();
    private Day22 _day22 = new();
    private Day23 _day23 = new();
    //private Day24 _day24 = new();
    //private Day25 _day25 = new();

    [Benchmark] public string Day1Part1() => _day01.Part1();
    [Benchmark] public string Day1Part2() => _day01.Part2();
    [Benchmark] public string Day2Part1() => _day02.Part1();
    [Benchmark] public string Day2Part2() => _day02.Part2();
    [Benchmark] public string Day3Part1() => _day03.Part1();
    [Benchmark] public string Day3Part2() => _day03.Part2();
    [Benchmark] public string Day4Part1() => _day04.Part1();
    [Benchmark] public string Day4Part2() => _day04.Part2();
    [Benchmark] public string Day5Part1() => _day05.Part1();
    [Benchmark] public string Day5Part2() => _day05.Part2();
    [Benchmark] public string Day6Part1() => _day06.Part1();
    [Benchmark] public string Day6Part2() => _day06.Part2();
    [Benchmark] public string Day7Part1() => _day07.Part1();
    [Benchmark] public string Day7Part2() => _day07.Part2();
    [Benchmark] public string Day8Part1() => _day08.Part1();
    [Benchmark] public string Day8Part2() => _day08.Part2();
    [Benchmark] public string Day9Part1() => _day09.Part1();
    [Benchmark] public string Day9Part2() => _day09.Part2();
    [Benchmark] public string Day10Part1() => _day10.Part1();
    [Benchmark] public string Day10Part2() => _day10.Part2();
    [Benchmark] public string Day11Part1() => _day11.Part1();
    [Benchmark] public string Day11Part2() => _day11.Part2();
    [Benchmark] public string Day12Part1() => _day12.Part1();
    [Benchmark] public string Day12Part2() => _day12.Part2();
    [Benchmark] public string Day13Part1() => _day13.Part1();
    [Benchmark] public string Day13Part2() => _day13.Part2();
    [Benchmark] public string Day14Part1() => _day14.Part1();
    [Benchmark] public string Day14Part2() => _day14.Part2();
    [Benchmark] public string Day15Part1() => _day15.Part1();
    [Benchmark] public string Day15Part2() => _day15.Part2();
    [Benchmark] public string Day16Part1() => _day16.Part1();
    [Benchmark] public string Day16Part2() => _day16.Part2();
    [Benchmark] public string Day17Part1() => _day17.Part1();
    [Benchmark] public string Day17Part2() => _day17.Part2();
    [Benchmark] public string Day18Part1() => _day18.Part1();
    [Benchmark] public string Day18Part2() => _day18.Part2();
    [Benchmark] public string Day19Part1() => _day19.Part1();
    [Benchmark] public string Day19Part2() => _day19.Part2();
    [Benchmark] public string Day20Part1() => _day20.Part1();
    [Benchmark] public string Day20Part2() => _day20.Part2();
    [Benchmark] public string Day21Part1() => _day21.Part1();
    [Benchmark] public string Day21Part2() => _day21.Part2();
    [Benchmark] public string Day22Part1() => _day22.Part1();
    [Benchmark] public string Day22Part2() => _day22.Part2();
    [Benchmark] public string Day23Part1() => _day23.Part1();
    [Benchmark] public string Day23Part2() => _day23.Part2();
}
