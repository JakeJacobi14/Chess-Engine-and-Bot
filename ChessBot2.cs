using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot2 : MonoBehaviour
{
	public bool _isWhite;

	[SerializeField]
	private int depth = 3;

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
		Move botMove = CalculateMove(num, _isWhite);
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
		}
		isThinking = false;
	}

	public Move CalculateMove(int depth, bool isWhite)
	{
		List<Move> list = new List<Move>();
		float num = (isWhite ? float.MinValue : float.MaxValue);
		bool flag = false;
		for (int i = 0; i < 64; i++)
		{
			if (Board.GetPiece(i).Color == Piece.PieceColor.White != isWhite)
			{
				continue;
			}
			foreach (int item in Board.FindValidMoves(i))
			{
				flag = true;
				Move move = new Move
				{
					From = i,
					To = item
				};
				Board.MakeMove(move);
				float num2;
				if (Board.IsKingInCheck((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black) && Board.IsCheckmate((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black))
				{
					num2 = (isWhite ? float.MaxValue : float.MinValue);
				}
				else if (depth > 0)
				{
					Move move2 = new Move();
					move2 = ((!Board.IsKingInCheck((!isWhite) ? Piece.PieceColor.White : Piece.PieceColor.Black)) ? CalculateMove(depth - 1, !isWhite) : CalculateMove(depth, !isWhite));
					if (move2.From == -1)
					{
						num2 = ((!Board.IsKingInCheck(isWhite ? Piece.PieceColor.White : Piece.PieceColor.Black)) ? 0f : (isWhite ? float.MinValue : float.MaxValue));
					}
					else
					{
						Board.MakeMove(move2);
						num2 = CalculateEvaluation();
						Board.UnMakeMove(move2);
					}
				}
				else
				{
					num2 = CalculateEvaluation();
				}
				if (isWhite ? (num2 > num) : (num2 < num))
				{
					num = num2;
					list.Clear();
					list.Add(move);
				}
				else if (num2 == num)
				{
					list.Add(move);
				}
				Board.UnMakeMove(move);
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

	private float CalculateEvaluation()
	{
		float num = 0f;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type == Piece.PieceType.None)
			{
				continue;
			}
			int num2 = pieceValues[piece.Type];
			num += (float)((piece.Color == Piece.PieceColor.White) ? num2 : (-num2));
			if (piece.Type == Piece.PieceType.Bishop || piece.Type == Piece.PieceType.Knight || piece.Type == Piece.PieceType.Queen)
			{
				int num3 = i / 8;
				int num4 = i % 8;
				if (num3 >= 2 && num3 <= 5 && num4 >= 2 && num4 <= 5)
				{
					num += (float)((piece.Color == Piece.PieceColor.White) ? 12 : (-12));
				}
			}
			if (piece.Type == Piece.PieceType.King)
			{
				int num5 = i / 8;
				if (IsEndgame() && num5 >= 2 && num5 <= 5)
				{
					num += (float)((piece.Color == Piece.PieceColor.White) ? 15 : (-15));
				}
				if (piece.IsFirstMove)
				{
					num += (float)((piece.Color == Piece.PieceColor.White) ? 10 : (-10));
				}
			}
			if (piece.Type == Piece.PieceType.Pawn)
			{
				num = ((piece.Color != Piece.PieceColor.White) ? (num - (float)((6 - i / 8) * 4)) : (num + (float)((i / 8 - 1) * 4)));
			}
		}
		if (Board.WhiteHasCastled)
		{
			num += 50f;
		}
		if (Board.BlackHasCastled)
		{
			num -= 50f;
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
		if (num < 8)
		{
			return !flag;
		}
		return false;
	}
}
