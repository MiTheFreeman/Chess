// *****************************************************
// *                                                   *
// * O Lord, Thank you for your goodness in our lives. *
// *     Please bless this code to our compilers.      *
// *                     Amen.                         *
// *                                                   *
// *****************************************************
//                                    Made by Geras1mleo

namespace Chess;

/// <summary>
/// https://www.chessprogramming.org/Repetitions
/// </summary>
internal class RepetitionRule : EndGameRule
{
    private const int MINIMUM_MOVES_COUNT = 8; // at least 8 moves required to get threefold repetition

    public RepetitionRule(ChessBoard board) : base(board) { }

    internal override EndgameType Type => EndgameType.Repetition;

    internal override bool IsEndGame()
    {
        bool isRepetition = false;
        var movesCount = board.MoveIndex + 1;

        if (movesCount >= MINIMUM_MOVES_COUNT
        && board.LastIrreversibleMoveIndex <= board.MoveIndex - MINIMUM_MOVES_COUNT) // If last 8 moves were reversible
        {
            var currentIndex = board.MoveIndex;

            HashSet<ChessBoard> piecesPositions = new HashSet<ChessBoard>(new ChessBoardComparer());
            piecesPositions.Add(board.Clone());

            board.MoveIndex -= MINIMUM_MOVES_COUNT;

            piecesPositions.Add(board.Clone());

            if (piecesPositions.Count == 1)
            {
                board.MoveIndex += (MINIMUM_MOVES_COUNT / 2);

                piecesPositions.Add(board.Clone());
            }

            board.MoveIndex = currentIndex; // Setting back to original positions

            isRepetition = piecesPositions.Count == 1;
        }

        return isRepetition;
    }
}

internal class ChessBoardComparer : IEqualityComparer<ChessBoard>
{
    public bool Equals(ChessBoard? x, ChessBoard? y)
    {
        bool isEqual = false;

        if (x is null && y is null)
        {
            isEqual = true;
        }
        else if (x is not null && y is not null)
        {
            isEqual = true;

            for (int i = 0; i < 8 * 8 && isEqual; i++)
            {
                if (x.pieces[i] is null != y.pieces[i] is null)
                    isEqual = false;

                if (x.pieces[i] is not null && y.pieces[i] is not null)
                {
                    isEqual = x.pieces[i].Color == y.pieces[i].Color && x.pieces[i].Type == y.pieces[i].Type;
                }
            }

            isEqual &= ChessBoard.HasRightToCastle(PieceColor.White, CastleType.King, x) == ChessBoard.HasRightToCastle(PieceColor.White, CastleType.King, y);
            isEqual &= ChessBoard.HasRightToCastle(PieceColor.White, CastleType.Queen, x) == ChessBoard.HasRightToCastle(PieceColor.White, CastleType.Queen, y);
            isEqual &= ChessBoard.HasRightToCastle(PieceColor.Black, CastleType.King, x) == ChessBoard.HasRightToCastle(PieceColor.Black, CastleType.King, y);
            isEqual &= ChessBoard.HasRightToCastle(PieceColor.Black, CastleType.Queen, x) == ChessBoard.HasRightToCastle(PieceColor.Black, CastleType.Queen, y);

            isEqual &= ChessBoard.LastMoveEnPassantPosition(x) == ChessBoard.LastMoveEnPassantPosition(y);
        }
        else
        {
            isEqual = false;
        }

        return isEqual;
    }

    public int GetHashCode([DisallowNull] ChessBoard obj)
    {
        return 0;
    }
}
