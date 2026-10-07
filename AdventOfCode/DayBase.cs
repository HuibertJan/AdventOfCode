namespace AdventOfCode;

public abstract class DayBase
{
    public abstract string Part1();

    public abstract string Part2();

    public abstract string Input { get; }

    protected IEnumerable<string> ReadLines(string input)
    {
        var reader = new StringReader(input);
        string? s;
        while ((s = reader.ReadLine()) != null)
            yield return s;
    }
}
