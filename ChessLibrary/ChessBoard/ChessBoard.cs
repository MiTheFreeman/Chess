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
/// Chess board logic
/// </summary>
public partial class ChessBoard
{
    internal Piece?[] pieces;

    /// <summary>
    /// Returns Piece on given position
    /// </summary>
    /// <param name="pos">Position on chess board</param>
    public Piece? this[Position pos] => pieces[pos.P];

    /// <summary>
    /// Returns Piece on given position
    /// </summary>
    /// <param name="x">a->h</param>
    /// <param name="y">1->8</param>
    public Piece? this[char x, short y] => this[new Position(x.ToString() + y.ToString())];

    /// <summary>
    /// Returns Piece on given position
    /// </summary>
    /// <param name="s">
    /// Position as string<br/>
    /// ex.: e4 / a1 / h8 etc.
    /// </param>
    public Piece? this[string s] => this[new Position(s)];

    /// <summary>
    /// Returns Piece on given position
    /// </summary>
    /// <param name="x">0->8</param>
    /// <param name="y">0->8</param>
    public Piece? this[short x, short y] => pieces[x + y * 8];

    internal readonly Dictionary<string, string> headers;

    /// <summary>
    /// Headers of current chess board
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers => new Dictionary<string, string>(headers);

    internal FenBoardBuilder? FenBuilder;
    /// <summary>
    /// Whether board has been loaded from Forsyth-Edwards Notation
    /// </summary>
    public bool LoadedFromFen => FenBuilder is not null;

    /// <summary>
    /// Get all pieces on the board
    /// </summary>
    /// <returns></returns>
    public Piece[] GetPieces() => pieces.Cast<Piece>().Where(p => p != null).ToArray();

    /// <summary>
    /// Get all pieces on the board
    /// </summary>
    /// <returns></returns>
    public Dictionary<Position, Piece> GetPiecesWithPosition()
    {
        Dictionary<Position, Piece> dict = new Dictionary<Position, Piece>();

        for (short x = 0; x < 8; x++)
        {
            for (short y = 0; y < 8; y++)
            {
                var piece = this[x, y];
                if (piece != null)
                {
                    dict.Add(new Position(x, y), piece);
                }
            }
        }

        return dict;
    }

    /// <summary>
    /// Determinize whose player turn is it now
    /// </summary>
    public PieceColor Turn
    {
        get
        {
            if (LoadedFromFen)
                return DisplayedMoves.Count % 2 == 0 ? FenBuilder.Turn : FenBuilder.Turn.OppositeColor();
            else
                return DisplayedMoves.Count % 2 == 0 ? PieceColor.White : PieceColor.Black;

        }
    }

    private bool whiteKingChecked = false;
    /// <summary>
    /// Returns the state of White king (Checked or not)
    /// </summary>
    public bool WhiteKingChecked
    {
        get => whiteKingChecked;
        private set
        {
            if (value != whiteKingChecked)
            {
                whiteKingChecked = value;
                OnWhiteKingCheckedChangedEvent(new CheckEventArgs(this, WhiteKing, value));
            }
        }
    }

    private bool blackKingChecked = false;
    /// <summary>
    /// Returns the state of Black king (Checked or not)
    /// </summary>
    public bool BlackKingChecked
    {
        get => blackKingChecked;
        private set
        {
            if (value != blackKingChecked)
            {
                blackKingChecked = value;
                OnBlackKingCheckedChangedEvent(new CheckEventArgs(this, BlackKing, value));
            }
        }
    }

    /// <summary>
    /// Returns White king position on chess board
    /// </summary>
    public Position WhiteKing => GetKingPosition(PieceColor.White, this);
    /// <summary>
    /// Returns Black king position on chess board
    /// </summary>
    public Position BlackKing => GetKingPosition(PieceColor.Black, this);

    /// <summary>
    /// White pieces that has been captured by black player
    /// </summary>
    public Piece[] CapturedWhite
    {
        get
        {
            var captured = new List<Piece>();

            if (LoadedFromFen)
                captured.AddRange(FenBuilder!.WhiteCaptured);

            captured.AddRange(DisplayedMoves.Where(m => m.CapturedPiece?.Color == PieceColor.White)
                                            .Select(m => new Piece(m.CapturedPiece.Color, m.CapturedPiece.Type)));

            return captured.ToArray();
        }
    }

    /// <summary>
    /// Black pieces that has been captured by white player
    /// </summary>
    public Piece[] CapturedBlack
    {
        get
        {
            var captured = new List<Piece>();

            if (LoadedFromFen)
                captured.AddRange(FenBuilder!.BlackCaptured);

            captured.AddRange(DisplayedMoves.Where(m => m.CapturedPiece?.Color == PieceColor.Black)
                                            .Select(m => new Piece(m.CapturedPiece.Color, m.CapturedPiece.Type)));

            return captured.ToArray();
        }
    }

    private EndGameInfo? endGame;
    /// <summary>
    /// Represents End of game state(or null), won side(or null if draw) and type of end game
    /// </summary>
    public EndGameInfo? EndGame
    {
        get => endGame;
        private set
        {
            endGame = value;
            if (value is not null) OnEndGameEvent();
        }
    }
    /// <summary>
    /// When true => use: EndGame. for more info on endgame(type, won side)
    /// </summary>
    public bool IsEndGame => EndGame is not null;

    internal readonly List<Move> executedMoves;
    /// <summary>
    /// Executed moves on this chess board<br/>
    /// </summary>
    public IReadOnlyList<Move> ExecutedMoves => new List<Move>(executedMoves);

    /// <summary>
    /// Executed moves in SAN
    /// </summary>
    public List<string> MovesToSan => new List<Move>(executedMoves).Select(m => m.San).ToList();

    internal int moveIndex = -1;
    /// <summary>
    /// Index of displayed move on this chess board
    /// </summary>
    public int MoveIndex
    {
        get => moveIndex;
        set
        {
            if (value < executedMoves.Count && value >= -1)
                DisplayMoves(executedMoves.GetRange(0, value + 1));
            else
                throw new IndexOutOfRangeException("Move not found");
        }
    }

    /// <summary>
    /// Whether last move is displayed on this chess board<br/>
    /// False after Previous() / First() / MoveIndex = ...
    /// </summary>
    public bool IsLastMoveDisplayed => moveIndex == executedMoves.Count - 1;

    internal List<Move> DisplayedMoves => executedMoves.GetRange(0, moveIndex + 1);

    /// <summary>
    /// https://www.chessprogramming.org/Irreversible_Moves
    /// </summary>
    public int LastIrreversibleMoveIndex
    {
        get
        {
            int index = moveIndex;
            bool moveFound = false;

            while (index >= 0 && !moveFound)
            {
                if (executedMoves[index].CapturedPiece is not null
                 || executedMoves[index].Piece.Type == PieceType.Pawn)
                {
                    moveFound = true;
                }

                index--;
            }

            return moveFound ? index + 1 : index;
        }
    }

    private AutoEndgameRules autoEndgameRules = AutoEndgameRules.None;

    /// <summary>
    /// This property keeps track of auto-draw (endgame) rules that will be used to check for endgame
    /// </summary>
    public AutoEndgameRules AutoEndgameRules { get => autoEndgameRules; set => autoEndgameRules = value; }

    /// <summary>
    /// Creates new chess board with default pieces positions
    /// </summary>
    public ChessBoard()
    {
        executedMoves = new List<Move>();
        headers = new Dictionary<string, string>();
        SetChessBeginSituation();
    }

    /// <summary>
    /// To execute operations and to not corrupt the main chess object
    /// </summary>
    private ChessBoard(Piece?[] pieces, List<Move> moves)
    {
        executedMoves = new List<Move>(moves);
        this.pieces = (Piece[])pieces.Clone();
    }

    /// <summary>
    /// Converts SAN move into Move object and performs it on chess board
    /// </summary>
    /// <param name="san">Chess move in SAN</param>
    public bool Move(string san)
    {
        return Move(ParseFromSan(san, false));
    }

    /// <summary>
    /// Validates the move and performs it on chess board
    /// </summary>
    /// <param name="move">Move that will be validated and performed</param>
    /// <returns>Validation succeed and move is performed => true</returns>
    /// <exception cref="ArgumentNullException">Move is null</exception>
    /// <exception cref="ChessGameEndedException">Game Ended</exception>
    /// <exception cref="ChessInvalidMoveException">Currently displaying one of previous moves, use first board.DisplayLastMove();</exception>
    /// <exception cref="ChessPieceNotFoundException">Piece on given position not found</exception>
    public bool Move(Move move)
    {
        if (IsEndGame)
            throw new ChessGameEndedException(this, EndGame);

        if (!IsLastMoveDisplayed)
            throw new ChessInvalidMoveException(this, "Please use board.DisplayLastMove(); to be able to perform new moves in this chess game", move);

        if (IsValidMove(move, this, true, true))
        {
            if (move.San is null)
                ParseToSan(move);

            executedMoves.Add(move);

            if (move.CapturedPiece is not null)
                OnCapturedEvent(move.CapturedPiece);

            DropPieceToNewPosition(move);

            moveIndex = executedMoves.Count - 1;

            HandleKingChecked();
            HandleEndGame();

            return true;
        }
        else return false;
    }

    /// <summary>
    /// Adding header to this chess game<br/>
    /// ex.:<br/>
    /// name => Black, value => Geras1mleo<br/>
    /// Pgn Output: [Black "Geras1mleo"]
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    public void AddHeader(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        if (name.ToLower() == "fen")
            throw new ChessArgumentException(this, "To load game from fen please use: board.LoadFen();");

        headers.Add(name, value);
    }

    /// <summary>
    /// Removing header from this chess game
    /// </summary>
    /// <param name="name">Header name</param>
    public void RemoveHeader(string name)
    {
        if (name.ToLower() == "fen")
            throw new ChessArgumentException(this, "Could not remove FEN header from current game: FEN header required when loaded from FEN...");

        headers.Remove(name);
    }

    // Temporary disabled
    /// <summary>
    /// Puts given piece on given position<br/>
    /// Warning! Checked state and end game state is not being updated
    /// </summary>
    public void Put(Piece piece, Position position)
    {
        pieces[position.P] = piece;
    }

    /// <summary>
    /// Removes a piece on given position from board<br/>
    /// Warning! Checked state and end game state is not being updated
    /// </summary>
    public void Remove(Position position)
    {
        pieces[position.P] = null;
    }

    /// <summary>
    /// Clears board and sets begin positions
    /// </summary>
    public void Clear()
    {
        SetChessBeginSituation();
        executedMoves.Clear();
        headers.Clear();
        moveIndex = -1;
        endGame = null;
        FenBuilder = null;
        WhiteKingChecked = false;
        BlackKingChecked = false;
    }

    /// <summary>
    /// Restores board according to moves and/or positions loaded from fen<br/>
    /// Similar to Last()
    /// </summary>
    public void Restore()
    {
        DisplayMoves(executedMoves);
    }

    /// <summary>
    /// Cancel last move and display previous pieces positions
    /// </summary>
    public void Cancel()
    {
        if (IsLastMoveDisplayed && executedMoves.Count > 0)
        {
            var move = executedMoves[^1];

            if (move.Parameter is not null)
                move.Parameter.Undo(move, this);
            else
                RestorePiece(move, this);

            executedMoves.RemoveAt(executedMoves.Count - 1);
            moveIndex = executedMoves.Count - 1;

            HandleKingChecked();
            EndGame = null;
        }
    }

    /// <summary>
    /// Displaying first move(if possible)
    /// </summary>
    public void First() => MoveIndex = 0;

    /// <summary>
    /// Displaying last move(if possible)
    /// </summary>
    public void Last() => MoveIndex = executedMoves.Count - 1;

    /// <summary>
    /// Displaying next move(if possible)
    /// </summary>
    public void Next() => MoveIndex++;

    /// <summary>
    /// Displaying previous move(if possible)
    /// </summary>
    public void Previous() => MoveIndex--;

    /// <summary>
    /// Declares resign of one of sides
    /// </summary>
    /// <param name="resignedSide">Resigned side</param>
    /// <exception cref="ChessGameEndedException"></exception>
    public void Resign(PieceColor resignedSide)
    {
        if (IsEndGame)
            throw new ChessGameEndedException(this, EndGame);

        EndGame = new EndGameInfo(EndgameType.Resigned, resignedSide.OppositeColor());
    }

    /// <summary>
    /// Declares draw in current chess game
    /// </summary>
    /// <exception cref="ChessGameEndedException"></exception>
    public void Draw()
    {
        if (IsEndGame)
            throw new ChessGameEndedException(this, EndGame);

        EndGame = new EndGameInfo(EndgameType.DrawDeclared, null);
    }

    internal void DropPieceToNewPosition(Move move)
    {
        if (move.Parameter is not null)
        {
            move.Parameter.Execute(move, this);
            return;
        }

        DropPiece(move, this);
    }

    /// <summary>
    /// Default dropping piece implementation<br/>
    /// Clearing old, copy to new...
    /// </summary>
    internal static void DropPiece(Move move, ChessBoard board)
    {
        // Moving piece to its new position
        board.pieces[move.NewPosition.P] = new(board.pieces[move.OriginalPosition.P].Color, board.pieces[move.OriginalPosition.P].Type);

        // Clearing old position
        board.pieces[move.OriginalPosition.P] = null;
    }

    internal static void RestorePiece(Move move, ChessBoard board)
    {
        // Moving piece to its original position
        board.pieces[move.OriginalPosition.P] = new(board.pieces[move.NewPosition.P].Color, board.pieces[move.NewPosition.P].Type);

        // Clearing new position / or setting captured piece back
        board.pieces[move.NewPosition.P] = move.CapturedPiece;
    }

    private void DisplayMoves(List<Move> moves)
    {
        if (LoadedFromFen)
            pieces = FenBuilder.Pieces;
        else
            SetChessBeginSituation();

        for (int i = 0; i < moves.Count; i++)
            DropPieceToNewPosition(moves[i]);

        moveIndex = moves.Count - 1;

        HandleKingChecked();
    }

    /// <summary>
    /// Checking if there one of kings are checked
    /// and updating checked states
    /// </summary>
    internal void HandleKingChecked()
    {
        WhiteKingChecked = false;
        BlackKingChecked = false;

        if (moveIndex >= 0 && executedMoves[moveIndex].IsCheck)
        {
            if (executedMoves[moveIndex].Piece.Color == PieceColor.White)
                BlackKingChecked = true;

            else if (executedMoves[moveIndex].Piece.Color == PieceColor.Black)
                WhiteKingChecked = true;
        }
        else if (LoadedFromFen)
        {
            WhiteKingChecked = IsKingChecked(PieceColor.White, this);
            BlackKingChecked = IsKingChecked(PieceColor.Black, this);
        }
    }

    /// <summary>
    /// Checking if there is a checkmate or stalemate
    /// and updating end game state
    /// </summary>
    internal void HandleEndGame()
    {
        EndGame = new EndGameProvider(this).GetEndGameInfo();
    }

    /// <summary>
    /// Slow clone
    /// </summary>
    /// <returns></returns>
    public ChessBoard Clone()
    {
        return new ChessBoard(pieces, DisplayedMoves) { FenBuilder = FenBuilder, moveIndex = MoveIndex };
    }

    private void SetChessBeginSituation()
    {
        pieces = new Piece[8 * 8];

        pieces[0] = new Piece(PieceColor.White, PieceType.Rook);
        pieces[1] = new Piece(PieceColor.White, PieceType.Knight);
        pieces[2] = new Piece(PieceColor.White, PieceType.Bishop);
        pieces[3] = new Piece(PieceColor.White, PieceType.Queen);
        pieces[4] = new Piece(PieceColor.White, PieceType.King);
        pieces[5] = new Piece(PieceColor.White, PieceType.Bishop);
        pieces[6] = new Piece(PieceColor.White, PieceType.Knight);
        pieces[7] = new Piece(PieceColor.White, PieceType.Rook);

        pieces[8 * 1 + 0] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 1] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 2] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 3] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 4] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 5] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 6] = new Piece(PieceColor.White, PieceType.Pawn);
        pieces[8 * 1 + 7] = new Piece(PieceColor.White, PieceType.Pawn);

        pieces[8 * 6 + 0] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 1] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 2] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 3] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 4] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 5] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 6] = new Piece(PieceColor.Black, PieceType.Pawn);
        pieces[8 * 6 + 7] = new Piece(PieceColor.Black, PieceType.Pawn);

        pieces[8 * 7 + 0] = new Piece(PieceColor.Black, PieceType.Rook);
        pieces[8 * 7 + 1] = new Piece(PieceColor.Black, PieceType.Knight);
        pieces[8 * 7 + 2] = new Piece(PieceColor.Black, PieceType.Bishop);
        pieces[8 * 7 + 3] = new Piece(PieceColor.Black, PieceType.Queen);
        pieces[8 * 7 + 4] = new Piece(PieceColor.Black, PieceType.King);
        pieces[8 * 7 + 5] = new Piece(PieceColor.Black, PieceType.Bishop);
        pieces[8 * 7 + 6] = new Piece(PieceColor.Black, PieceType.Knight);
        pieces[8 * 7 + 7] = new Piece(PieceColor.Black, PieceType.Rook);
    }
}
