using AdventOfCode;

namespace AoC2015;

public class Day22 : DayBase
{
    private readonly record struct State(int PlayerHp, int Mana, int BossHp, int Shield, int Poison, int Recharge, int Spent);

    private static readonly int[] SpellCosts = [53, 73, 113, 173, 229];

    public override string Part1() => Solve(false).ToString();

    private int Solve(bool hardMode)
    {
        var values = Input
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => int.Parse(line.Split(':')[1]))
            .ToArray();
        var bossDamage = values[1];

        var queue = new PriorityQueue<(State State, bool Won), int>();
        queue.Enqueue((new State(50, 500, values[0], 0, 0, 0, 0), false), 0);

        while (queue.TryDequeue(out var item, out _))
        {
            if (item.Won)
            {
                return item.State.Spent;
            }

            var state = item.State;
            if (hardMode)
            {
                state = state with { PlayerHp = state.PlayerHp - 1 };
                if (state.PlayerHp <= 0) continue;
            }

            state = ApplyEffects(state);
            if (state.BossHp <= 0)
            {
                queue.Enqueue((state, true), state.Spent);
                continue;
            }

            for (var spell = 0; spell < SpellCosts.Length; spell++)
            {
                var cost = SpellCosts[spell];
                if (cost > state.Mana) continue;

                var next = state with { Mana = state.Mana - cost, Spent = state.Spent + cost };
                switch (spell)
                {
                    case 0: next = next with { BossHp = next.BossHp - 4 }; break;
                    case 1: next = next with { BossHp = next.BossHp - 2, PlayerHp = next.PlayerHp + 2 }; break;
                    case 2: if (state.Shield > 0) continue; next = next with { Shield = 6 }; break;
                    case 3: if (state.Poison > 0) continue; next = next with { Poison = 6 }; break;
                    case 4: if (state.Recharge > 0) continue; next = next with { Recharge = 5 }; break;
                }

                if (next.BossHp <= 0)
                {
                    queue.Enqueue((next, true), next.Spent);
                    continue;
                }

                next = ApplyEffects(next);
                if (next.BossHp <= 0)
                {
                    queue.Enqueue((next, true), next.Spent);
                    continue;
                }

                var armor = next.Shield > 0 ? 7 : 0;

                next = next with { PlayerHp = next.PlayerHp - Math.Max(1, bossDamage - armor) };
                if (next.PlayerHp > 0)
                {
                    queue.Enqueue((next, false), next.Spent);
                }
            }
        }

        throw new InvalidOperationException("No winning strategy found.");
    }

    private static State ApplyEffects(State state)
    {
        if (state.Poison > 0)
        {
            state = state with { BossHp = state.BossHp - 3, Poison = state.Poison - 1 };
        }

        if (state.Recharge > 0)
        {
            state = state with { Mana = state.Mana + 101, Recharge = state.Recharge - 1 };
        }

        if (state.Shield > 0)
        {
            state = state with { Shield = state.Shield - 1 };
        }

        return state;
    }

    public override string Part2() => Solve(true).ToString();

    public override string Input => @"Hit Points: 55
Damage: 8";
}
