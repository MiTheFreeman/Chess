using Chess;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace ChessUnitTests;

public class AnalyseTests
{
    [Fact]
    public async Task TestAnalyse()
    {
        ChessBoard board = new ChessBoard();

        var analyses = board.Analyses();
        Assert.Equal(40, analyses.AllMoves.Length);

        var p = analyses[0, 0]; // rook
        Assert.Equal(0, p.PieceMoves.Length);
        Assert.Equal(0, p.DefendThisPosition.Length);

        p = analyses[1, 0]; // night
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[2, 0]; // bishop
        Assert.Equal(0, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[3, 0]; // queen
        Assert.Equal(0, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[4, 0]; // king
        Assert.Equal(0, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[5, 0]; // bishop
        Assert.Equal(0, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[6, 0]; // night
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[7, 0]; // rook
        Assert.Equal(0, p.PieceMoves.Length);
        Assert.Equal(0, p.DefendThisPosition.Length);



        p = analyses[0, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[1, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[2, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[3, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(4, p.DefendThisPosition.Length);

        p = analyses[4, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(4, p.DefendThisPosition.Length);

        p = analyses[5, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[6, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);

        p = analyses[7, 1]; // pawn
        Assert.Equal(2, p.PieceMoves.Length);
        Assert.Equal(1, p.DefendThisPosition.Length);



        p = analyses[0, 2]; // empty
        Assert.Equal(2, p.DefendThisPosition.Length);
        Assert.Equal(2, p.MoveToThisPosition.Length);

        p = analyses[1, 2]; // empty
        Assert.Equal(2, p.DefendThisPosition.Length);
        Assert.Equal(1, p.MoveToThisPosition.Length);

        p = analyses[2, 2]; // empty
        Assert.Equal(3, p.DefendThisPosition.Length);
        Assert.Equal(2, p.MoveToThisPosition.Length);

        p = analyses[3, 2]; // empty
        Assert.Equal(2, p.DefendThisPosition.Length);
        Assert.Equal(1, p.MoveToThisPosition.Length);

        p = analyses[4, 2]; // empty
        Assert.Equal(2, p.DefendThisPosition.Length);
        Assert.Equal(1, p.MoveToThisPosition.Length);

        p = analyses[5, 2]; // empty
        Assert.Equal(3, p.DefendThisPosition.Length);
        Assert.Equal(2, p.MoveToThisPosition.Length);

        p = analyses[6, 2]; // empty
        Assert.Equal(2, p.DefendThisPosition.Length);
        Assert.Equal(1, p.MoveToThisPosition.Length);

        p = analyses[7, 2]; // empty
        Assert.Equal(2, p.DefendThisPosition.Length);
        Assert.Equal(2, p.MoveToThisPosition.Length);
    }
}