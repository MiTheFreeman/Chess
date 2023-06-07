// *****************************************************
// *                                                   *
// * O Lord, Thank you for your goodness in our lives. *
// *     Please bless this code to our compilers.      *
// *                     Amen.                         *
// *                                                   *
// *****************************************************
//                                    Made by Geras1mleo

namespace Chess;

public partial class ChessBoard
{
    /// <summary>
    /// Raises when trying to make or validate move but after the move would have been made, king would have been checked
    /// </summary>
    public event ChessCheckedChangedEventHandler? OnInvalidMoveKingChecked = delegate { };
    /// <summary>
    /// Raises when white king is (un)checked
    /// </summary>
    public event ChessCheckedChangedEventHandler? OnWhiteKingCheckedChanged = delegate { };
    /// <summary>
    /// Raises when black king is (un)checked
    /// </summary>
    public event ChessCheckedChangedEventHandler? OnBlackKingCheckedChanged = delegate { };
    /// <summary>
    /// Raises when user has to choose promotion action
    /// </summary>
    public event ChessPromotionResultEventHandler? OnPromotePawn = delegate { };
    /// <summary>
    /// Raises when it's end of game
    /// </summary>
    public event ChessEndGameEventHandler? OnEndGame = delegate { };
    /// <summary>
    /// Raises when any piece has been captured
    /// </summary>
    public event ChessCaptureEventHandler? OnCaptured = delegate { };
    private SynchronizationContext? context = SynchronizationContext.Current;

    /// <summary>
    /// Disable synchronization context for events
    /// </summary>
    public void DisableSyncContext() => context = null;

    private void OnWhiteKingCheckedChangedEvent(CheckEventArgs e)
    {
        if (context is not null)
            context.Send(delegate { OnWhiteKingCheckedChanged?.Invoke(this, e); }, null);
        else
            OnWhiteKingCheckedChanged?.Invoke(this, e);
    }

    private void OnBlackKingCheckedChangedEvent(CheckEventArgs e)
    {
        if (context is not null)
            context.Send(delegate { OnBlackKingCheckedChanged?.Invoke(this, e); }, null);
        else
            OnBlackKingCheckedChanged?.Invoke(this, e);
    }

    private void OnInvalidMoveKingCheckedEvent(CheckEventArgs e)
    {
        if (context is not null)
            context.Send(delegate { OnInvalidMoveKingChecked?.Invoke(this, e); }, null);
        else
            OnInvalidMoveKingChecked?.Invoke(this, e);
    }

    private void OnPromotePawnEvent(PromotionEventArgs e)
    {
        if (context is not null)
            context.Send(delegate { OnPromotePawn?.Invoke(this, e); }, null);
        else
            OnPromotePawn?.Invoke(this, e);
    }

    private void OnEndGameEvent()
    {
        if (context is not null)
            context.Send(delegate { OnEndGame?.Invoke(this, new EndgameEventArgs(this, EndGame)); }, null);
        else
            OnEndGame?.Invoke(this, new EndgameEventArgs(this, EndGame));
    }

    private void OnCapturedEvent(Piece piece)
    {
        if (context is not null)
            context.Send(delegate { OnCaptured?.Invoke(this, new CaptureEventArgs(this, piece, CapturedWhite, CapturedBlack)); }, null);
        else
            OnCaptured?.Invoke(this, new CaptureEventArgs(this, piece, CapturedWhite, CapturedBlack));
    }
}
