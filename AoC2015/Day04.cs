using AdventOfCode;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace AoC2015;

public class Day04 : DayBase
{
    // The first three hash bytes are the low three bytes of the first (little-endian) MD5 word.
    public override string Part1() => FindLowestNumber(0x00F0FFFF).ToString();

    public override string Part2() => FindLowestNumber(0x00FFFFFF).ToString();

    private const int Lanes = 8;

    // Numbers are split as prefix * SuffixRange + suffix. The prefix is shared by all lanes,
    // only the four suffix digits differ per lane.
    private const int SuffixRange = 10000;
    private const int SuffixDigits = 4;

    private static readonly uint[] K =
    [
        0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
        0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
        0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
        0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
        0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
        0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
        0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
        0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391,
    ];

    private static readonly int[] MessageIndex = Enumerable.Range(0, 64)
        .Select(step => (step / 16) switch
        {
            0 => step,
            1 => (1 + 5 * step) % 16,
            2 => (5 + 3 * step) % 16,
            _ => 7 * step % 16,
        })
        .ToArray();

    private const uint A0 = 0x67452301;
    private const uint B0 = 0xefcdab89;
    private const uint C0 = 0x98badcfe;
    private const uint D0 = 0x10325476;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int FindLowestNumber(uint firstWordMask)
    {
        var key = Encoding.ASCII.GetBytes(Input);

        var small = FindLowestSmall(key, firstWordMask, SuffixRange);
        if (small > 0)
        {
            return small;
        }

        Span<byte> block = stackalloc byte[64];
        var words = MemoryMarshal.Cast<byte, uint>(block);
        Span<Vector256<uint>> kc = stackalloc Vector256<uint>[64];
        Span<Vector256<uint>> m = stackalloc Vector256<uint>[16];
        var mask = Vector256.Create(firstWordMask);
        var low = new Vector256<uint>[SuffixRange / Lanes];
        var high = new Vector256<uint>[SuffixRange / Lanes];
        var tableOffset = -1;

        for (var prefix = 1; ; prefix++)
        {
            block.Clear();
            key.CopyTo(block);
            prefix.TryFormat(block[key.Length..], out var written);
            var suffixStart = key.Length + written;
            var length = suffixStart + SuffixDigits;
            if (length > 55)
            {
                throw new InvalidOperationException("Message does not fit in a single MD5 block.");
            }

            block[length] = 0x80;
            BinaryPrimitives.WriteUInt64LittleEndian(block[56..], (ulong)length * 8);

            var offset = suffixStart % 4;
            if (offset != tableOffset)
            {
                BuildSuffixTable(low, high, offset);
                tableOffset = offset;
            }

            // Constant message words (key, prefix, padding, length) are folded into the step constants.
            for (var step = 0; step < 64; step++)
            {
                kc[step] = Vector256.Create(K[step] + words[MessageIndex[step]]);
            }

            var a = B0 + uint.RotateLeft(A0 + ((B0 & C0) | (~B0 & D0)) + K[0] + words[0], 7);
            var d = a + uint.RotateLeft(D0 + ((a & B0) | (~a & C0)) + K[1] + words[1], 12);
            var va = Vector256.Create(a);
            var vb = Vector256.Create(B0);
            var vc = Vector256.Create(C0);
            var vd = Vector256.Create(d);

            var hit = (suffixStart / 4) switch
            {
                2 => SearchSuffixes<SuffixAt2>(va, vb, vc, vd, kc, m, low, high, mask, 0),
                3 => SearchSuffixes<SuffixAt3>(va, vb, vc, vd, kc, m, low, high, mask, 0),
                4 => SearchSuffixes<SuffixAt4>(va, vb, vc, vd, kc, m, low, high, mask, 0),
                5 => SearchSuffixes<SuffixAt5>(va, vb, vc, vd, kc, m, low, high, mask, 0),
                var word => SearchSuffixes<AllWords>(va, vb, vc, vd, kc, m, low, high, mask, word),
            };

            if (hit >= 0)
            {
                return prefix * SuffixRange + hit;
            }
        }
    }

    // Returns the lowest matching suffix, or -1. lowWord is only used by the AllWords fallback layout.
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static int SearchSuffixes<TLayout>(
        Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d,
        ReadOnlySpan<Vector256<uint>> kc, Span<Vector256<uint>> m,
        Vector256<uint>[] low, Vector256<uint>[] high, Vector256<uint> mask, int lowWord)
        where TLayout : struct, ILayout
    {
        m.Clear();
        for (var i = 0; i < low.Length; i++)
        {
            if (TLayout.LowWord < 0)
            {
                m[lowWord] = low[i];
                m[lowWord + 1] = high[i];
            }

            var (sa, sb, sc, sd) = (a, b, c, d);
            if (TLayout.LowWord < 0 && lowWord < 2)
            {
                // Steps 1-2 depend on the suffix, so they cannot use the precomputed scalar state.
                (sa, sb, sc, sd) = (Vector256.Create(A0), Vector256.Create(B0), Vector256.Create(C0), Vector256.Create(D0));
                F(ref sa, sb, sc, sd, kc[0] + m[0], 7);
                F(ref sd, sa, sb, sc, kc[1] + m[1], 12);
            }

            var hits = Vector256.Equals(Md5FirstWordFromStep2<TLayout>(sa, sb, sc, sd, kc, m, low[i], high[i]) & mask, Vector256<uint>.Zero)
                .ExtractMostSignificantBits();
            if (hits != 0)
            {
                return i * Lanes + BitOperations.TrailingZeroCount(hits);
            }
        }

        return -1;
    }

    // Pre-transposed message words for the suffix digits: lane j of entry i holds suffix i * Lanes + j,
    // shifted to the byte offset where the suffix starts within its word.
    private static void BuildSuffixTable(Vector256<uint>[] low, Vector256<uint>[] high, int offset)
    {
        Span<uint> lowLanes = stackalloc uint[Lanes];
        Span<uint> highLanes = stackalloc uint[Lanes];

        for (var i = 0; i < low.Length; i++)
        {
            for (var lane = 0; lane < Lanes; lane++)
            {
                var suffix = i * Lanes + lane;
                var digits = (ulong)(uint)('0' + suffix / 1000)
                    | (ulong)(uint)('0' + suffix / 100 % 10) << 8
                    | (ulong)(uint)('0' + suffix / 10 % 10) << 16
                    | (ulong)(uint)('0' + suffix % 10) << 24;
                digits <<= 8 * offset;
                lowLanes[lane] = (uint)digits;
                highLanes[lane] = (uint)(digits >> 32);
            }

            low[i] = Vector256.Create<uint>(lowLanes);
            high[i] = Vector256.Create<uint>(highLanes);
        }
    }

    // Hashes 8 consecutive numbers at once, one per SIMD lane. Each lane's digits are incremented
    // in place, so only the message words that hold digits need to be reloaded into the vectors.
    // Returns the first hit, or 0 when no number below the limit matches.
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static int FindLowestSmall(byte[] key, uint firstWordMask, int limit)
    {
        Span<byte> blocks = stackalloc byte[64 * Lanes];
        Span<int> lengths = stackalloc int[Lanes];
        var words = MemoryMarshal.Cast<byte, uint>(blocks);
        Span<Vector256<uint>> m = stackalloc Vector256<uint>[16];
        Span<Vector256<uint>> kc = stackalloc Vector256<uint>[64];
        var mask = Vector256.Create(firstWordMask);
        var firstDigitWord = key.Length / 4;

        for (var step = 0; step < 64; step++)
        {
            kc[step] = Vector256.Create(K[step]);
        }

        for (var lane = 0; lane < Lanes; lane++)
        {
            var block = blocks.Slice(lane * 64, 64);
            key.CopyTo(block);
            lengths[lane] = WriteNumber(block, key.Length, 1 + lane);
        }

        var firstDirtyWord = 0;
        var lastDirtyWord = 15;

        for (var number = 1; number < limit; number += Lanes)
        {
            for (var i = firstDirtyWord; i <= lastDirtyWord; i++)
            {
                m[i] = Vector256.Create(
                    words[i], words[16 + i], words[32 + i], words[48 + i],
                    words[64 + i], words[80 + i], words[96 + i], words[112 + i]);
            }

            var a = Vector256.Create(A0);
            var b = Vector256.Create(B0);
            var c = Vector256.Create(C0);
            var d = Vector256.Create(D0);
            F(ref a, b, c, d, kc[0] + m[0], 7);
            F(ref d, a, b, c, kc[1] + m[1], 12);

            var hits = Vector256.Equals(Md5FirstWordFromStep2<AllWords>(a, b, c, d, kc, m, default, default) & mask, Vector256<uint>.Zero)
                .ExtractMostSignificantBits();
            if (hits != 0)
            {
                return number + BitOperations.TrailingZeroCount(hits);
            }

            firstDirtyWord = firstDigitWord;
            lastDirtyWord = firstDigitWord;
            for (var lane = 0; lane < Lanes; lane++)
            {
                var block = blocks.Slice(lane * 64, 64);
                if (IncrementDigits(block, key.Length, lengths[lane]))
                {
                    lastDirtyWord = Math.Max(lastDirtyWord, (lengths[lane] - 1) / 4);
                }
                else
                {
                    lengths[lane] = WriteNumber(block, key.Length, number + Lanes + lane);
                    lastDirtyWord = 15;
                }
            }
        }

        return 0;
    }

    private static int WriteNumber(Span<byte> block, int keyLength, int number)
    {
        number.TryFormat(block[keyLength..], out var written);
        var length = keyLength + written;
        block[length] = 0x80;
        block[(length + 1)..56].Clear();
        BinaryPrimitives.WriteUInt64LittleEndian(block[56..], (ulong)length * 8);
        return length;
    }

    // Adds Lanes to the decimal digits in place. Returns false when the number gains a digit.
    private static bool IncrementDigits(Span<byte> block, int start, int length)
    {
        var carry = Lanes;
        for (var i = length - 1; i >= start; i--)
        {
            var value = block[i] - '0' + carry;
            if (value < 10)
            {
                block[i] = (byte)('0' + value);
                return true;
            }
            block[i] = (byte)('0' + value - 10);
            carry = 1;
        }
        return false;
    }

    // Single-block MD5 that only computes the first output word, starting from the state after
    // step 2. kc holds the per-step constant plus any lane-invariant message word; m holds the
    // lane-varying message words (zero where the word is lane-invariant). The A register is last
    // updated in step 61, so the final three steps are skipped.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Vector256<uint> Md5FirstWordFromStep2<TLayout>(
        Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d,
        ReadOnlySpan<Vector256<uint>> kc, ReadOnlySpan<Vector256<uint>> m, Vector256<uint> lo, Vector256<uint> hi)
        where TLayout : struct, ILayout
    {
        F(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 2, 2), 17);
        F(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 3, 3), 22);
        F(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 4, 4), 7);
        F(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 5, 5), 12);
        F(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 6, 6), 17);
        F(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 7, 7), 22);
        F(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 8, 8), 7);
        F(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 9, 9), 12);
        F(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 10, 10), 17);
        F(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 11, 11), 22);
        F(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 12, 12), 7);
        F(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 13, 13), 12);
        F(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 14, 14), 17);
        F(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 15, 15), 22);

        G(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 16, 1), 5);
        G(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 17, 6), 9);
        G(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 18, 11), 14);
        G(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 19, 0), 20);
        G(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 20, 5), 5);
        G(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 21, 10), 9);
        G(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 22, 15), 14);
        G(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 23, 4), 20);
        G(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 24, 9), 5);
        G(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 25, 14), 9);
        G(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 26, 3), 14);
        G(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 27, 8), 20);
        G(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 28, 13), 5);
        G(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 29, 2), 9);
        G(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 30, 7), 14);
        G(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 31, 12), 20);

        H(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 32, 5), 4);
        H(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 33, 8), 11);
        H(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 34, 11), 16);
        H(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 35, 14), 23);
        H(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 36, 1), 4);
        H(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 37, 4), 11);
        H(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 38, 7), 16);
        H(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 39, 10), 23);
        H(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 40, 13), 4);
        H(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 41, 0), 11);
        H(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 42, 3), 16);
        H(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 43, 6), 23);
        H(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 44, 9), 4);
        H(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 45, 12), 11);
        H(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 46, 15), 16);
        H(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 47, 2), 23);

        I(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 48, 0), 6);
        I(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 49, 7), 10);
        I(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 50, 14), 15);
        I(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 51, 5), 21);
        I(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 52, 12), 6);
        I(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 53, 3), 10);
        I(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 54, 10), 15);
        I(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 55, 1), 21);
        I(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 56, 8), 6);
        I(ref d, a, b, c, X<TLayout>(kc, m, lo, hi, 57, 15), 10);
        I(ref c, d, a, b, X<TLayout>(kc, m, lo, hi, 58, 6), 15);
        I(ref b, c, d, a, X<TLayout>(kc, m, lo, hi, 59, 13), 21);
        I(ref a, b, c, d, X<TLayout>(kc, m, lo, hi, 60, 4), 6);

        return a + Vector256.Create(A0);
    }

    // Message input for a step. TLayout.LowWord is a JIT-time constant, so for lane-invariant words
    // (already folded into kc) the vector add disappears entirely.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> X<TLayout>(
        ReadOnlySpan<Vector256<uint>> kc, ReadOnlySpan<Vector256<uint>> m, Vector256<uint> lo, Vector256<uint> hi, int step, int index)
        where TLayout : struct, ILayout =>
        TLayout.LowWord < 0 ? kc[step] + m[index]
        : index == TLayout.LowWord ? kc[step] + lo
        : index == TLayout.LowWord + 1 ? kc[step] + hi
        : kc[step];

    private interface ILayout
    {
        static abstract int LowWord { get; }
    }

    private struct AllWords : ILayout { public static int LowWord => -1; }
    private struct SuffixAt2 : ILayout { public static int LowWord => 2; }
    private struct SuffixAt3 : ILayout { public static int LowWord => 3; }
    private struct SuffixAt4 : ILayout { public static int LowWord => 4; }
    private struct SuffixAt5 : ILayout { public static int LowWord => 5; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void F(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> x, int s) =>
        a = b + RotateLeft(a + ((b & c) | (~b & d)) + x, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void G(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> x, int s) =>
        a = b + RotateLeft(a + ((b & d) | (c & ~d)) + x, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void H(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> x, int s) =>
        a = b + RotateLeft(a + (b ^ c ^ d) + x, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void I(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> x, int s) =>
        a = b + RotateLeft(a + (c ^ (b | ~d)) + x, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> RotateLeft(Vector256<uint> x, int s) =>
        Vector256.ShiftLeft(x, s) | Vector256.ShiftRightLogical(x, 32 - s);

    public override string Input => @"yzbqklnj";
}
