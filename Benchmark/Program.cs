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
    private Day1 _day1 = new();
    private Day2 _day2 = new();
    private Day3 _day3 = new();
    private Day4 _day4 = new();

    private Day5 _day5 = new();
    private Day6 _day6 = new();

    [Benchmark] public string Day1Part1() => _day1.Part1();
    [Benchmark] public string Day1Part2() => _day1.Part2();
    [Benchmark] public string Day2Part1() => _day2.Part1();
    [Benchmark] public string Day2Part2() => _day2.Part2();
    [Benchmark] public string Day3Part1() => _day3.Part1();
    [Benchmark] public string Day3Part2() => _day3.Part2();
    [Benchmark] public string Day4Part1() => _day4.Part1();
    [Benchmark] public string Day4Part2() => _day4.Part2();
    [Benchmark] public string Day5Part1() => _day5.Part1();
    [Benchmark] public string Day5Part2() => _day5.Part2();
    [Benchmark] public string Day6Part1() => _day6.Part1();
    [Benchmark] public string Day6Part2() => _day6.Part2();

}
