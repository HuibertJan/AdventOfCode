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

    // Hashes 8 consecutive numbers at once, one per SIMD lane (still a single core).
    // Each lane's digits are incremented in place, so only the message words that hold
    // digits need to be reloaded into the vectors.
    private int FindLowestNumber(uint firstWordMask)
    {
        var key = Encoding.ASCII.GetBytes(Input);
        Span<byte> blocks = stackalloc byte[64 * Lanes];
        Span<int> lengths = stackalloc int[Lanes];
        var words = MemoryMarshal.Cast<byte, uint>(blocks);
        Span<Vector256<uint>> m = stackalloc Vector256<uint>[16];
        var mask = Vector256.Create(firstWordMask);
        var firstDigitWord = key.Length / 4;

        for (var lane = 0; lane < Lanes; lane++)
        {
            var block = blocks.Slice(lane * 64, 64);
            key.CopyTo(block);
            lengths[lane] = WriteNumber(block, key.Length, 1 + lane);
        }

        var firstDirtyWord = 0;
        var lastDirtyWord = 15;

        for (var number = 1; ; number += Lanes)
        {
            for (var i = firstDirtyWord; i <= lastDirtyWord; i++)
            {
                m[i] = Vector256.Create(
                    words[i], words[16 + i], words[32 + i], words[48 + i],
                    words[64 + i], words[80 + i], words[96 + i], words[112 + i]);
            }

            var hits = Vector256.Equals(Md5FirstWord(m) & mask, Vector256<uint>.Zero).ExtractMostSignificantBits();
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

    // Single-block MD5 that only computes the first output word. The A register is last
    // updated in step 61, so the final three steps are skipped.
    private static Vector256<uint> Md5FirstWord(ReadOnlySpan<Vector256<uint>> m)
    {
        var a = Vector256.Create(0x67452301u);
        var b = Vector256.Create(0xefcdab89u);
        var c = Vector256.Create(0x98badcfeu);
        var d = Vector256.Create(0x10325476u);

        F(ref a, b, c, d, m[0], 7, 0xd76aa478);
        F(ref d, a, b, c, m[1], 12, 0xe8c7b756);
        F(ref c, d, a, b, m[2], 17, 0x242070db);
        F(ref b, c, d, a, m[3], 22, 0xc1bdceee);
        F(ref a, b, c, d, m[4], 7, 0xf57c0faf);
        F(ref d, a, b, c, m[5], 12, 0x4787c62a);
        F(ref c, d, a, b, m[6], 17, 0xa8304613);
        F(ref b, c, d, a, m[7], 22, 0xfd469501);
        F(ref a, b, c, d, m[8], 7, 0x698098d8);
        F(ref d, a, b, c, m[9], 12, 0x8b44f7af);
        F(ref c, d, a, b, m[10], 17, 0xffff5bb1);
        F(ref b, c, d, a, m[11], 22, 0x895cd7be);
        F(ref a, b, c, d, m[12], 7, 0x6b901122);
        F(ref d, a, b, c, m[13], 12, 0xfd987193);
        F(ref c, d, a, b, m[14], 17, 0xa679438e);
        F(ref b, c, d, a, m[15], 22, 0x49b40821);

        G(ref a, b, c, d, m[1], 5, 0xf61e2562);
        G(ref d, a, b, c, m[6], 9, 0xc040b340);
        G(ref c, d, a, b, m[11], 14, 0x265e5a51);
        G(ref b, c, d, a, m[0], 20, 0xe9b6c7aa);
        G(ref a, b, c, d, m[5], 5, 0xd62f105d);
        G(ref d, a, b, c, m[10], 9, 0x02441453);
        G(ref c, d, a, b, m[15], 14, 0xd8a1e681);
        G(ref b, c, d, a, m[4], 20, 0xe7d3fbc8);
        G(ref a, b, c, d, m[9], 5, 0x21e1cde6);
        G(ref d, a, b, c, m[14], 9, 0xc33707d6);
        G(ref c, d, a, b, m[3], 14, 0xf4d50d87);
        G(ref b, c, d, a, m[8], 20, 0x455a14ed);
        G(ref a, b, c, d, m[13], 5, 0xa9e3e905);
        G(ref d, a, b, c, m[2], 9, 0xfcefa3f8);
        G(ref c, d, a, b, m[7], 14, 0x676f02d9);
        G(ref b, c, d, a, m[12], 20, 0x8d2a4c8a);

        H(ref a, b, c, d, m[5], 4, 0xfffa3942);
        H(ref d, a, b, c, m[8], 11, 0x8771f681);
        H(ref c, d, a, b, m[11], 16, 0x6d9d6122);
        H(ref b, c, d, a, m[14], 23, 0xfde5380c);
        H(ref a, b, c, d, m[1], 4, 0xa4beea44);
        H(ref d, a, b, c, m[4], 11, 0x4bdecfa9);
        H(ref c, d, a, b, m[7], 16, 0xf6bb4b60);
        H(ref b, c, d, a, m[10], 23, 0xbebfbc70);
        H(ref a, b, c, d, m[13], 4, 0x289b7ec6);
        H(ref d, a, b, c, m[0], 11, 0xeaa127fa);
        H(ref c, d, a, b, m[3], 16, 0xd4ef3085);
        H(ref b, c, d, a, m[6], 23, 0x04881d05);
        H(ref a, b, c, d, m[9], 4, 0xd9d4d039);
        H(ref d, a, b, c, m[12], 11, 0xe6db99e5);
        H(ref c, d, a, b, m[15], 16, 0x1fa27cf8);
        H(ref b, c, d, a, m[2], 23, 0xc4ac5665);

        I(ref a, b, c, d, m[0], 6, 0xf4292244);
        I(ref d, a, b, c, m[7], 10, 0x432aff97);
        I(ref c, d, a, b, m[14], 15, 0xab9423a7);
        I(ref b, c, d, a, m[5], 21, 0xfc93a039);
        I(ref a, b, c, d, m[12], 6, 0x655b59c3);
        I(ref d, a, b, c, m[3], 10, 0x8f0ccc92);
        I(ref c, d, a, b, m[10], 15, 0xffeff47d);
        I(ref b, c, d, a, m[1], 21, 0x85845dd1);
        I(ref a, b, c, d, m[8], 6, 0x6fa87e4f);
        I(ref d, a, b, c, m[15], 10, 0xfe2ce6e0);
        I(ref c, d, a, b, m[6], 15, 0xa3014314);
        I(ref b, c, d, a, m[13], 21, 0x4e0811a1);
        I(ref a, b, c, d, m[4], 6, 0xf7537e82);

        return a + Vector256.Create(0x67452301u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void F(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> m, int s, uint k) =>
        a = b + RotateLeft(a + ((b & c) | (~b & d)) + Vector256.Create(k) + m, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void G(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> m, int s, uint k) =>
        a = b + RotateLeft(a + ((b & d) | (c & ~d)) + Vector256.Create(k) + m, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void H(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> m, int s, uint k) =>
        a = b + RotateLeft(a + (b ^ c ^ d) + Vector256.Create(k) + m, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void I(ref Vector256<uint> a, Vector256<uint> b, Vector256<uint> c, Vector256<uint> d, Vector256<uint> m, int s, uint k) =>
        a = b + RotateLeft(a + (c ^ (b | ~d)) + Vector256.Create(k) + m, s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> RotateLeft(Vector256<uint> x, int s) =>
        Vector256.ShiftLeft(x, s) | Vector256.ShiftRightLogical(x, 32 - s);

    public override string Input => @"yzbqklnj";
}
