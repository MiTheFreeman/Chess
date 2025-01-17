﻿// *****************************************************
// *                                                   *
// * O Lord, Thank you for your goodness in our lives. *
// *     Please bless this code to our compilers.      *
// *                     Amen.                         *
// *                                                   *
// *****************************************************
//                                    Made by Geras1mleo

namespace Chess;

internal static class Extensions
{
    /// <summary>
    /// See: https://stackoverflow.com/questions/49190830/is-it-possible-for-string-split-to-return-tuple
    /// Deconstruct into 2 vars
    /// </summary>
    internal static void Deconstruct<T>(this IList<T> list, out T first, out T second)
    {
        first = list.Count > 0 ? list[0] : default; // or throw
        second = list.Count > 1 ? list[1] : default; // or throw
    }

    internal static List<Piece> PiecesList(this Piece?[] pieces)
    {
        var list = new List<Piece>();

        for (int i = 0; i < 8 * 8; i++)
        {
            if (pieces[i] is not null)
                list.Add(pieces[i]);
        }

        return list;
    }

    public static Span<Piece> PiecesSpan(this Piece[] pieces)
    {
        int nonNullCount = 0;
        for (int i = 0; i < 8 * 8; i++)
        {
            if (pieces[i] != null)
            {
                nonNullCount++;
            }
        }

        var piecesFlat = new Piece[nonNullCount];
        int offset = 0;
        for (int i = 0; i < 8 && offset < piecesFlat.Length; i++)
        {
            for (int j = 0; j < 8 && offset < piecesFlat.Length; j++)
            {
                if (pieces[j + i * 8] != null)
                {
                    piecesFlat[offset++] = pieces[j + i * 8];
                }
            }
        }

        return new Span<Piece>(piecesFlat);
    }


    public static int InsertSpan(this Span<char> span, int offset, ReadOnlySpan<char> source)
    {
        for (int i = 0; i < source.Length; i++)
        {
            span[offset++] = source[i];
        }
        return offset;
    }
}