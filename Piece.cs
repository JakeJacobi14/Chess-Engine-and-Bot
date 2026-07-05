public class Piece
{
	public enum PieceType
	{
		None,
		Pawn,
		Rook,
		Knight,
		Bishop,
		Queen,
		King
	}

	public enum PieceColor
	{
		None,
		White,
		Black
	}

	public bool IsFirstMove = true;

	public bool IsSecondMove;

	public PieceType Type { get; private set; }

	public PieceColor Color { get; private set; }

	public Piece(PieceType type, PieceColor color, bool firstMove = true)
	{
		Type = type;
		Color = color;
		IsFirstMove = firstMove;
	}
}
