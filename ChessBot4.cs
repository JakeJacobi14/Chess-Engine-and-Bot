using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot4 : MonoBehaviour
{
	public bool _isWhite;

	[SerializeField]
	private int depth = 4;

	[SerializeField]
	private bool _pieceMoveAnimation = true;

	private Dictionary<int, int> pieceMoveCounts = new Dictionary<int, int>();

	private const int OPENING_MOVE_LIMIT = 8;

	private int currentMoveCount;

	private bool isThinking;

	private Dictionary<Piece.PieceType, int> pieceValues = new Dictionary<Piece.PieceType, int>
	{
		{
			Piece.PieceType.Pawn,
			100
		},
		{
			Piece.PieceType.Knight,
			320
		},
		{
			Piece.PieceType.Bishop,
			330
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
		if (isWhiteTurn == _isWhite && !isThinking && !Board.GameOver)
		{
			StartCoroutine(MakeBotMoveCoroutine());
		}
	}

	private IEnumerator MakeBotMoveCoroutine()
	{
		isThinking = true;
		yield return null;
		while (Board.IsUpdating)
		{
			yield return null;
		}
		int num = depth;
		if (IsEndgame())
		{
			num++;
			Debug.Log("Endgame detected. Increasing depth to " + num);
		}
		Move botMove = CalculateMove(num, _isWhite, int.MinValue, int.MaxValue);
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
		}
		else
		{
			Debug.Log(_isWhite ? "White" : "Black has no valid moves. Game over.");
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
			int evaluation2 = ((!isWhite) ? Mathf.Max(0, -num * 9 / 10) : Mathf.Min(0, -num * 9 / 10));
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
			Move move3 = CalculateMove(depth - 1, !isWhite, alpha, beta);
			Board.UnMakeMove(item2);
			if (isWhite)
			{
				if (move3.Evaluation > move2.Evaluation)
				{
					move2 = item2;
					move2.Evaluation = move3.Evaluation;
				}
				alpha = Mathf.Max(alpha, move3.Evaluation);
			}
			else
			{
				if (move3.Evaluation < move2.Evaluation)
				{
					move2 = item2;
					move2.Evaluation = move3.Evaluation;
				}
				beta = Mathf.Min(beta, move3.Evaluation);
			}
			if (beta <= alpha)
			{
				break;
			}
		}
		return move2;
	}

	private bool IsCaptureOrCheck(Move move, bool isWhite)
	{
		if (Board.GetPiece(move.To).Type != Piece.PieceType.None)
		{
			return true;
		}
		Board.MakeMove(move);
		bool result = Board.IsKingInCheck((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black);
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
			num += pieceValues[piece2.Type] - pieceValues[piece.Type];
		}
		if (piece.Type == Piece.PieceType.Pawn && (move.To / 8 == 0 || move.To / 8 == 7))
		{
			num += pieceValues[Piece.PieceType.Queen];
		}
		int num2 = move.To / 8;
		int num3 = move.To % 8;
		if (num2 >= 2 && num2 <= 5 && num3 >= 2 && num3 <= 5)
		{
			num += 20;
		}
		return num;
	}

	private int CalculateEvaluation()
	{
		int num = 0;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type == Piece.PieceType.None)
			{
				continue;
			}
			int num2 = pieceValues[piece.Type];
			num += ((piece.Color == Piece.PieceColor.White) ? num2 : (-num2));
			if (currentMoveCount <= 8)
			{
				int num3 = pieceMoveCounts[i] * 20;
				num -= ((piece.Color == Piece.PieceColor.White) ? num3 : (-num3));
			}
			if (piece.Type == Piece.PieceType.Bishop || piece.Type == Piece.PieceType.Knight || piece.Type == Piece.PieceType.Queen)
			{
				int num4 = i / 8;
				int num5 = i % 8;
				if (num4 >= 2 && num4 <= 5 && num5 >= 2 && num5 <= 5)
				{
					num += ((piece.Color == Piece.PieceColor.White) ? 15 : (-15));
				}
			}
			if (piece.Type == Piece.PieceType.Rook)
			{
				int num6 = i / 8;
				num += (((piece.Color == Piece.PieceColor.White && num6 == 6) || (piece.Color == Piece.PieceColor.Black && num6 == 1)) ? 20 : 0);
			}
			if (piece.Type == Piece.PieceType.King)
			{
				int num7 = i / 8;
				if (IsEndgame() && num7 >= 2 && num7 <= 5)
				{
					num += ((piece.Color == Piece.PieceColor.White) ? 15 : (-15));
				}
				if (piece.IsFirstMove)
				{
					num += ((piece.Color == Piece.PieceColor.White) ? 10 : (-10));
				}
			}
			if (piece.Type == Piece.PieceType.Pawn)
			{
				int num8 = i / 8;
				int num9 = i % 8;
				num = ((piece.Color != Piece.PieceColor.White) ? (num - (6 - num8) * 4) : (num + (num8 - 1) * 4));
				if (num9 >= 2 && num9 <= 5)
				{
					num += ((piece.Color == Piece.PieceColor.White) ? 10 : (-10));
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
		return num;
	}

	private int EvaluatePieceStructure()
	{
		int num = 0;
		int[] array = new int[8];
		int[] array2 = new int[8];
		int[] array3 = new int[16];
		int[] array4 = new int[16];
		for (int i = 0; i < 64; i++)
		{
			int num2 = i % 8;
			Piece piece = Board.GetPiece(i);
			if (piece.Type == Piece.PieceType.None)
			{
				continue;
			}
			if (piece.Color == Piece.PieceColor.White)
			{
				array3[num2]++;
				if (piece.Type == Piece.PieceType.Pawn)
				{
					array[num2]++;
				}
				else if (piece.Type == Piece.PieceType.Rook && array3[num2] == 0)
				{
					num += 30 - array4[num2] * 5;
				}
			}
			else
			{
				array4[num2]++;
				if (piece.Type == Piece.PieceType.Pawn)
				{
					array2[num2]++;
				}
				else if (piece.Type == Piece.PieceType.Rook && array4[num2] == 0)
				{
					num -= 30 - array3[num2] * 5;
				}
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
		if (num < 7)
		{
			return !flag;
		}
		return false;
	}
}
