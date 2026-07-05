using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChessBot6 : MonoBehaviour
{
	public bool _isWhite;

	public int depth = 4;

	[SerializeField]
	private bool _pieceMoveAnimation = true;

	private Dictionary<int, int> pieceMoveCounts = new Dictionary<int, int>();

	private const int OPENING_MOVE_LIMIT = 8;

	private int currentMoveCount;

	private Move lastMove;

	private Move doubleLastMove;

	private Move tripleLastMove;

	private Move quadrupleLastMove;

	private bool IsEndgameRightNow;

	private Dictionary<string, List<(Move, int)>> openingBook;

	public bool isThinking;

	private Dictionary<Piece.PieceType, int> pieceValues = new Dictionary<Piece.PieceType, int>
	{
		{
			Piece.PieceType.Pawn,
			100
		},
		{
			Piece.PieceType.Knight,
			300
		},
		{
			Piece.PieceType.Bishop,
			325
		},
		{
			Piece.PieceType.Rook,
			500
		},
		{
			Piece.PieceType.Queen,
			900
		},
		{
			Piece.PieceType.King,
			20000
		}
	};

	private void Start()
	{
		InitializeOpeningBook();
		lastMove = new Move
		{
			From = -1,
			To = -1
		};
		doubleLastMove = new Move
		{
			From = -1,
			To = -1
		};
		tripleLastMove = new Move
		{
			From = -1,
			To = -1
		};
		quadrupleLastMove = new Move
		{
			From = -1,
			To = -1
		};
		for (int i = 0; i < 64; i++)
		{
			pieceMoveCounts[i] = 0;
		}
		if (Board.IsWhitesTurn == _isWhite)
		{
			StartCoroutine(MakeBotMoveCoroutine());
		}
	}

	private void OnEnable()
	{
		Board.OnTurnChanged += OnTurnChanged;
	}

	private void OnDisable()
	{
		Board.OnTurnChanged -= OnTurnChanged;
	}

	private void OnTurnChanged(bool isWhiteTurn)
	{
		currentMoveCount++;
		if (isWhiteTurn == _isWhite && !isThinking)
		{
			StartCoroutine(MakeBotMoveCoroutine());
		}
	}

	private int ConvertSquareToIndex(string square)
	{
		int num = square[0] - 97;
		return (int.Parse(square[1].ToString()) - 1) * 8 + num;
	}

	private Move ParseMove(string move)
	{
		return new Move
		{
			From = ConvertSquareToIndex(move.Substring(0, 2)),
			To = ConvertSquareToIndex(move.Substring(2, 2))
		};
	}

	private void InitializeOpeningBook()
	{
		openingBook = new Dictionary<string, List<(Move, int)>>();
		TextAsset textAsset = Resources.Load<TextAsset>("OpeningBook");
		if (textAsset != null)
		{
			string[] array = textAsset.text.Split('\n');
			string text = null;
			List<(Move, int)> list = new List<(Move, int)>();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text2 = array2[i].Trim();
				if (text2.StartsWith("pos "))
				{
					if (text != null)
					{
						openingBook[text] = new List<(Move, int)>(list);
					}
					text = text2.Substring(4).Trim();
					list.Clear();
				}
				else if (!string.IsNullOrWhiteSpace(text2))
				{
					string[] array3 = text2.Split(' ');
					if (array3.Length == 2)
					{
						Move item = ParseMove(array3[0]);
						int item2 = int.Parse(array3[1]);
						list.Add((item, item2));
					}
				}
			}
			if (text != null)
			{
				openingBook[text] = new List<(Move, int)>(list);
			}
		}
		else
		{
			Debug.LogError("OpeningBook.txt not found in Resources folder");
		}
	}

	private Move GetOpeningBookMove()
	{
		string fEN = Board.GetFEN();
		if (openingBook.TryGetValue(fEN, out var value))
		{
			int maxExclusive = value.Sum(((Move, int) m) => m.Item2);
			int num = UnityEngine.Random.Range(0, maxExclusive);
			int num2 = 0;
			foreach (var item3 in value)
			{
				Move item = item3.Item1;
				int item2 = item3.Item2;
				num2 += item2;
				if (num < num2)
				{
					return item;
				}
			}
		}
		return null;
	}

	public IEnumerator MakeBotMoveCoroutine()
	{
		isThinking = true;
		yield return null;
		while (Board.IsUpdating)
		{
			yield return null;
		}
		int num = depth;
		IsEndgameRightNow = IsEndgame();
		if (IsEndgameRightNow)
		{
			num++;
			Debug.Log("Endgame detected. Increasing depth to " + num);
		}
		Move botMove = GetOpeningBookMove();
		if (botMove == null)
		{
			botMove = CalculateMove(num, _isWhite, int.MinValue, int.MaxValue);
		}
		if (botMove.From != -1 && botMove.To != -1)
		{
			pieceMoveCounts[botMove.From]++;
			if (_pieceMoveAnimation)
			{
				yield return new WaitForSeconds(0.4f);
				Board.ReallyMakeMove(botMove, doAnimation: true);
			}
			else
			{
				Board.ReallyMakeMove(botMove, doAnimation: false);
			}
			UpdateMoveForward(botMove);
		}
		else
		{
			Debug.Log((_isWhite ? "White" : "Black") + " has no valid moves. Game over.");
			Board.GameOver = true;
		}
		isThinking = false;
	}

	public Move CalculateMove(int depth, bool isWhite, int alpha, int beta, bool onlyCaptures = false)
	{
		if (depth == 0 || onlyCaptures)
		{
			int evaluation = CalculateEvaluation();
			return new Move
			{
				From = -1,
				To = -1,
				Evaluation = evaluation
			};
		}
		List<Move> list = new List<Move>();
		for (int i = 0; i < 64; i++)
		{
			if (Board.GetPiece(i).Color == Piece.PieceColor.White != isWhite)
			{
				continue;
			}
			foreach (int item in Board.FindValidMoves(i))
			{
				Move move = new Move
				{
					From = i,
					To = item
				};
				if (!onlyCaptures || IsCaptureOrCheck(move, isWhite))
				{
					list.Add(move);
				}
			}
		}
		list.Sort((Move move5, Move move4) => EvaluateMove(move4, isWhite).CompareTo(EvaluateMove(move5, isWhite)));
		Move move2 = new Move
		{
			From = -1,
			To = -1,
			Evaluation = (isWhite ? int.MinValue : int.MaxValue)
		};
		if (list.Count == 0)
		{
			if (Board.IsKingInCheck(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black))
			{
				return new Move
				{
					From = -1,
					To = -1,
					Evaluation = (isWhite ? (-100000) : 100000)
				};
			}
			int num = CalculateEvaluation();
			int evaluation2 = ((!isWhite) ? Math.Max(0, -num * 9 / 10) : Math.Min(0, -num * 9 / 10));
			return new Move
			{
				From = -1,
				To = -1,
				Evaluation = evaluation2
			};
		}
		foreach (Move item2 in list)
		{
			Board.MakeMove(item2);
			UpdateMoveForward(item2);
			if (Board.IsCheckmate(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black))
			{
				Board.UnMakeMove(item2);
				return item2;
			}
			Move move3 = CalculateMove(depth - 1, !isWhite, alpha, beta);
			CalculateEvaluation(item2);
			UpdateMoveBackwards();
			Board.UnMakeMove(item2);
			if (isWhite)
			{
				if (move3.Evaluation > move2.Evaluation)
				{
					move2 = item2;
					move2.Evaluation = move3.Evaluation;
				}
				alpha = Math.Max(alpha, move3.Evaluation);
			}
			else
			{
				if (move3.Evaluation < move2.Evaluation)
				{
					move2 = item2;
					move2.Evaluation = move3.Evaluation;
				}
				beta = Math.Min(beta, move3.Evaluation);
			}
			if (beta <= alpha)
			{
				break;
			}
		}
		return move2;
	}

	private void UpdateMoveForward(Move botMove)
	{
		quadrupleLastMove = tripleLastMove;
		tripleLastMove = doubleLastMove;
		doubleLastMove = lastMove;
		lastMove = botMove;
	}

	private void UpdateMoveBackwards()
	{
		tripleLastMove = quadrupleLastMove;
		doubleLastMove = tripleLastMove;
		lastMove = doubleLastMove;
	}

	private bool IsCaptureOrCheck(Move move, bool isWhite)
	{
		Piece piece = Board.GetPiece(move.From);
		if (piece.Type == Piece.PieceType.Pawn)
		{
			if (piece.Color == Piece.PieceColor.White && move.To / 8 > 4)
			{
				return true;
			}
			if (piece.Color == Piece.PieceColor.Black && move.To / 8 < 3)
			{
				return true;
			}
		}
		if (Board.GetPiece(move.To).Type != Piece.PieceType.None)
		{
			return true;
		}
		Board.MakeMove(move);
		UpdateMoveForward(move);
		bool result = Board.IsKingInCheck((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black);
		if (Board.IsKingInCheck(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black))
		{
			result = true;
		}
		UpdateMoveBackwards();
		Board.UnMakeMove(move);
		return result;
	}

	private int EvaluateMove(Move move, bool isWhite)
	{
		int num = 0;
		Piece piece = Board.GetPiece(move.From);
		Piece piece2 = Board.GetPiece(move.To);
		if (piece2.Type != Piece.PieceType.None)
		{
			num += 10000 + pieceValues[piece2.Type] - pieceValues[piece.Type] / 100;
		}
		if (piece.Type == Piece.PieceType.Pawn && (move.To / 8 == 0 || move.To / 8 == 7))
		{
			num += 9000;
		}
		if (piece.Type == Piece.PieceType.King && Math.Abs(move.From - move.To) == 2)
		{
			num += 8000;
		}
		int num2 = move.To / 8;
		int num3 = move.To % 8;
		if (num2 >= 3 && num2 <= 4 && num3 >= 3 && num3 <= 4)
		{
			num += 200;
		}
		else if (num2 >= 2 && num2 <= 5 && num3 >= 2 && num3 <= 5)
		{
			num += 100;
		}
		switch (piece.Type)
		{
		case Piece.PieceType.Pawn:
			num += EvaluatePawnMove(move, isWhite);
			break;
		case Piece.PieceType.Knight:
			num += EvaluateKnightMove(move);
			break;
		case Piece.PieceType.Bishop:
			num += EvaluateBishopMove(move);
			break;
		case Piece.PieceType.Rook:
			num += EvaluateRookMove(move);
			break;
		case Piece.PieceType.Queen:
			num += EvaluateQueenMove(move);
			break;
		case Piece.PieceType.King:
			num += EvaluateKingMove(move, isWhite);
			break;
		}
		if (piece.Type == Piece.PieceType.Queen && currentMoveCount <= 8)
		{
			num -= 200;
		}
		return num;
	}

	private int EvaluatePawnMove(Move move, bool isWhite)
	{
		int num = 0;
		int num2 = move.To / 8;
		num += (isWhite ? num2 : (7 - num2));
		if ((move.To % 8 == 3 || move.To % 8 == 4) && (num2 == 3 || num2 == 4))
		{
			num += 10;
		}
		return num;
	}

	private int EvaluateKnightMove(Move move)
	{
		int num = move.To / 8;
		int num2 = move.To % 8;
		return (0 + (4 - Math.Max(Math.Abs(3 - num2), Math.Abs(4 - num2))) + (4 - Math.Max(Math.Abs(3 - num), Math.Abs(4 - num)))) * 2;
	}

	private int EvaluateBishopMove(Move move)
	{
		int num = move.To / 8;
		int num2 = move.To % 8;
		return (0 + Math.Min(Math.Abs(num2 - num), Math.Abs(num2 + num - 7))) * 2;
	}

	private int EvaluateRookMove(Move move)
	{
		int num = 0;
		int file = move.To % 8;
		if (IsOpenFile(file))
		{
			num += 20;
		}
		return num;
	}

	private int EvaluateQueenMove(Move move)
	{
		return 5;
	}

	private int EvaluateKingMove(Move move, bool isWhite)
	{
		int num = 0;
		int num2 = move.To / 8;
		if (IsEndgameRightNow)
		{
			num += 4 - Math.Abs(3 - move.To % 8);
			return num + (4 - Math.Abs(3 - num2));
		}
		return num + (isWhite ? num2 : (7 - num2));
	}

	private bool IsOpenFile(int file)
	{
		for (int i = 0; i < 8; i++)
		{
			if (Board.GetPiece(i * 8 + file).Type == Piece.PieceType.Pawn)
			{
				return false;
			}
		}
		return true;
	}

	private int CalculateEvaluation(Move move = null)
	{
		int num = 0;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type != Piece.PieceType.None)
			{
				int num2 = pieceValues[piece.Type];
				num += ((piece.Color == Piece.PieceColor.White) ? num2 : (-num2));
				if (currentMoveCount <= 8)
				{
					int num3 = pieceMoveCounts[i] * 10;
					num -= ((piece.Color == Piece.PieceColor.White) ? num3 : (-num3));
				}
				if (piece.Type == Piece.PieceType.Pawn)
				{
					int[] array = new int[64]
					{
						0, 5, 5, -10, -10, 5, 5, 0, 50, 50,
						50, 50, 50, 50, 50, 50, 10, 10, 20, 30,
						30, 20, 10, 10, 5, 5, 10, 25, 25, 10,
						5, 5, 0, 0, 0, 20, 20, 0, 0, 0,
						5, -5, -10, 0, 0, -10, -5, 5, 5, 10,
						10, -20, -20, 10, 10, 5, 0, 0, 0, 0,
						0, 0, 0, 0
					};
					int[] array2 = new int[64]
					{
						0, 0, 0, 0, 0, 0, 0, 0, 50, 50,
						50, 50, 50, 50, 50, 50, 20, 20, 20, 20,
						20, 20, 20, 20, 10, 10, 10, 10, 10, 10,
						10, 10, 5, 5, 5, 5, 5, 5, 5, 5,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0
					};
					int num4 = ((!IsEndgameRightNow) ? ((piece.Color == Piece.PieceColor.White) ? array[63 - i] : (-array[i])) : ((piece.Color == Piece.PieceColor.White) ? array2[63 - i] : (-array2[i])));
					num += num4;
				}
				else if (piece.Type == Piece.PieceType.Bishop)
				{
					int[] array3 = new int[64]
					{
						-20, -10, -10, -10, -10, -10, -10, -20, -10, 5,
						0, 0, 0, 0, 5, -10, -10, 10, 10, 10,
						10, 10, 10, -10, -10, 0, 10, 10, 10, 10,
						0, -10, -10, 5, 5, 10, 10, 5, 5, -10,
						-10, 0, 5, 10, 10, 5, 0, -10, -10, 0,
						0, 0, 0, 0, 0, -10, -20, -10, -10, -10,
						-10, -10, -10, -20
					};
					int[] array4 = new int[64]
					{
						-20, -10, -10, -10, -10, -10, -10, -20, -10, 0,
						0, 0, 0, 0, 0, -10, -10, 0, 10, 15,
						15, 10, 0, -10, -10, 0, 15, 20, 20, 15,
						0, -10, -10, 0, 15, 20, 20, 15, 0, -10,
						-10, 0, 10, 15, 15, 10, 0, -10, -10, 0,
						0, 0, 0, 0, 0, -10, -20, -10, -10, -10,
						-10, -10, -10, -20
					};
					int num5 = ((!IsEndgameRightNow) ? ((piece.Color == Piece.PieceColor.White) ? array3[63 - i] : (-array3[i])) : ((piece.Color == Piece.PieceColor.White) ? array4[63 - i] : (-array4[i])));
					num += num5;
				}
				else if (piece.Type == Piece.PieceType.Knight)
				{
					int[] array5 = new int[64]
					{
						-50, -40, -30, -30, -30, -30, -40, -50, -40, -20,
						0, 0, 0, 0, -20, -40, -30, 0, 10, 15,
						15, 10, 0, -30, -30, 5, 15, 20, 20, 15,
						5, -30, -30, 0, 15, 20, 20, 15, 0, -30,
						-30, 5, 10, 15, 15, 10, 5, -30, -40, -20,
						0, 5, 5, 0, -20, -40, -50, -40, -30, -30,
						-30, -30, -40, -50
					};
					int[] array6 = new int[64]
					{
						-50, -40, -30, -30, -30, -30, -40, -50, -40, -20,
						0, 0, 0, 0, -20, -40, -30, 0, 15, 20,
						20, 15, 0, -30, -30, 5, 20, 25, 25, 20,
						5, -30, -30, 5, 20, 25, 25, 20, 5, -30,
						-30, 0, 15, 20, 20, 15, 0, -30, -40, -20,
						0, 5, 5, 0, -20, -40, -50, -40, -30, -30,
						-30, -30, -40, -50
					};
					int num6 = ((!IsEndgameRightNow) ? ((piece.Color == Piece.PieceColor.White) ? array5[63 - i] : (-array5[i])) : ((piece.Color == Piece.PieceColor.White) ? array6[63 - i] : (-array6[i])));
					num += num6;
				}
				else if (piece.Type == Piece.PieceType.Rook)
				{
					int[] array7 = new int[64]
					{
						0, 0, 0, 5, 5, 0, 0, 0, -5, 5,
						5, 5, 5, 5, 5, -5, -5, 0, 0, 0,
						0, 0, 0, -5, -5, 0, 0, 0, 0, 0,
						0, -5, -5, 0, 0, 0, 0, 0, 0, -5,
						-5, 0, 0, 0, 0, 0, 0, -5, 5, 10,
						10, 10, 10, 10, 10, 5, 0, 0, 0, 5,
						5, 0, 0, 0
					};
					int[] array8 = new int[64]
					{
						0, 0, 0, 0, 0, 0, 0, 0, 10, 10,
						10, 10, 10, 10, 10, 10, 15, 15, 15, 15,
						15, 15, 15, 15, 20, 20, 20, 20, 20, 20,
						20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
						15, 15, 15, 15, 15, 15, 15, 15, 10, 10,
						10, 10, 10, 10, 10, 10, 0, 0, 0, 0,
						0, 0, 0, 0
					};
					int num7 = ((!IsEndgameRightNow) ? ((piece.Color == Piece.PieceColor.White) ? array7[63 - i] : (-array7[i])) : ((piece.Color == Piece.PieceColor.White) ? array8[63 - i] : (-array8[i])));
					num += num7;
					int num8 = CalculateRookMobility(i);
					num += ((piece.Color == Piece.PieceColor.White) ? (num8 * 2) : (-num8 * 2));
				}
				else if (piece.Type == Piece.PieceType.Queen)
				{
					int[] array9 = new int[64]
					{
						-20, -10, -10, -5, -5, -10, -10, -20, -10, 0,
						0, 0, 0, 0, 0, -10, -10, 0, 5, 5,
						5, 5, 0, -10, -5, 0, 5, 5, 5, 5,
						0, -5, 0, 0, 5, 5, 5, 5, 0, -5,
						-10, 5, 5, 5, 5, 5, 0, -10, -10, 0,
						5, 0, 0, 0, 0, -10, -20, -10, -10, -5,
						-5, -10, -10, -20
					};
					int[] array10 = new int[64]
					{
						-20, -10, -10, -5, -5, -10, -10, -20, -10, 0,
						0, 0, 0, 0, 0, -10, -10, 0, 10, 10,
						10, 10, 0, -10, -5, 0, 10, 10, 10, 10,
						0, -5, 0, 0, 10, 10, 10, 10, 0, -5,
						-10, 0, 10, 10, 10, 10, 0, -10, -10, 0,
						0, 0, 0, 0, 0, -10, -20, -10, -10, -5,
						-5, -10, -10, -20
					};
					int num9 = ((!IsEndgameRightNow) ? ((piece.Color == Piece.PieceColor.White) ? array9[63 - i] : (-array9[i])) : ((piece.Color == Piece.PieceColor.White) ? array10[63 - i] : (-array10[i])));
					num += num9;
				}
				else if (piece.Type == Piece.PieceType.King)
				{
					int[] array11 = new int[64]
					{
						-30, -40, -40, -50, -50, -40, -40, -30, -30, -40,
						-40, -50, -50, -40, -40, -30, -30, -40, -40, -50,
						-50, -40, -40, -30, -30, -40, -40, -50, -50, -40,
						-40, -30, -20, -30, -30, -40, -40, -30, -30, -20,
						-10, -20, -20, -20, -20, -20, -20, -10, 20, 20,
						0, 0, 0, 0, 20, 20, 30, 40, 10, 0,
						0, 10, 40, 30
					};
					int[] array12 = new int[64]
					{
						-50, -30, -30, -30, -30, -30, -30, -50, -30, -30,
						0, 0, 0, 0, -30, -30, -30, -10, 20, 30,
						30, 20, -10, -30, -30, -10, 30, 40, 40, 30,
						-10, -30, -30, -10, 30, 40, 40, 30, -10, -30,
						-30, -10, 20, 30, 30, 20, -10, -30, -30, -20,
						-10, 0, 0, -10, -20, -30, -50, -40, -30, -20,
						-20, -30, -40, -50
					};
					int num10 = ((!IsEndgameRightNow) ? ((piece.Color == Piece.PieceColor.White) ? array11[63 - i] : (-array11[i])) : ((piece.Color == Piece.PieceColor.White) ? array12[63 - i] : (-array12[i])));
					num += num10;
				}
			}
		}
		num += EvaluatePieceStructure();
		if (Board.WhiteHasCastled)
		{
			num += 50;
		}
		if (Board.BlackHasCastled)
		{
			num -= 50;
		}
		if (Board.IsKingInCheck(Piece.PieceColor.White))
		{
			num -= 25;
		}
		else if (Board.IsKingInCheck(Piece.PieceColor.Black))
		{
			num += 25;
		}
		if (move != null)
		{
			num = EvaluateRepetition(move, num);
		}
		if (Board.InsufficientMaterial())
		{
			num = 0;
		}
		return num;
	}

	private int CalculateRookMobility(int squareIndex)
	{
		int num = 0;
		int num2 = squareIndex / 8;
		int num3 = squareIndex % 8;
		Piece piece = Board.GetPiece(squareIndex);
		int num4 = num3 - 1;
		while (num4 >= 0)
		{
			Piece piece2 = Board.GetPiece(num2 * 8 + num4);
			if (piece2.Type == Piece.PieceType.None)
			{
				num++;
				num4--;
				continue;
			}
			if (piece2.Color != piece.Color)
			{
				num++;
			}
			break;
		}
		for (int i = num3 + 1; i < 8; i++)
		{
			Piece piece3 = Board.GetPiece(num2 * 8 + i);
			if (piece3.Type == Piece.PieceType.None)
			{
				num++;
				continue;
			}
			if (piece3.Color != piece.Color)
			{
				num++;
			}
			break;
		}
		int num5 = num2 - 1;
		while (num5 >= 0)
		{
			Piece piece4 = Board.GetPiece(num5 * 8 + num3);
			if (piece4.Type == Piece.PieceType.None)
			{
				num++;
				num5--;
				continue;
			}
			if (piece4.Color != piece.Color)
			{
				num++;
			}
			break;
		}
		for (int j = num2 + 1; j < 8; j++)
		{
			Piece piece5 = Board.GetPiece(j * 8 + num3);
			if (piece5.Type == Piece.PieceType.None)
			{
				num++;
				continue;
			}
			if (piece5.Color != piece.Color)
			{
				num++;
			}
			break;
		}
		return num;
	}

	private bool MovesAreEqual(Move move1, Move move2)
	{
		if (move1.From == move2.To)
		{
			return move1.To == move2.From;
		}
		return false;
	}

	private int EvaluateRepetition(Move currentMove, int currentEval)
	{
		int num = currentEval;
		bool flag = Board.GetPiece(currentMove.To).Color == Piece.PieceColor.White;
		if (MovesAreEqual(currentMove, lastMove) && MovesAreEqual(lastMove, doubleLastMove) && MovesAreEqual(doubleLastMove, tripleLastMove))
		{
			num = 0;
		}
		else if (MovesAreEqual(currentMove, lastMove) && MovesAreEqual(lastMove, doubleLastMove) && ((flag && num > 100) || (!flag && num < -100)))
		{
			num -= (flag ? 200 : (-200));
		}
		return num;
	}

	private int EvaluatePieceStructure()
	{
		int num = 0;
		int[] array = new int[8];
		int[] array2 = new int[8];
		int[] array3 = new int[8];
		int[] array4 = new int[8];
		for (int i = 0; i < 64; i++)
		{
			int num2 = i % 8;
			Piece piece = Board.GetPiece(i);
			if (piece.Color == Piece.PieceColor.White)
			{
				array3[num2]++;
				if (piece.Type == Piece.PieceType.Pawn)
				{
					array[num2]++;
				}
				else if (piece.Type == Piece.PieceType.King)
				{
					num -= CountLegalMoves(i) * 2;
				}
			}
			else if (piece.Color == Piece.PieceColor.Black)
			{
				array4[num2]++;
				if (piece.Type == Piece.PieceType.Pawn)
				{
					array2[num2]++;
				}
				else if (piece.Type == Piece.PieceType.King)
				{
					num += CountLegalMoves(i) * 2;
				}
			}
			if (piece.Type == Piece.PieceType.Rook)
			{
				num += EvaluateRookFile(piece.Color, num2);
			}
		}
		for (int j = 0; j < 8; j++)
		{
			if (array3[j] > 3)
			{
				num -= 5 * (array3[j] - 3);
			}
			if (array4[j] > 3)
			{
				num += 5 * (array4[j] - 3);
			}
		}
		for (int k = 0; k < 8; k++)
		{
			if (array[k] > 1)
			{
				num -= 10 * (array[k] - 1);
			}
			if (array2[k] > 1)
			{
				num += 10 * (array2[k] - 1);
			}
			bool flag = (k == 0 || array[k - 1] == 0) && (k == 7 || array[k + 1] == 0);
			bool num3 = (k == 0 || array2[k - 1] == 0) && (k == 7 || array2[k + 1] == 0);
			if (flag && array[k] > 0)
			{
				num -= 10;
			}
			if (num3 && array2[k] > 0)
			{
				num += 10;
			}
			bool flag2 = array[k] > 0 && (k == 0 || array2[k - 1] == 0) && array2[k] == 0 && (k == 7 || array2[k + 1] == 0);
			bool num4 = array2[k] > 0 && (k == 0 || array[k - 1] == 0) && array[k] == 0 && (k == 7 || array[k + 1] == 0);
			if (flag2)
			{
				num += 20;
			}
			if (num4)
			{
				num -= 20;
			}
		}
		return num;
	}

	private int EvaluateRookFile(Piece.PieceColor rookColor, int file)
	{
		if (IsOpenFile(file))
		{
			if (rookColor != Piece.PieceColor.White)
			{
				return -30;
			}
			return 30;
		}
		if (IsSemiOpenFile(file, rookColor))
		{
			if (rookColor != Piece.PieceColor.White)
			{
				return -15;
			}
			return 15;
		}
		return 0;
	}

	private bool IsSemiOpenFile(int file, Piece.PieceColor color)
	{
		for (int i = 0; i < 8; i++)
		{
			Piece piece = Board.GetPiece(i * 8 + file);
			if (piece.Type == Piece.PieceType.Pawn && piece.Color == color)
			{
				return false;
			}
		}
		return true;
	}

	private int CountLegalMoves(int square)
	{
		return Board.FindValidMoves(square).Count;
	}

	private bool IsEndgame()
	{
		int num = 0;
		bool flag = false;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type != Piece.PieceType.None && piece.Type != Piece.PieceType.King)
			{
				num++;
				if (piece.Type == Piece.PieceType.Queen)
				{
					flag = true;
				}
			}
		}
		if (num > 8 || flag)
		{
			return num <= 4 && flag;
		}
		return true;
	}
}
