// *****************************************************
// *                                                   *
// * O Lord, Thank you for your goodness in our lives. *
// *     Please bless this code to our compilers.      *
// *                     Amen.                         *
// *                                                   *
// *****************************************************
//                                    Made by Geras1mleo

using Chess.Analyse;

namespace Chess;

public partial class ChessBoard
{
    /// <summary>
    /// Returns all moves that the piece on given position can perform
    /// </summary>
    /// <param name="piecePosition">Position of piece</param>
    /// <param name="allowAmbiguousCastle">Whether Castle move will be e1-g1 AND also e1-h1 which is in fact the same O-O</param>
    /// <param name="generateSan">Whether SAN notation needs to be generated. For higher productivity => set to false</param>
    /// <param name="checkTurn">Only show moves for current turn color</param>
    /// <returns>All available moves for given piece</returns>
    public Move[] Moves(Position piecePosition, bool allowAmbiguousCastle = false, bool generateSan = true, bool checkTurn = true)
    {
        if (pieces[piecePosition.P] is null)
            throw new ChessPieceNotFoundException(this, piecePosition);

        var moves = new List<Move>();
        var positions = GeneratePositions(piecePosition, this);

        foreach (var position in positions)
        {
            Move move = new(piecePosition, position);

            if (!IsValidMove(move, this, false, checkTurn))
            {
                continue;
            }

            // Ambiguous castle
            if (!allowAmbiguousCastle && move.Parameter is MoveCastle)
            {
                if (move.NewPosition.X % 7 == 0) // Dropping king on position of rook 
                    continue;
            }

            // If promotion => 4 different moves for each promotion type
            if (move.Parameter is MovePromotion promotion)
            {
                AddPromotionMoves(moves, move, generateSan, promotion.PromotionType);
            }
            else
            {
                moves.Add(move);

                if (generateSan)
                {
                    ParseToSan(move);
                }
            }
        }

        return moves.ToArray();
    }

    /// <summary>
    /// Returns all moves that the piece on given position can perform
    /// </summary>
    /// <param name="piecePosition">Position of piece</param>
    /// <param name="allowAmbiguousCastle">Whether Castle move will be e1-g1 AND also e1-h1 which is in fact the same O-O</param>
    /// <param name="generateSan">Whether SAN notation needs to be generated. For higher productivity => set to false</param>
    /// <param name="checkTurn">Only show moves for current turn color</param>
    /// <returns>All available moves for given piece</returns>
    public Move[] Defended(Position piecePosition, bool allowAmbiguousCastle = false, bool checkTurn = true)
    {
        if (pieces[piecePosition.P] is null)
            throw new ChessPieceNotFoundException(this, piecePosition);

        var moves = new List<Move>();
        var positions = GenerateDefendPositions(piecePosition, this);

        foreach (var position in positions)
        {
            Move move = new(piecePosition, position);

            if (!IsValidDefendMove(move, this, false, checkTurn))
            {
                continue;
            }

            // Ambiguous castle
            if (!allowAmbiguousCastle && move.Parameter is MoveCastle)
            {
                if (move.NewPosition.X % 7 == 0) // Dropping king on position of rook 
                    continue;
            }

            moves.Add(move);
        }

        return moves.ToArray();
    }

    private void AddPromotionMoves(List<Move> moves, Move move, bool generateSan, PromotionType skipPromotion)
    {
        if (skipPromotion == PromotionType.Default) skipPromotion = PromotionType.ToQueen;

        moves.Add(new Move(move, skipPromotion));
        if (generateSan)
        {
            ParseToSan(moves[^1]);
        }

        // IsCheck and IsMate depends on promotion type so we have to reset those properties for each promotion type
        var promotions = new List<PromotionType>
        {
            PromotionType.ToQueen,
            PromotionType.ToRook,
            PromotionType.ToBishop,
            PromotionType.ToKnight
        };
        promotions.Remove(skipPromotion);

        foreach (var promotion in promotions)
        {
            var newMove = new Move(move, promotion);

            newMove.IsCheck = IsKingCheckedValidation(newMove, move.Piece.Color.OppositeColor(), this);
            newMove.IsMate = !PlayerHasMovesValidation(newMove, move.Piece.Color.OppositeColor(), this);

            moves.Add(newMove);

            if (generateSan)
            {
                ParseToSan(newMove);
            }
        }
    }

    /// <summary>
    /// Generates all moves that the player whose turn it is can make
    /// </summary>
    /// <param name="allowAmbiguousCastle">Whether Castle move will be e1-g1 AND also e1-h1 which is in fact the same O-O</param>
    /// <param name="generateSan">San notation needs to be generated</param>
    /// <param name="checkTurn">Only show moves for current turn color</param>
    /// <returns>All generated moves</returns>
    public async Task<Move[]> MovesAsync(bool allowAmbiguousCastle = false, bool generateSan = true, bool checkTurn = true)
    {
        var moves = new ConcurrentBag<Move>();
        var tasks = new List<Task>();

        for (short i = 0; i < 8 * 8; i++)
        {
            if (pieces[i] is not null)
            {
                short p = i;

                tasks.Add(Task.Run(() =>
                {
                    var generatedMoves = Moves(new Position(p), allowAmbiguousCastle, generateSan, checkTurn);
                    foreach (var move in generatedMoves)
                    {
                        moves.Add(move);
                    }
                }));
            }
        }

        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        return moves.ToArray();
    }

    /// <summary>
    /// Generates all moves that the player whose turn it is can make
    /// </summary>
    /// <param name="allowAmbiguousCastle">Whether Castle move will be e1-g1 AND also e1-h1 which is in fact the same O-O</param>
    /// <param name="generateSan">San notation needs to be generated</param>
    /// <param name="checkTurn">Only show moves for current turn color</param>
    /// <returns>All generated moves</returns>
    public Move[] Moves(bool allowAmbiguousCastle = false, bool generateSan = true, bool checkTurn = true)
    {
        var moves = new ConcurrentBag<Move>();

        for (short i = 0; i < 8 * 8; i++)
        {
            if (pieces[i] is not null)
            {
                short p = i;

                var generatedMoves = Moves(new Position(p), allowAmbiguousCastle, generateSan, checkTurn);
                foreach (var move in generatedMoves)
                {
                    moves.Add(move);
                }
            }
        }

        return moves.ToArray();
    }

    /// <summary>
    /// Generates all moves that the player whose turn it is can make
    /// </summary>
    /// <param name="allowAmbiguousCastle">Whether Castle move will be e1-g1 AND also e1-h1 which is in fact the same O-O</param>
    /// <param name="generateSan">San notation needs to be generated</param>
    /// <param name="checkTurn">Only show moves for current turn color</param>
    /// <returns>All generated moves</returns>
    public async Task<Move[]> DefendedAsync(bool allowAmbiguousCastle = false, bool checkTurn = true)
    {
        var moves = new ConcurrentBag<Move>();
        var tasks = new List<Task>();

        for (short i = 0; i < 8 * 8; i++)
        {
            if (pieces[i] is not null)
            {
                short p = i;

                tasks.Add(Task.Run(() =>
                {
                    var generatedMoves = Defended(new Position(p), allowAmbiguousCastle, checkTurn);
                    foreach (var move in generatedMoves)
                    {
                        moves.Add(move);
                    }
                }));
            }
        }

        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        return moves.ToArray();
    }

    /// <summary>
    /// Generates all moves that the player whose turn it is can make
    /// </summary>
    /// <param name="allowAmbiguousCastle">Whether Castle move will be e1-g1 AND also e1-h1 which is in fact the same O-O</param>
    /// <param name="generateSan">San notation needs to be generated</param>
    /// <param name="checkTurn">Only show moves for current turn color</param>
    /// <returns>All generated moves</returns>
    public Move[] Defended(bool allowAmbiguousCastle = false, bool checkTurn = true)
    {
        var moves = new ConcurrentBag<Move>();

        for (short i = 0; i < 8 * 8; i++)
        {
            if (pieces[i] is not null)
            {
                short p = i;

                var generatedMoves = Defended(new Position(p), allowAmbiguousCastle, checkTurn);
                foreach (var move in generatedMoves)
                {
                    moves.Add(move);
                }
            }
        }

        return moves.ToArray();
    }

    public async Task<BoardAnalyses> AnalysesAsync(bool allowAmbiguousCastle = false)
    {
        var analyse = new BoardAnalyses(this);
        analyse.SetMoves(await MovesAsync(allowAmbiguousCastle, checkTurn: false));
        analyse.SetDefend(await DefendedAsync(allowAmbiguousCastle, checkTurn: false));

        return analyse;
    }

    public BoardAnalyses Analyses(bool allowAmbiguousCastle = false)
    {
        var analyse = new BoardAnalyses(this);
        analyse.SetMoves(Moves(allowAmbiguousCastle, checkTurn: false));
        analyse.SetDefend(Defended(allowAmbiguousCastle, checkTurn: false));

        return analyse;
    }


    /// <summary>
    /// Generating potential positions for given piece<br/>
    /// (!) Method doesn't takes in account validation for king (may be checked after making move with returned position)
    /// </summary>
    /// <param name="piecePosition">Position of piece</param>
    /// <returns>Potential positions</returns>
    public Position[] GeneratePositions(Position piecePosition)
    {
        if (pieces[piecePosition.P] is null)
            throw new ChessPieceNotFoundException(this, piecePosition);

        return GeneratePositions(piecePosition, this);
    }

    public Position[] GenerateDefendPositions(Position piecePosition)
    {
        if (pieces[piecePosition.P] is null)
            throw new ChessPieceNotFoundException(this, piecePosition);

        return GenerateDefendPositions(piecePosition, this);
    }

    private static Position[] GeneratePositions(Position piecePosition, ChessBoard board)
    {
        var positions = new List<Position>();

        switch (board[piecePosition].Type)
        {
            case var e when e == PieceType.Pawn:
                GeneratePawnPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Rook:
                GenerateRookPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Knight:
                GenerateKnightPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Bishop:
                GenerateBishopPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Queen:
                GenerateRookPositions(piecePosition, board, positions);
                GenerateBishopPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.King:
                GenerateKingPositions(piecePosition, board, positions);
                break;
        }

        return positions.ToArray();
    }

    private static Position[] GenerateDefendPositions(Position piecePosition, ChessBoard board)
    {
        var positions = new List<Position>();

        switch (board[piecePosition].Type)
        {
            case var e when e == PieceType.Pawn:
                GeneratePawnDefendPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Rook:
                GenerateRookDefendPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Knight:
                GenerateKnightDefendPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Bishop:
                GenerateBishopDefendPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.Queen:
                GenerateRookDefendPositions(piecePosition, board, positions);
                GenerateBishopDefendPositions(piecePosition, board, positions);
                break;
            case var e when e == PieceType.King:
                GenerateKingDefendPositions(piecePosition, board, positions);
                break;
        }

        return positions.ToArray();
    }

    private static void GenerateKingPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        short minX = (short)Math.Max(0, piecePosition.X - 1);
        short maxX = (short)Math.Min(7, piecePosition.X + 1);
        short minY = (short)Math.Max(0, piecePosition.Y - 1);
        short maxY = (short)Math.Min(7, piecePosition.Y + 1);

        for (short x = minX; x <= maxX; x++)
        {
            for (short y = minY; y <= maxY; y++)
            {
                if (x != piecePosition.X || y != piecePosition.Y)
                {
                    if (board[x, y] is null || board[x, y].Color != board[piecePosition].Color)
                    {
                        positions.Add(new Position(x, y));
                    }
                }
            }
        }

        if (piecePosition.Y % 7 == 0 && piecePosition.X == 4)
        {
            // Castle options

            var rook = board[new Position(0, piecePosition.Y)];

            if (board[1, piecePosition.Y] is null && board[2, piecePosition.Y] is null && board[3, piecePosition.Y] is null)
            {
                if (rook?.Type == PieceType.Rook && rook.Color == board[piecePosition].Color)
                {
                    positions.Add(new Position(0, piecePosition.Y)); // TODO verbose option
                    positions.Add(new Position(2, piecePosition.Y));
                }
            }

            rook = board[new Position(7, piecePosition.Y)];

            if (board[5, piecePosition.Y] is null && board[6, piecePosition.Y] is null)
            {
                if (rook?.Type == PieceType.Rook && rook.Color == board[piecePosition].Color)
                {
                    positions.Add(new Position(6, piecePosition.Y));
                    positions.Add(new Position(7, piecePosition.Y)); // TODO verbose option
                }
            }
        }
    }

    private static void GenerateKnightPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        short x = piecePosition.X;
        short y = piecePosition.Y;
        short[] xOffset = { 2, 2, -2, -2, 1, 1, -1, -1 };
        short[] yOffset = { 1, -1, 1, -1, 2, -2, 2, -2 };

        for (int i = 0; i < 8; i++)
        {
            short newX = (short)(x + xOffset[i]);
            short newY = (short)(y + yOffset[i]);

            if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
            {
                Position newPos = new Position(newX, newY);

                if (board[newPos] is null || board[newPos].Color != board[piecePosition].Color)
                {
                    positions.Add(newPos);
                }
            }
        }
    }

    private static void GeneratePawnPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        short step = (short)(board[piecePosition].Color == PieceColor.White ? 1 : -1);
        PieceColor pieceColor = board[piecePosition].Color;

        var x = piecePosition.X;
        var y = piecePosition.Y;

        short nextY = (short)(y + step);
        bool nextYIsNull = board[x, nextY] is null;

        if (nextYIsNull)
            positions.Add(new Position(x, nextY));

        short rightX = (short)(x + 1);
        bool rightXInRange = rightX < 8;

        if (rightXInRange)
        {
            Piece rightPiece = board[rightX, nextY];
            if (rightPiece is not null && rightPiece.Color != pieceColor)
                positions.Add(new Position(rightX, nextY));

            else if (IsValidEnPassant(new Move(piecePosition, new Position(rightX, nextY)) { Piece = board[piecePosition] }, board, step, 1))
                positions.Add(new Position(rightX, nextY));
        }

        short leftX = (short)(x - 1);
        bool leftXInRange = leftX > -1;

        if (leftXInRange)
        {
            Piece leftPiece = board[leftX, nextY];
            if (leftPiece is not null && leftPiece.Color != pieceColor)
                positions.Add(new Position(leftX, nextY));

            else if (IsValidEnPassant(new Move(piecePosition, new Position(leftX, nextY)) { Piece = board[piecePosition] }, board, step, -1))
                positions.Add(new Position(leftX, nextY));
        }

        // 2 forward
        if ((y == 1 && pieceColor == PieceColor.White || y == 6 && pieceColor == PieceColor.Black)
            && nextYIsNull
            && board[x, (short)(y + step * 2)] is null)
            positions.Add(new Position(x, (short)(y + step * 2)));
    }

    // Directions for which the bishop can move
    private static readonly List<(short x, short y)> BishopDirections = new() { (1, 1), (1, -1), (-1, 1), (-1, -1) };

    private static void GenerateBishopPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        // Iterate through each direction
        foreach (var direction in BishopDirections)
        {
            // Start at the current piece position and move in the current direction
            var currentPosition = (x: (short)(piecePosition.X + direction.x), y: (short)(piecePosition.Y + direction.y));

            // Loop until the end of the board is reached or a piece is encountered
            while (currentPosition.x >= 0 && currentPosition.x < 8 && currentPosition.y >= 0 && currentPosition.y < 8)
            {
                // Check if the current position has a piece
                var currentPiece = board[currentPosition.x, currentPosition.y];
                if (currentPiece != null)
                {
                    // If the current piece is not of the same color as the original piece, add it to the list of positions
                    if (currentPiece.Color != board[piecePosition].Color)
                        positions.Add(new Position(currentPosition.x, currentPosition.y));

                    // Break out of the loop
                    break;
                }

                // Add the current position to the list of positions
                positions.Add(new Position(currentPosition.x, currentPosition.y));

                // Move to the next position in the current direction
                currentPosition.x += direction.x;
                currentPosition.y += direction.y;
            }
        }
    }

    // Directions for which the rook can move
    private static readonly List<(short x, short y)> RookDirections = new() { (0, 1), (1, 0), (0, -1), (-1, 0) };

    private static void GenerateRookPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        foreach (var direction in RookDirections)
        {
            short x = (short)(piecePosition.X + direction.x);
            short y = (short)(piecePosition.Y + direction.y);

            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                var currentPiece = board[x, y];
                if (currentPiece is not null)
                {
                    if (currentPiece.Color != board[piecePosition].Color)
                        positions.Add(new Position(x, y));
                    break;
                }

                positions.Add(new Position(x, y));

                x += direction.x;
                y += direction.y;
            }
        }
    }



    // DEFEND
    private static void GenerateKingDefendPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        short minX = (short)Math.Max(0, piecePosition.X - 1);
        short maxX = (short)Math.Min(7, piecePosition.X + 1);
        short minY = (short)Math.Max(0, piecePosition.Y - 1);
        short maxY = (short)Math.Min(7, piecePosition.Y + 1);

        for (short x = minX; x <= maxX; x++)
        {
            for (short y = minY; y <= maxY; y++)
            {
                if (x != piecePosition.X || y != piecePosition.Y)
                {
                    if (board[x, y] is not null && board[x, y].Color == board[piecePosition].Color)
                    {
                        positions.Add(new Position(x, y));
                    }
                }
            }
        }
    }

    private static void GenerateKnightDefendPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        short x = piecePosition.X;
        short y = piecePosition.Y;
        short[] xOffset = { 2, 2, -2, -2, 1, 1, -1, -1 };
        short[] yOffset = { 1, -1, 1, -1, 2, -2, 2, -2 };

        for (int i = 0; i < 8; i++)
        {
            short newX = (short)(x + xOffset[i]);
            short newY = (short)(y + yOffset[i]);

            if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
            {
                Position newPos = new Position(newX, newY);

                if (board[newPos] is not null && board[newPos].Color == board[piecePosition].Color)
                {
                    positions.Add(newPos);
                }
            }
        }
    }

    private static void GeneratePawnDefendPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        short step = (short)(board[piecePosition].Color == PieceColor.White ? 1 : -1);
        PieceColor pieceColor = board[piecePosition].Color;

        var x = piecePosition.X;
        var y = piecePosition.Y;

        short nextY = (short)(y + step);

        short rightX = (short)(x + 1);
        bool rightXInRange = rightX < 8;

        if (rightXInRange)
        {
            Piece rightPiece = board[rightX, nextY];
            if (rightPiece is null || rightPiece.Color == pieceColor)
                positions.Add(new Position(rightX, nextY));
        }

        short leftX = (short)(x - 1);
        bool leftXInRange = leftX > -1;

        if (leftXInRange)
        {
            Piece leftPiece = board[leftX, nextY];
            if (leftPiece is null || leftPiece.Color == pieceColor)
                positions.Add(new Position(leftX, nextY));
        }
    }

    private static void GenerateBishopDefendPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        // Iterate through each direction
        foreach (var direction in BishopDirections)
        {
            // Start at the current piece position and move in the current direction
            var currentPosition = (x: (short)(piecePosition.X + direction.x), y: (short)(piecePosition.Y + direction.y));

            // Loop until the end of the board is reached or a piece is encountered
            while (currentPosition.x >= 0 && currentPosition.x < 8 && currentPosition.y >= 0 && currentPosition.y < 8)
            {
                // Check if the current position has a piece
                var currentPiece = board[currentPosition.x, currentPosition.y];
                if (currentPiece is not null)
                {
                    // If the current piece is not of the same color as the original piece, add it to the list of positions
                    if (currentPiece.Color == board[piecePosition].Color)
                        positions.Add(new Position(currentPosition.x, currentPosition.y));

                    // Break out of the loop
                    break;
                }

                // Move to the next position in the current direction
                currentPosition.x += direction.x;
                currentPosition.y += direction.y;
            }
        }
    }

    private static void GenerateRookDefendPositions(Position piecePosition, ChessBoard board, List<Position> positions)
    {
        foreach (var direction in RookDirections)
        {
            short x = (short)(piecePosition.X + direction.x);
            short y = (short)(piecePosition.Y + direction.y);

            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                var currentPiece = board[x, y];
                if (currentPiece is not null)
                {
                    if (currentPiece.Color == board[piecePosition].Color)
                        positions.Add(new Position(x, y));
                    break;
                }

                x += direction.x;
                y += direction.y;
            }
        }
    }
}