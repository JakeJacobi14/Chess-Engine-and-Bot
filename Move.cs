public class Move
{
	public int From;

	public int To;

	public Piece PreviousPiece;

	public bool WasFirstMove;

	public bool WasPromotion;

	public Move PreviousLastMove;

	public int Evaluation;

	public bool PreviousWhiteKingsideCastleRight { get; set; }

	public bool PreviousWhiteQueensideCastleRight { get; set; }

	public bool PreviousBlackKingsideCastleRight { get; set; }

	public bool PreviousBlackQueensideCastleRight { get; set; }

	public string PreviousEnPassantTargetSquare { get; set; }

	public int PreviousHalfmoveClock { get; set; }

	public override string ToString()
	{
		return $"from {From} to {To}";
	}
}
