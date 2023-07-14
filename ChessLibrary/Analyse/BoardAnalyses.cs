using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Analyse
{
    public class BoardAnalyses
    {
        private BoardPosition[] positions;

        public BoardPosition? this[short x, short y] => positions.First(p => p.Position.X == x && p.Position.Y == y);
        public BoardPosition? this[short pos] => positions.First(p => p.Position.P == pos);

        public BoardAnalyses(ChessBoard board)
        {
            positions = Enumerable.Range(0, 8 * 8).Select(i => new BoardPosition((short)i, board[(short)i])).ToArray();
        }

        public Move[] AllMoves { get; private set; }


        internal void SetMoves(Move[] allMoves)
        {
            AllMoves = allMoves;

            foreach (var move in AllMoves)
            {
                positions[move.OriginalPosition.P].AddMoveFrom(move);
                positions[move.NewPosition.P].AddMoveTo(move);
            }
        }

        internal void SetDefend(Move[] moves)
        {
            foreach (var move in moves)
            {
                positions[move.NewPosition.P].AddDefended(move);
            }
        }
    }

    public class BoardPosition
    {
        private List<Move> pieceMoves = new List<Move>();
        private List<Move> moveToThisPosition = new List<Move>();
        private List<Move> defendThisPosition = new List<Move>();

        internal BoardPosition(short positionNr, Piece? piece)
        {
            Position = new Position(positionNr);
            Piece = piece;
        }

        public Position Position { get; }

        public Piece? Piece { get; set; }

        public Move[] PieceMoves => pieceMoves.ToArray();
        public Move[] MoveToThisPosition => moveToThisPosition.ToArray();
        public Move[] DefendThisPosition => defendThisPosition.ToArray();

        internal void AddMoveFrom(Move move)
        {
            pieceMoves.Add(move);
        }
        internal void AddMoveTo(Move move)
        {
            moveToThisPosition.Add(move);
            if(move.Piece.Type != PieceType.Pawn)
            {
                defendThisPosition.Add(move);
            }
        }
        internal void AddDefended(Move move)
        {
            defendThisPosition.Add(move);
        }
    }
}
