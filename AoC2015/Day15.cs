using AdventOfCode;

namespace AoC2015;

public class Day15 : DayBase
{
    private const int Teaspoons = 100;

    public override string Part1()
    {
        return BestScore(null).ToString();
    }

    private int[][] ParseIngredients()
    {
        return Input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line[(line.IndexOf(':') + 1)..]
                .Split(',')
                .Select(p => int.Parse(p.Trim().Split(' ')[1]))
                .ToArray())
            .ToArray();
    }

    // Enumerates every split of the teaspoons, keeping running property totals (last property is calories).
    private long BestScore(int? calories)
    {
        var ingredients = ParseIngredients();
        var properties = ingredients[0].Length;
        var totals = new int[properties];
        var best = 0L;

        void Add(int ingredient, int amount)
        {
            for (var p = 0; p < properties; p++)
                totals[p] += ingredients[ingredient][p] * amount;
        }

        void Choose(int ingredient, int remaining)
        {
            if (ingredient == ingredients.Length - 1)
            {
                Add(ingredient, remaining);
                if (calories is null || totals[properties - 1] == calories)
                {
                    var score = 1L;
                    for (var p = 0; p < properties - 1 && score > 0; p++)
                        score *= Math.Max(0, totals[p]);
                    best = Math.Max(best, score);
                }
                Add(ingredient, -remaining);
                return;
            }

            for (var amount = 0; amount <= remaining; amount++)
            {
                Choose(ingredient + 1, remaining - amount);
                Add(ingredient, 1);
            }
            Add(ingredient, -(remaining + 1));
        }

        Choose(0, Teaspoons);
        return best;
    }

    public override string Part2()
    {
        return BestScore(500).ToString();
    }

    public override string Input => @"Sprinkles: capacity 2, durability 0, flavor -2, texture 0, calories 3
Butterscotch: capacity 0, durability 5, flavor -3, texture 0, calories 3
Chocolate: capacity 0, durability 0, flavor 5, texture -1, calories 8
Candy: capacity 0, durability -1, flavor 0, texture 5, calories 8";
}
