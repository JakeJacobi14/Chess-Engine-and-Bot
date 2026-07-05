using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot3 : MonoBehaviour
{
	public bool _isWhite;

	[SerializeField]
	private int depth = 4;

	[SerializeField]
	private bool _pieceMoveAnimation = true;

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
		if (isWhiteTurn == _isWhite && !isThinking)
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

	public Move CalculateMove(int depth, bool isWhite, int alpha, int beta)
	{
		List<Move> list = new List<Move>();
		int num = (isWhite ? int.MinValue : int.MaxValue);
		bool flag = false;
		List<Move> list2 = new List<Move>();
		for (int i = 0; i < 64; i++)
		{
			if (Board.GetPiece(i).Color == Piece.PieceColor.White != isWhite)
			{
				continue;
			}
			foreach (int item in Board.FindValidMoves(i))
			{
				list2.Add(new Move
				{
					From = i,
					To = item
				});
			}
		}
		list2.Sort((Move move3, Move move2) => EvaluateMove(move2, isWhite).CompareTo(EvaluateMove(move3, isWhite)));
		foreach (Move item2 in list2)
		{
			flag = true;
			Board.MakeMove(item2);
			int num2;
			if (Board.IsKingInCheck((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black) && Board.IsCheckmate((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black))
			{
				num2 = (isWhite ? int.MaxValue : int.MinValue);
			}
			else if (depth > 0)
			{
				Move move = new Move();
				if (Board.IsKingInCheck(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black))
				{
					depth++;
				}
				move = CalculateMove(depth - 1, !isWhite, alpha, beta);
				if (move.From == -1)
				{
					num2 = (Board.IsKingInCheck(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black) ? (isWhite ? int.MinValue : int.MaxValue) : 0);
				}
				else
				{
					Board.MakeMove(move);
					num2 = CalculateEvaluation();
					Board.UnMakeMove(move);
				}
			}
			else
			{
				num2 = CalculateEvaluation();
			}
			Board.UnMakeMove(item2);
			if (isWhite)
			{
				if (num2 > num)
				{
					num = num2;
					alpha = Mathf.Max(alpha, num2);
					list.Clear();
					list.Add(item2);
				}
				else if (num2 == num)
				{
					list.Add(item2);
				}
				if (alpha >= beta)
				{
					break;
				}
			}
			else
			{
				if (num2 < num)
				{
					num = num2;
					beta = Mathf.Min(beta, num2);
					list.Clear();
					list.Add(item2);
				}
				else if (num2 == num)
				{
					list.Add(item2);
				}
				if (beta <= alpha)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			Debug.Log(Board.IsKingInCheck(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black) ? "Checkmate!" : "Stalemate!");
			return new Move
			{
				From = -1,
				To = -1
			};
		}
		if (list.Count > 0)
		{
			int index = Random.Range(0, list.Count);
			return list[index];
		}
		Debug.LogError("No best moves found. This should not happen.");
		return new Move
		{
			From = -1,
			To = -1
		};
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
		int num2 = move.To / 8;
		int num3 = move.To % 8;
		if (num2 >= 2 && num2 <= 5 && num3 >= 2 && num3 <= 5)
		{
			num += 10;
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
			if (piece.Type == Piece.PieceType.Bishop || piece.Type == Piece.PieceType.Knight || piece.Type == Piece.PieceType.Queen)
			{
				int num3 = i / 8;
				int num4 = i % 8;
				if (num3 >= 2 && num3 <= 5 && num4 >= 2 && num4 <= 5)
				{
					num += ((piece.Color == Piece.PieceColor.White) ? 12 : (-12));
				}
			}
			if (piece.Type == Piece.PieceType.King)
			{
				int num5 = i / 8;
				if (IsEndgame() && num5 >= 2 && num5 <= 5)
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
				int num6 = i / 8;
				int num7 = i % 8;
				num = ((piece.Color != Piece.PieceColor.White) ? (num - (6 - num6) * 4) : (num + (num6 - 1) * 4));
				if (num6 >= 2 && num6 <= 5 && num7 >= 2 && num7 <= 5)
				{
					num += ((piece.Color == Piece.PieceColor.White) ? 12 : (-12));
				}
			}
		}
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
