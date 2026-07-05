using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Board
{
	public static bool IsWhitesTurn;

	public static bool WhiteHasCastled;

	public static bool BlackHasCastled;

	public static bool GameOver;

	public static Piece[] Squares { get; private set; }

	public static Move LastMove { get; private set; }

	public static bool IsUpdating { get; set; }

	public static string EnPassantTargetSquare { get; set; }

	public static int HalfmoveClock { get; set; }

	public static bool CanWhiteCastleKingside { get; set; }

	public static bool CanWhiteCastleQueenside { get; set; }

	public static bool CanBlackCastleKingside { get; set; }

	public static bool CanBlackCastleQueenside { get; set; }

	public static event Action<bool> OnTurnChanged;

	static Board()
	{
		IsWhitesTurn = true;
		CanWhiteCastleKingside = true;
		CanWhiteCastleQueenside = true;
		CanBlackCastleKingside = true;
		CanBlackCastleQueenside = true;
		Squares = new Piece[64];
		for (int i = 0; i < 64; i++)
		{
			Squares[i] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
		}
	}

	public static Piece GetPiece(int position)
	{
		if (IsValidPosition(position))
		{
			return Squares[position];
		}
		Debug.LogError($"Invalid board position: {position}");
		return null;
	}

	public static void PlacePiece(Piece piece, int position)
	{
		if (IsValidPosition(position))
		{
			Squares[position] = piece;
		}
		else
		{
			Debug.LogError($"Invalid board position: {position}");
		}
	}

	public static List<int> FindValidMoves(int pieceIndex)
	{
		Piece piece = GetPiece(pieceIndex);
		List<int> list = new List<int>();
		switch (piece.Type)
		{
		case Piece.PieceType.Pawn:
		{
			int num7 = ((piece.Color == Piece.PieceColor.White) ? 1 : (-1));
			int num8 = pieceIndex + 8 * num7;
			int num9 = pieceIndex + 16 * num7;
			int num10 = pieceIndex + 7 * num7;
			int num11 = pieceIndex + 9 * num7;
			if (num8 >= 0 && num8 < 64 && GetPiece(num8).Type == Piece.PieceType.None)
			{
				list.Add(num8);
				if (piece.IsFirstMove && num9 >= 0 && num9 < 64 && Squares[num9].Type == Piece.PieceType.None)
				{
					list.Add(num9);
				}
			}
			int num12 = pieceIndex % 8;
			if (num10 >= 0 && num10 < 64)
			{
				int num13 = num10 % 8;
				if (((piece.Color == Piece.PieceColor.White && num13 == num12 - 1) || (piece.Color == Piece.PieceColor.Black && num13 == num12 + 1)) && GetPiece(num10).Type != Piece.PieceType.None && GetPiece(num10).Color != piece.Color)
				{
					list.Add(num10);
				}
			}
			if (num11 >= 0 && num11 < 64)
			{
				int num14 = num11 % 8;
				if (((piece.Color == Piece.PieceColor.White && num14 == num12 + 1) || (piece.Color == Piece.PieceColor.Black && num14 == num12 - 1)) && GetPiece(num11).Type != Piece.PieceType.None && GetPiece(num11).Color != piece.Color)
				{
					list.Add(num11);
				}
			}
			if (((pieceIndex / 8 == 4 && piece.Color == Piece.PieceColor.White) || (pieceIndex / 8 == 3 && piece.Color == Piece.PieceColor.Black)) && Math.Abs(LastMove.To - LastMove.From) == 16 && GetPiece(LastMove.To).Type == Piece.PieceType.Pawn && GetPiece(LastMove.To).Color != piece.Color)
			{
				if (LastMove.To % 8 == pieceIndex % 8 + 1)
				{
					list.Add(pieceIndex + num7 * 8 + 1);
				}
				else if (LastMove.To % 8 == pieceIndex % 8 - 1)
				{
					list.Add(pieceIndex + num7 * 8 - 1);
				}
			}
			break;
		}
		case Piece.PieceType.Knight:
		{
			int[] array2 = new int[8] { -17, -15, -10, -6, 6, 10, 15, 17 };
			foreach (int num15 in array2)
			{
				int num16 = pieceIndex + num15;
				if (num16 < 0 || num16 > 63)
				{
					continue;
				}
				int num17 = pieceIndex % 8;
				int num18 = num16 % 8;
				int num19 = Math.Abs(num17 - num18);
				if (num19 == 1 || num19 == 2)
				{
					Piece piece2 = GetPiece(num16);
					if (piece2.Type == Piece.PieceType.None || piece2.Color != piece.Color)
					{
						list.Add(num16);
					}
				}
			}
			break;
		}
		case Piece.PieceType.Bishop:
		{
			int[] array2 = new int[4] { 9, -9, 7, -7 };
			foreach (int num6 in array2)
			{
				for (int k = pieceIndex + num6; IsBishopMoveValid(pieceIndex, k, num6, piece); k += num6)
				{
					list.Add(k);
					if (GetPiece(k).Type != Piece.PieceType.None)
					{
						if (GetPiece(k).Color != piece.Color)
						{
						}
						break;
					}
				}
			}
			break;
		}
		case Piece.PieceType.Rook:
		{
			int[] array2 = new int[4] { 8, -8, 1, -1 };
			foreach (int num20 in array2)
			{
				for (int n = pieceIndex + num20; IsRookMoveValid(pieceIndex, n, num20, piece); n += num20)
				{
					list.Add(n);
					if (GetPiece(n).Type != Piece.PieceType.None)
					{
						if (GetPiece(n).Color != piece.Color)
						{
						}
						break;
					}
				}
			}
			break;
		}
		case Piece.PieceType.Queen:
		{
			int[] array3 = new int[8] { 8, -8, 1, -1, 9, -9, 7, -7 };
			for (int l = 0; l < 8; l++)
			{
				for (int m = pieceIndex + array3[l]; (l < 4) ? IsRookMoveValid(pieceIndex, m, array3[l], piece) : IsBishopMoveValid(pieceIndex, m, array3[l], piece); m += array3[l])
				{
					list.Add(m);
					if (GetPiece(m).Type != Piece.PieceType.None)
					{
						if (GetPiece(m).Color != piece.Color)
						{
						}
						break;
					}
				}
			}
			break;
		}
		case Piece.PieceType.King:
		{
			int[] array = new int[8] { 8, -8, 1, -1, 9, -9, 7, -7 };
			int num = pieceIndex / 8;
			int num2 = pieceIndex % 8;
			for (int i = 0; i < 8; i++)
			{
				int num3 = pieceIndex + array[i];
				int num4 = num3 / 8;
				int num5 = num3 % 8;
				if (num3 >= 0 && num3 <= 63 && Math.Abs(num4 - num) <= 1 && Math.Abs(num5 - num2) <= 1 && GetPiece(num3).Color != piece.Color)
				{
					list.Add(num3);
				}
			}
			if (piece.IsFirstMove)
			{
				if (CanCastle(pieceIndex, pieceIndex + 3, piece.Color))
				{
					list.Add(pieceIndex + 2);
				}
				if (CanCastle(pieceIndex, pieceIndex - 4, piece.Color))
				{
					list.Add(pieceIndex - 2);
				}
			}
			break;
		}
		}
		List<int> list2 = new List<int>();
		foreach (int item in list)
		{
			if (!DoesMovePutKingInCheck(pieceIndex, item, piece.Color))
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private static bool CanCastle(int kingIndex, int rookIndex, Piece.PieceColor kingColor)
	{
		Piece piece = GetPiece(rookIndex);
		if (piece.Type != Piece.PieceType.Rook || !piece.IsFirstMove || piece.Color != kingColor || IsKingInCheck(kingColor))
		{
			return false;
		}
		int num = ((rookIndex > kingIndex) ? 1 : (-1));
		int num2 = kingIndex + 3 * num;
		for (int i = kingIndex + num; i != num2 + num; i += num)
		{
			if (i != rookIndex && GetPiece(i).Type != Piece.PieceType.None)
			{
				return false;
			}
			if (i != kingIndex + 3 * num && IsSquareAttacked(i, (kingColor != Piece.PieceColor.White) ? Piece.PieceColor.White : Piece.PieceColor.Black))
			{
				return false;
			}
		}
		if (GetPiece(rookIndex).Type != Piece.PieceType.Rook)
		{
			return false;
		}
		return true;
	}

	private static bool IsBishopMoveValid(int originalIndex, int newIndex, int modifier, Piece piece)
	{
		if (newIndex < 0 || newIndex > 63)
		{
			return false;
		}
		int num = originalIndex % 8;
		int num2 = newIndex % 8;
		int num3 = originalIndex / 8;
		if (Math.Abs(newIndex / 8 - num3) != Math.Abs(num2 - num))
		{
			return false;
		}
		switch (modifier)
		{
		case -7:
		case 9:
			if (num2 <= num)
			{
				return false;
			}
			break;
		case -9:
		case 7:
			if (num2 >= num)
			{
				return false;
			}
			break;
		}
		if (GetPiece(newIndex).Type != Piece.PieceType.None && GetPiece(newIndex).Color == piece.Color)
		{
			return false;
		}
		return true;
	}

	private static bool IsRookMoveValid(int originalIndex, int newIndex, int modifier, Piece piece)
	{
		if (newIndex < 0 || newIndex > 63)
		{
			return false;
		}
		int num = originalIndex % 8;
		int num2 = newIndex % 8;
		int num3 = originalIndex / 8;
		int num4 = newIndex / 8;
		if (num != num2 && num3 != num4)
		{
			return false;
		}
		if (GetPiece(newIndex).Type != Piece.PieceType.None && GetPiece(newIndex).Color == piece.Color)
		{
			return false;
		}
		return true;
	}

	public static void MakeMove(Move move)
	{
		if (GameOver)
		{
			return;
		}
		IsWhitesTurn = !IsWhitesTurn;
		Piece piece = GetPiece(move.From);
		move.PreviousPiece = GetPiece(move.To);
		move.WasFirstMove = piece.IsFirstMove;
		move.PreviousLastMove = LastMove;
		LastMove = move;
		if (piece.Type == Piece.PieceType.Pawn && move.From % 8 != move.To % 8 && GetPiece(move.To).Type == Piece.PieceType.None)
		{
			int num = move.To - ((piece.Color == Piece.PieceColor.White) ? 8 : (-8));
			Squares[num] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
		}
		if (piece.Type == Piece.PieceType.King && Math.Abs(move.To - move.From) == 2)
		{
			if (move.To > move.From)
			{
				Squares[move.To] = piece;
				Squares[move.To - 1] = Squares[move.From + 3];
				Squares[move.From + 3] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
			}
			else
			{
				Squares[move.To] = piece;
				Squares[move.To + 1] = Squares[move.From - 4];
				Squares[move.From - 4] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
			}
			if (piece.Color == Piece.PieceColor.White)
			{
				WhiteHasCastled = true;
			}
			else
			{
				BlackHasCastled = true;
			}
		}
		else
		{
			Squares[move.To] = piece;
		}
		if (piece.Type == Piece.PieceType.Pawn && (move.To / 8 == 0 || move.To / 8 == 7))
		{
			Squares[move.To] = new Piece(Piece.PieceType.Queen, piece.Color);
			move.WasPromotion = true;
		}
		if (piece.Type == Piece.PieceType.Pawn || piece.Type == Piece.PieceType.Rook || piece.Type == Piece.PieceType.King)
		{
			Squares[move.To].IsFirstMove = false;
		}
		Squares[move.From] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
		UpdateFenProperties(move);
	}

	public static void ReallyMakeMove(Move move, bool doAnimation)
	{
		if (GameOver)
		{
			return;
		}
		GameObject.FindWithTag("BoardManager").GetComponent<BoardManager>().MakeOutline(move);
		Piece piece = GetPiece(move.From);
		move.PreviousPiece = GetPiece(move.To);
		move.WasFirstMove = piece.IsFirstMove;
		move.PreviousLastMove = LastMove;
		LastMove = move;
		if (piece.Type == Piece.PieceType.Pawn && move.From % 8 != move.To % 8 && GetPiece(move.To).Type == Piece.PieceType.None)
		{
			int num = move.To - ((piece.Color == Piece.PieceColor.White) ? 8 : (-8));
			Squares[num] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
		}
		if (piece.Type == Piece.PieceType.King && Math.Abs(move.To - move.From) == 2)
		{
			if (move.To > move.From)
			{
				Squares[move.To] = piece;
				Squares[move.To - 1] = Squares[move.From + 3];
				Squares[move.From + 3] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
			}
			else
			{
				Squares[move.To] = piece;
				Squares[move.To + 1] = Squares[move.From - 4];
				Squares[move.From - 4] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
			}
			if (piece.Color == Piece.PieceColor.White)
			{
				WhiteHasCastled = true;
			}
			else
			{
				BlackHasCastled = true;
			}
		}
		else
		{
			Squares[move.To] = piece;
		}
		if (piece.Type == Piece.PieceType.Pawn && (move.To / 8 == 0 || move.To / 8 == 7))
		{
			Squares[move.To] = new Piece(Piece.PieceType.Queen, piece.Color);
			move.WasPromotion = true;
		}
		if (piece.Type == Piece.PieceType.Pawn || piece.Type == Piece.PieceType.Rook || piece.Type == Piece.PieceType.King)
		{
			Squares[move.To].IsFirstMove = false;
		}
		Squares[move.From] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
		if (doAnimation)
		{
			GameObject.FindWithTag("SetupBoard").GetComponent<SetupBoard>().UpdateMovedPiece(move);
		}
		else
		{
			GameObject.FindWithTag("SetupBoard").GetComponent<SetupBoard>().UpdateBoard();
		}
		BoardManager component = GameObject.FindWithTag("BoardManager").GetComponent<BoardManager>();
		component.UpdateTexts(move);
		component.PlaySound(move);
		IsWhitesTurn = !IsWhitesTurn;
		Board.OnTurnChanged?.Invoke(IsWhitesTurn);
		UpdateFenProperties(move);
	}

	public static void UnMakeMove(Move move)
	{
		if (GameOver)
		{
			return;
		}
		IsWhitesTurn = !IsWhitesTurn;
		Piece piece = GetPiece(move.To);
		if (piece.Type == Piece.PieceType.Pawn && move.From % 8 != move.To % 8 && move.PreviousPiece.Type == Piece.PieceType.None)
		{
			int num = move.To - ((piece.Color == Piece.PieceColor.White) ? 8 : (-8));
			Squares[num] = new Piece(Piece.PieceType.Pawn, (piece.Color != Piece.PieceColor.White) ? Piece.PieceColor.White : Piece.PieceColor.Black, firstMove: false);
		}
		if (piece.Type == Piece.PieceType.King && Math.Abs(move.To - move.From) == 2)
		{
			if (move.To > move.From)
			{
				Squares[move.From] = piece;
				Squares[move.From + 3] = Squares[move.To - 1];
				Squares[move.To - 1] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
			}
			else
			{
				Squares[move.From] = piece;
				Squares[move.From - 4] = Squares[move.To + 1];
				Squares[move.To + 1] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
			}
			if (piece.Color == Piece.PieceColor.White)
			{
				WhiteHasCastled = false;
			}
			else
			{
				BlackHasCastled = false;
			}
		}
		else if (move.WasPromotion)
		{
			Squares[move.From] = new Piece(Piece.PieceType.Pawn, piece.Color, firstMove: false);
		}
		else
		{
			Squares[move.From] = piece;
		}
		Squares[move.From].IsFirstMove = move.WasFirstMove;
		Squares[move.To] = move.PreviousPiece;
		LastMove = move.PreviousLastMove;
		RestoreFenProperties(move);
	}

	private static void UpdateFenProperties(Move move)
	{
		move.PreviousEnPassantTargetSquare = EnPassantTargetSquare;
		move.PreviousHalfmoveClock = HalfmoveClock;
		if (GetPiece(move.To).Type == Piece.PieceType.Pawn && Math.Abs(move.To - move.From) == 16)
		{
			EnPassantTargetSquare = SquareToAlgebraic((move.From + move.To) / 2);
		}
		else
		{
			EnPassantTargetSquare = "-";
		}
		if (GetPiece(move.To).Type == Piece.PieceType.Pawn || move.PreviousPiece.Type != Piece.PieceType.None)
		{
			HalfmoveClock = 0;
		}
		else
		{
			HalfmoveClock++;
		}
	}

	private static void RestoreFenProperties(Move move)
	{
		EnPassantTargetSquare = move.PreviousEnPassantTargetSquare;
		HalfmoveClock = move.PreviousHalfmoveClock;
	}

	private static string SquareToAlgebraic(int square)
	{
		char c = (char)(97 + square % 8);
		int num = square / 8 + 1;
		return $"{c}{num}";
	}

	public static bool IsKingInCheck(Piece.PieceColor kingColor)
	{
		int num = -1;
		for (int i = 0; i < 64; i++)
		{
			if (Squares[i].Type == Piece.PieceType.King && Squares[i].Color == kingColor)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogError("King not found on the board.");
			return false;
		}
		return IsSquareAttacked(num, (kingColor != Piece.PieceColor.White) ? Piece.PieceColor.White : Piece.PieceColor.Black);
	}

	public static bool IsCheckmate(Piece.PieceColor color)
	{
		if (!IsKingInCheck(color))
		{
			return false;
		}
		for (int i = 0; i < 64; i++)
		{
			Piece piece = GetPiece(i);
			if (piece.Type == Piece.PieceType.None || piece.Color != color)
			{
				continue;
			}
			foreach (int item in FindValidMoves(i))
			{
				Move move = new Move
				{
					From = i,
					To = item
				};
				MakeMove(move);
				bool num = IsKingInCheck(color);
				UnMakeMove(move);
				if (!num)
				{
					return false;
				}
			}
			if (FindValidMoves(i).Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsStaleMate()
	{
		for (int i = 0; i < 64; i++)
		{
			if (GetPiece(i).Color == (Piece.PieceColor)((!IsWhitesTurn) ? 1 : 2) && FindValidMoves(i).Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsSquareAttacked(int squareIndex, Piece.PieceColor attackingColor)
	{
		int num = ((attackingColor == Piece.PieceColor.White) ? 1 : (-1));
		int[] array = new int[2]
		{
			7 * num,
			9 * num
		};
		foreach (int num2 in array)
		{
			int num3 = squareIndex - num2;
			if (num3 < 0 || num3 >= 64)
			{
				continue;
			}
			int num4 = squareIndex % 8;
			int num5 = num3 % 8;
			if (Math.Abs(num4 - num5) == 1)
			{
				Piece piece = GetPiece(num3);
				if (piece.Type == Piece.PieceType.Pawn && piece.Color == attackingColor)
				{
					return true;
				}
			}
		}
		array = new int[8] { -17, -15, -10, -6, 6, 10, 15, 17 };
		foreach (int num6 in array)
		{
			int num7 = squareIndex - num6;
			if (num7 < 0 || num7 >= 64)
			{
				continue;
			}
			int num8 = squareIndex % 8;
			int num9 = num7 % 8;
			int num10 = Math.Abs(num8 - num9);
			if (num10 == 1 || num10 == 2)
			{
				Piece piece2 = GetPiece(num7);
				if (piece2.Type == Piece.PieceType.Knight && piece2.Color == attackingColor)
				{
					return true;
				}
			}
		}
		array = new int[8] { -9, -8, -7, -1, 1, 7, 8, 9 };
		foreach (int num11 in array)
		{
			int num12 = squareIndex - num11;
			if (num12 < 0 || num12 >= 64)
			{
				continue;
			}
			int num13 = squareIndex % 8;
			if (Math.Abs(num12 % 8 - num13) <= 1)
			{
				Piece piece3 = GetPiece(num12);
				if (piece3.Type == Piece.PieceType.King && piece3.Color == attackingColor)
				{
					return true;
				}
			}
		}
		array = new int[8] { -9, -8, -7, -1, 1, 7, 8, 9 };
		foreach (int num14 in array)
		{
			int num15 = squareIndex;
			int num16 = num15 % 8;
			Piece piece4;
			do
			{
				num15 -= num14;
				if (num15 < 0 || num15 >= 64)
				{
					break;
				}
				int num17 = num15 % 8;
				if (Math.Abs(num17 - num16) > 1)
				{
					break;
				}
				num16 = num17;
				piece4 = GetPiece(num15);
				if (piece4.Color == attackingColor)
				{
					if ((Math.Abs(num14) == 8 || Math.Abs(num14) == 1) && (piece4.Type == Piece.PieceType.Rook || piece4.Type == Piece.PieceType.Queen))
					{
						return true;
					}
					if ((Math.Abs(num14) != 7 && Math.Abs(num14) != 9) || (piece4.Type != Piece.PieceType.Bishop && piece4.Type != Piece.PieceType.Queen))
					{
						break;
					}
					return true;
				}
			}
			while (piece4.Type == Piece.PieceType.None);
		}
		return false;
	}

	public static bool DoesMovePutKingInCheck(int from, int to, Piece.PieceColor color)
	{
		Move move = new Move
		{
			From = from,
			To = to
		};
		MakeMove(move);
		int num = -1;
		for (int i = 0; i < 64; i++)
		{
			if (GetPiece(i).Type == Piece.PieceType.King && GetPiece(i).Color == color)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogError("King not found on the board.");
			UnMakeMove(move);
			return false;
		}
		bool result = IsSquareAttacked(num, (color != Piece.PieceColor.White) ? Piece.PieceColor.White : Piece.PieceColor.Black);
		UnMakeMove(move);
		return result;
	}

	private static bool IsValidPosition(int position)
	{
		if (position >= 0)
		{
			return position < 64;
		}
		return false;
	}

	public static string GetFEN()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int num2 = 7; num2 >= 0; num2--)
		{
			for (int i = 0; i < 8; i++)
			{
				Piece piece = GetPiece(num2 * 8 + i);
				if (piece.Type == Piece.PieceType.None)
				{
					num++;
					continue;
				}
				if (num > 0)
				{
					stringBuilder.Append(num);
					num = 0;
				}
				stringBuilder.Append(GetPieceChar(piece));
			}
			if (num > 0)
			{
				stringBuilder.Append(num);
				num = 0;
			}
			if (num2 > 0)
			{
				stringBuilder.Append('/');
			}
		}
		stringBuilder.Append(IsWhitesTurn ? " w" : " b");
		stringBuilder.Append(" ");
		stringBuilder.Append(CanWhiteCastleKingside ? "K" : "");
		stringBuilder.Append(CanWhiteCastleQueenside ? "Q" : "");
		stringBuilder.Append(CanBlackCastleKingside ? "k" : "");
		stringBuilder.Append(CanBlackCastleQueenside ? "q" : "");
		if (stringBuilder[stringBuilder.Length - 1] == ' ')
		{
			stringBuilder.Append("-");
		}
		stringBuilder.Append(" -");
		return stringBuilder.ToString();
	}

	private static char GetPieceChar(Piece piece)
	{
		switch (piece.Type)
		{
		case Piece.PieceType.King:
			if (piece.Color != Piece.PieceColor.White)
			{
				return 'k';
			}
			return 'K';
		case Piece.PieceType.Queen:
			if (piece.Color != Piece.PieceColor.White)
			{
				return 'q';
			}
			return 'Q';
		case Piece.PieceType.Rook:
			if (piece.Color != Piece.PieceColor.White)
			{
				return 'r';
			}
			return 'R';
		case Piece.PieceType.Bishop:
			if (piece.Color != Piece.PieceColor.White)
			{
				return 'b';
			}
			return 'B';
		case Piece.PieceType.Knight:
			if (piece.Color != Piece.PieceColor.White)
			{
				return 'n';
			}
			return 'N';
		case Piece.PieceType.Pawn:
			if (piece.Color != Piece.PieceColor.White)
			{
				return 'p';
			}
			return 'P';
		default:
			return ' ';
		}
	}

	public static bool InsufficientMaterial()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = GetPiece(i);
			if (piece.Type == Piece.PieceType.None)
			{
				continue;
			}
			if (piece.Type == Piece.PieceType.King)
			{
				if (piece.Color == Piece.PieceColor.White)
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
				continue;
			}
			if (piece.Type == Piece.PieceType.Bishop)
			{
				if (piece.Color == Piece.PieceColor.White)
				{
					flag3 = true;
				}
				else
				{
					flag4 = true;
				}
				continue;
			}
			if (piece.Type == Piece.PieceType.Knight)
			{
				if (piece.Color == Piece.PieceColor.White)
				{
					flag5 = true;
				}
				else
				{
					flag6 = true;
				}
				continue;
			}
			return false;
		}
		bool num = flag && flag2 && !flag3 && !flag4 && !flag5 && !flag6;
		bool flag7 = (flag && flag2 && flag3 && !flag4 && !flag5 && !flag6) || (flag2 && flag && flag4 && !flag3 && !flag6 && !flag5);
		bool flag8 = (flag && flag2 && flag5 && !flag6 && !flag3 && !flag4) || (flag2 && flag && flag6 && !flag5 && !flag4 && !flag3);
		return num || flag7 || flag8;
	}
}
