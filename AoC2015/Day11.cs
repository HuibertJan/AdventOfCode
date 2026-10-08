using AdventOfCode;

namespace AoC2015;

public class Day11 : DayBase
{
    public override string Part1()
    {
        return NextPassword(Input);
    }

    private static string NextPassword(string current)
    {
        if (current.Length < 3)
            throw new ArgumentException("Password must be at least 3 letters; is Input filled in?", nameof(current));

        var password = current.ToCharArray();
        var skipped = false;

        // Skip past any forbidden letter already present: bump it and reset everything after it.
        for (var i = 0; i < password.Length; i++)
        {
            if (IsForbidden(password[i]))
            {
                password[i]++;
                for (var j = i + 1; j < password.Length; j++)
                    password[j] = 'a';
                skipped = true;
                break;
            }
        }

        if (skipped && IsValid(password))
            return new string(password);

        do
        {
            Increment(password);
        } while (!IsValid(password));

        return new string(password);
    }

    private static void Increment(char[] password)
    {
        for (var i = password.Length - 1; i >= 0; i--)
        {
            if (password[i] == 'z')
            {
                password[i] = 'a';
                continue;
            }
            password[i]++;
            if (IsForbidden(password[i]))
                password[i]++;
            return;
        }
    }

    private static bool IsForbidden(char c) => c is 'i' or 'o' or 'l';

    private static bool IsValid(char[] password)
    {
        var hasStraight = false;
        for (var i = 2; i < password.Length && !hasStraight; i++)
            hasStraight = password[i - 2] + 1 == password[i - 1] && password[i - 1] + 1 == password[i];
        if (!hasStraight)
            return false;

        var firstPair = '\0';
        for (var i = 1; i < password.Length; i++)
        {
            if (password[i] != password[i - 1])
                continue;
            if (firstPair == '\0')
                firstPair = password[i];
            else if (password[i] != firstPair)
                return true;
            i++;
        }
        return false;
    }

    public override string Part2()
    {
        return NextPassword(NextPassword(Input));
    }

    public override string Input => @"hxbxwxba";
}
