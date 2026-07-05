using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
	[Header("SFX")]
	[SerializeField]
	private AudioClip _captureSFX;

	[SerializeField]
	private AudioClip _moveSFX;

	[SerializeField]
	private AudioClip _castleSFX;

	[SerializeField]
	private AudioClip _checkSFX;

	[SerializeField]
	private AudioClip _promoteSFX;

	[Space(20f)]
	[SerializeField]
	private GameObject _validMoveSquare;

	[SerializeField]
	private GameObject Outline;

	[SerializeField]
	private TMP_Text _moveTimeText;

	[SerializeField]
	private TMP_Text _lastMoveText;

	[SerializeField]
	private TMP_Text _evaluationText;

	[SerializeField]
	private TMP_Text _whiteMaterial;

	[SerializeField]
	private TMP_Text _blackMaterial;

	[SerializeField]
	private GameObject CheckMarking;

	[SerializeField]
	private GameObject _chessBot;

	private bool boardFlipped;

	private const int LAST_MOVES_COUNT = 8;

	private Move[] lastMoves = new Move[8];

	private int moveCount;

	private float startTime;

	private Dictionary<Piece.PieceType, int> pieceValues = new Dictionary<Piece.PieceType, int>
	{
		{
			Piece.PieceType.Pawn,
			1
		},
		{
			Piece.PieceType.Knight,
			3
		},
		{
			Piece.PieceType.Bishop,
			3
		},
		{
			Piece.PieceType.Rook,
			5
		},
		{
			Piece.PieceType.Queen,
			9
		},
		{
			Piece.PieceType.King,
			0
		}
	};

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 vector = Input.mousePosition;
			Vector2 worldPosition = Camera.main.ScreenToWorldPoint(new Vector2(vector.x, vector.y));
			int squareIndex = GetSquareIndex(worldPosition);
			GameObject[] array = GameObject.FindGameObjectsWithTag("ValidMoveSquare");
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i]);
			}
			if (Board.GameOver)
			{
				return;
			}
			if (Board.GetPiece(squareIndex).Color == Piece.PieceColor.White == Board.IsWhitesTurn)
			{
				foreach (int item in Board.FindValidMoves(squareIndex))
				{
					UnityEngine.Object.Instantiate(_validMoveSquare, new Vector2((float)(item % 8) - 4f, (float)(item / 8) - 4f), Quaternion.identity).GetComponent<MovePiece>().ParentMove = new Move
					{
						From = squareIndex,
						To = item
					};
				}
			}
		}
		if (Board.HalfmoveClock >= 50)
		{
			Board.GameOver = true;
		}
	}

	private int GetKingPosition(Piece.PieceColor color)
	{
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type == Piece.PieceType.King && piece.Color == color)
			{
				return i;
			}
		}
		throw new Exception("King not found on the board!");
	}

	public Vector2 SquareToWorldPosition(int i)
	{
		return new Vector2(i % 8 - 4, (float)(i / 8) - 3.5f);
	}

	private void AddMoveToHistory(Move move)
	{
		if (moveCount < 8)
		{
			lastMoves[moveCount] = move;
			moveCount++;
			return;
		}
		for (int i = 0; i < 7; i++)
		{
			lastMoves[i] = lastMoves[i + 1];
		}
		lastMoves[7] = move;
	}

	private bool IsThreeFoldRepetition()
	{
		if (moveCount < 8)
		{
			return false;
		}
		Move move = lastMoves[moveCount - 1];
		Move move2 = lastMoves[moveCount - 2];
		Move move3 = lastMoves[moveCount - 3];
		Move move4 = lastMoves[moveCount - 4];
		Move move5 = lastMoves[moveCount - 5];
		Move move6 = lastMoves[moveCount - 6];
		Move move7 = lastMoves[moveCount - 7];
		Move move8 = lastMoves[moveCount - 8];
		if (MovesAreEqual(move, move3) && MovesAreEqual(move2, move4) && MovesAreEqual(move3, move5) && MovesAreEqual(move4, move6) && MovesAreEqual(move5, move7) && MovesAreEqual(move6, move8))
		{
			return true;
		}
		return false;
	}

	private bool MovesAreEqual(Move move1, Move move2)
	{
		if (move1.From == move2.To)
		{
			return move1.To == move2.From;
		}
		return false;
	}

	public void UpdateTexts(Move move)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("CheckMarking");
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
		if (Board.IsKingInCheck(Piece.PieceColor.White))
		{
			int kingPosition = GetKingPosition(Piece.PieceColor.White);
			Vector3 position = SquareToWorldPosition(kingPosition);
			UnityEngine.Object.Instantiate(CheckMarking, position, Quaternion.identity);
		}
		else if (Board.IsKingInCheck(Piece.PieceColor.Black))
		{
			int kingPosition2 = GetKingPosition(Piece.PieceColor.Black);
			Vector3 position2 = SquareToWorldPosition(kingPosition2);
			UnityEngine.Object.Instantiate(CheckMarking, position2, Quaternion.identity);
		}
		Piece piece = Board.GetPiece(move.To);
		int num = move.From % 8;
		_ = move.From / 8;
		int num2 = move.To % 8;
		int num3 = move.To / 8;
		string text = "";
		if (piece.Type == Piece.PieceType.King && Math.Abs(num - num2) == 2)
		{
			text = ((num2 > num) ? "O-O" : "O-O-O");
		}
		else
		{
			if (piece.Type != Piece.PieceType.Pawn)
			{
				text += GetPieceSymbol(piece.Type);
			}
			if (piece.Type == Piece.PieceType.Pawn && move.PreviousPiece.Type != Piece.PieceType.None)
			{
				text += (char)(97 + num);
			}
			bool flag = IsEnPassant(move);
			if (move.PreviousPiece.Type != Piece.PieceType.None || flag)
			{
				text += "x";
			}
			text += (char)(97 + num2);
			text += (char)(49 + num3);
			if (move.WasPromotion)
			{
				text += "=Q";
			}
		}
		_lastMoveText.text = "Move: " + text;
		AddMoveToHistory(move);
		if (Board.InsufficientMaterial())
		{
			_lastMoveText.text = "Draw by insufficient material!";
			Board.GameOver = true;
		}
		if (IsThreeFoldRepetition())
		{
			_lastMoveText.text = "Draw by three-fold repetition!";
			Board.GameOver = true;
		}
		if (Board.HalfmoveClock >= 50)
		{
			_lastMoveText.text = "Draw by 50 move rule!";
			Board.GameOver = true;
		}
		if (Board.IsCheckmate(Piece.PieceColor.White))
		{
			_lastMoveText.text = "Black wins by checkmate!";
			Board.GameOver = true;
		}
		else if (Board.IsCheckmate(Piece.PieceColor.Black))
		{
			_lastMoveText.text = "White wins by checkmate!";
			Board.GameOver = true;
		}
		else if (Board.IsStaleMate())
		{
			_lastMoveText.text = "Stalemate!";
			Board.GameOver = true;
		}
		if (move.Evaluation != 0)
		{
			_evaluationText.text = $"Eval: {(float)move.Evaluation / 100f}";
		}
		if (move.Evaluation == 0)
		{
			_evaluationText.text = "Eval: 0.00";
		}
		_whiteMaterial.text = $"White Material: {CalculateMaterial(Piece.PieceColor.White)}";
		_blackMaterial.text = $"Black Material: {CalculateMaterial(Piece.PieceColor.Black)}";
		float num4 = Time.realtimeSinceStartup - startTime;
		_moveTimeText.text = $"Time: {num4:F3}";
		startTime = Time.realtimeSinceStartup;
	}

	private bool IsEnPassant(Move move)
	{
		if (Board.GetPiece(move.To).Type != Piece.PieceType.Pawn)
		{
			return false;
		}
		int num = move.From % 8;
		int num2 = move.To % 8;
		if (num != num2)
		{
			return move.PreviousPiece.Type == Piece.PieceType.None;
		}
		return false;
	}

	private int CalculateMaterial(Piece.PieceColor color)
	{
		int num = 0;
		for (int i = 0; i < 64; i++)
		{
			Piece piece = Board.GetPiece(i);
			if (piece.Type != Piece.PieceType.None && piece.Color == color)
			{
				num += pieceValues[piece.Type];
			}
		}
		return num;
	}

	public void PlaySound(Move move)
	{
		if (Board.IsKingInCheck((Board.GetPiece(move.To).Color != Piece.PieceColor.White) ? Piece.PieceColor.White : Piece.PieceColor.Black))
		{
			AudioSource.PlayClipAtPoint(_checkSFX, Vector2.zero);
		}
		else if (move.PreviousPiece.Type == Piece.PieceType.None)
		{
			if (Board.GetPiece(move.To).Type == Piece.PieceType.King && Mathf.Abs(move.To - move.From) == 2)
			{
				AudioSource.PlayClipAtPoint(_castleSFX, Vector2.zero);
			}
			else
			{
				AudioSource.PlayClipAtPoint(_moveSFX, Vector2.zero);
			}
		}
		else if (move.WasPromotion)
		{
			AudioSource.PlayClipAtPoint(_promoteSFX, Vector2.zero);
		}
		else
		{
			AudioSource.PlayClipAtPoint(_captureSFX, Vector2.zero);
		}
	}

	public void RestartGame()
	{
		for (int i = 0; i < 64; i++)
		{
			Board.Squares[i] = new Piece(Piece.PieceType.None, Piece.PieceColor.None);
		}
		Board.WhiteHasCastled = false;
		Board.BlackHasCastled = false;
		Board.IsWhitesTurn = true;
		Board.EnPassantTargetSquare = "-";
		Board.HalfmoveClock = 0;
		Board.IsUpdating = false;
		Board.CanWhiteCastleKingside = true;
		Board.CanWhiteCastleQueenside = true;
		Board.CanBlackCastleKingside = true;
		Board.CanBlackCastleQueenside = true;
		Board.GameOver = false;
		lastMoves = new Move[8];
		moveCount = 0;
		startTime = 0f;
		GameObject[] array = GameObject.FindGameObjectsWithTag("CheckMarking");
		for (int j = 0; j < array.Length; j++)
		{
			UnityEngine.Object.Destroy(array[j]);
		}
		array = GameObject.FindGameObjectsWithTag("Outline");
		for (int j = 0; j < array.Length; j++)
		{
			UnityEngine.Object.Destroy(array[j]);
		}
		if (boardFlipped)
		{
			FlipBoard();
		}
		array = GameObject.FindGameObjectsWithTag("ChessBot");
		for (int j = 0; j < array.Length; j++)
		{
			UnityEngine.Object.Destroy(array[j]);
		}
		GameObject.FindWithTag("SetupBoard").GetComponent<SetupBoard>().InitializeBoard();
		GameObject.FindWithTag("SetupBoard").GetComponent<SetupBoard>().UpdateBoard();
	}

	private char GetPieceSymbol(Piece.PieceType type)
	{
		return type switch
		{
			Piece.PieceType.King => 'K', 
			Piece.PieceType.Queen => 'Q', 
			Piece.PieceType.Rook => 'R', 
			Piece.PieceType.Bishop => 'B', 
			Piece.PieceType.Knight => 'N', 
			_ => ' ', 
		};
	}

	private int GetSquareIndex(Vector2 worldPosition)
	{
		float f = worldPosition.x + 4f;
		float f2 = worldPosition.y + 4f;
		int value = Mathf.RoundToInt(f);
		int value2 = Mathf.RoundToInt(f2);
		value = Math.Clamp(value, 0, 7);
		return Math.Clamp(value2, 0, 7) * 8 + value;
	}

	public void MakeOutline(Move move)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Outline");
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
		UnityEngine.Object.Instantiate(Outline, new Vector2((float)(move.From % 8) - 4f, (float)(move.From / 8) - 4f), Quaternion.identity);
		UnityEngine.Object.Instantiate(Outline, new Vector2((float)(move.To % 8) - 4f, (float)(move.To / 8) - 4f), Quaternion.identity);
	}

	public void FlipBoard()
	{
		GameObject.FindWithTag("SetupBoard").GetComponent<SetupBoard>().FlipBoard();
		boardFlipped = !boardFlipped;
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void PlayerVsPlayer()
	{
		RestartGame();
	}

	public void PlayWhite()
	{
		RestartGame();
		UnityEngine.Object.Instantiate(_chessBot, Vector2.zero, Quaternion.identity).GetComponent<ChessBot6>()._isWhite = false;
	}

	public void PlayBlack()
	{
		RestartGame();
		UnityEngine.Object.Instantiate(_chessBot, Vector2.zero, Quaternion.identity);
		FlipBoard();
	}

	public void AIvsAi()
	{
		RestartGame();
		GameObject obj = UnityEngine.Object.Instantiate(_chessBot, Vector2.zero, Quaternion.identity);
		obj.GetComponent<ChessBot6>()._isWhite = false;
		obj.GetComponent<ChessBot6>().depth = 4;
		UnityEngine.Object.Instantiate(_chessBot, Vector2.zero, Quaternion.identity).GetComponent<ChessBot6>().depth = 4;
	}
}
