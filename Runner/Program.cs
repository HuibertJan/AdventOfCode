using System.Diagnostics;
using System.Reflection;
using AdventOfCode;

namespace Runner;

internal static class Program
{
    private static void Main()
    {
        var dayTypes = GetAssemblies()
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && typeof(DayBase).IsAssignableFrom(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal);

        foreach (var dayType in dayTypes)
        {
            try
            {
                var instance = (DayBase)Activator.CreateInstance(dayType);

                RunPart(dayType, nameof(DayBase.Part1), instance.Part1);
                RunPart(dayType, nameof(DayBase.Part2), instance.Part2);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{dayType.FullName}: Could not create instance: {exception.GetBaseException().Message}");
                continue;
            }
        }
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
        yield return Assembly.GetExecutingAssembly();

        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
        {
            yield return Assembly.LoadFrom(path);
        }
    }

    private static void RunPart(Type dayType, string partName, Func<string> part)
    {
        var stopwatch = Stopwatch.StartNew();
        string output;

        try
        {
            output = part();
        }
        catch (Exception exception)
        {
            output = $"ERROR ({exception.GetType().Name}): {exception.Message}";
        }
        finally
        {
            stopwatch.Stop();
        }

        Console.WriteLine($"{dayType.FullName}.{partName}: {output} ({stopwatch.Elapsed.TotalMilliseconds:F3} ms)");
    }
}