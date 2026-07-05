using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot : MonoBehaviour
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
		Move botMove = CalculateMove(depth, _isWhite);
		if (_pieceMoveAnimation)
		{
			yield return new WaitForSeconds(0.4f);
			Board.ReallyMakeMove(botMove, doAnimation: true);
		}
		else
		{
			Board.ReallyMakeMove(botMove, doAnimation: false);
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
					Move move2 = CalculateEnemyMove(depth - 1, !isWhite);
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
			Board.GameOver = true;
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

	private Move CalculateEnemyMove(int depth, bool isWhite)
	{
		return CalculateMove(depth, isWhite);
	}

	private float CalculateEvaluation()
	{
		float num = 0f;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type != Piece.PieceType.None)
			{
				int num2 = pieceValues[piece.Type];
				num += (float)((piece.Color == Piece.PieceColor.White) ? num2 : (-num2));
			}
		}
		return num;
	}
}
