using AdventOfCode;

namespace AoC2015;

public class Day19 : DayBase
{
    public override string Part1()
    {
        var lines = Input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var molecule = lines[^1];
        var replacements = lines
            .Where(line => line.Contains("=>"))
            .Select(line => line.Split(" => "))
            .Select(parts => (From: parts[0], To: parts[1]));

        var results = new HashSet<string>();
        foreach (var (from, to) in replacements)
        {
            var index = molecule.IndexOf(from, StringComparison.Ordinal);
            while (index >= 0)
            {
                results.Add(string.Concat(molecule.AsSpan(0, index), to, molecule.AsSpan(index + from.Length)));
                index = molecule.IndexOf(from, index + 1, StringComparison.Ordinal);
            }
        }

        return results.Count.ToString();
    }

    public override string Part2()
    {
        var lines = Input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var molecule = lines[^1];

        var elements = molecule.Count(char.IsUpper);
        var rn = CountOccurrences(molecule, "Rn");
        var ar = CountOccurrences(molecule, "Ar");
        var y = molecule.Count(c => c == 'Y');

        return (elements - rn - ar - 2 * y - 1).ToString();
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = text.IndexOf(value, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }

    public override string Input => @"Al => ThF
Al => ThRnFAr
B => BCa
B => TiB
B => TiRnFAr
Ca => CaCa
Ca => PB
Ca => PRnFAr
Ca => SiRnFYFAr
Ca => SiRnMgAr
Ca => SiTh
F => CaF
F => PMg
F => SiAl
H => CRnAlAr
H => CRnFYFYFAr
H => CRnFYMgAr
H => CRnMgYFAr
H => HCa
H => NRnFYFAr
H => NRnMgAr
H => NTh
H => OB
H => ORnFAr
Mg => BF
Mg => TiMg
N => CRnFAr
N => HSi
O => CRnFYFAr
O => CRnMgAr
O => HP
O => NRnFAr
O => OTi
P => CaP
P => PTi
P => SiRnFAr
Si => CaSi
Th => ThCa
Ti => BP
Ti => TiTi
e => HF
e => NAl
e => OMg

CRnCaCaCaSiRnBPTiMgArSiRnSiRnMgArSiRnCaFArTiTiBSiThFYCaFArCaCaSiThCaPBSiThSiThCaCaPTiRnPBSiThRnFArArCaCaSiThCaSiThSiRnMgArCaPTiBPRnFArSiThCaSiRnFArBCaSiRnCaPRnFArPMgYCaFArCaPTiTiTiBPBSiThCaPTiBPBSiRnFArBPBSiRnCaFArBPRnSiRnFArRnSiRnBFArCaFArCaCaCaSiThSiThCaCaPBPTiTiRnFArCaPTiBSiAlArPBCaCaCaCaCaSiRnMgArCaSiThFArThCaSiThCaSiRnCaFYCaSiRnFYFArFArCaSiRnFYFArCaSiRnBPMgArSiThPRnFArCaSiRnFArTiRnSiRnFYFArCaSiRnBFArCaSiRnTiMgArSiThCaSiThCaFArPRnFArSiRnFArTiTiTiTiBCaCaSiRnCaCaFYFArSiThCaPTiBPTiBCaSiThSiRnMgArCaF";
}
