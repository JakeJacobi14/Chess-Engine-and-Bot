using System.Collections;
using UnityEngine;

public class SetupBoard : MonoBehaviour
{
	[SerializeField]
	private GameObject Square;

	[SerializeField]
	private Color _lightColor;

	[SerializeField]
	private Color _darkColor;

	[SerializeField]
	private GameObject[] WhitePieceGameobjects;

	[SerializeField]
	private GameObject[] BlackPieceGameobjects;

	private Quaternion rotation = Quaternion.identity;

	private bool pieceIsMoving;

	private void Start()
	{
		DrawBoard();
		InitializeBoard();
		PlacePiecePrefabs();
	}

	public void FlipBoard()
	{
		Camera.main.transform.Rotate(0f, 0f, 180f);
		if (rotation == Quaternion.Euler(0f, 0f, 0f))
		{
			rotation = Quaternion.Euler(0f, 0f, 180f);
		}
		else
		{
			rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		if (!pieceIsMoving)
		{
			UpdateBoard();
		}
	}

	private void DrawBoard()
	{
		for (int i = -4; i < 4; i++)
		{
			for (int j = -4; j < 4; j++)
			{
				GameObject gameObject = Object.Instantiate(Square, new Vector2(i, j), Quaternion.identity);
				if ((i + j) % 2 == 0)
				{
					gameObject.GetComponent<SpriteRenderer>().color = _darkColor;
				}
				else
				{
					gameObject.GetComponent<SpriteRenderer>().color = _lightColor;
				}
				gameObject.transform.SetParent(base.transform);
			}
		}
	}

	public void InitializeBoard()
	{
		for (int i = 0; i < 8; i++)
		{
			Board.PlacePiece(new Piece(Piece.PieceType.Pawn, Piece.PieceColor.White), i + 8);
			Board.PlacePiece(new Piece(Piece.PieceType.Pawn, Piece.PieceColor.Black), i + 48);
		}
		SetupBackRank(0, Piece.PieceColor.White);
		SetupBackRank(56, Piece.PieceColor.Black);
	}

	private void SetupBackRank(int startIndex, Piece.PieceColor color)
	{
		Board.PlacePiece(new Piece(Piece.PieceType.Rook, color), startIndex);
		Board.PlacePiece(new Piece(Piece.PieceType.Knight, color), startIndex + 1);
		Board.PlacePiece(new Piece(Piece.PieceType.Bishop, color), startIndex + 2);
		Board.PlacePiece(new Piece(Piece.PieceType.Queen, color), startIndex + 3);
		Board.PlacePiece(new Piece(Piece.PieceType.King, color), startIndex + 4);
		Board.PlacePiece(new Piece(Piece.PieceType.Bishop, color), startIndex + 5);
		Board.PlacePiece(new Piece(Piece.PieceType.Knight, color), startIndex + 6);
		Board.PlacePiece(new Piece(Piece.PieceType.Rook, color), startIndex + 7);
	}

	private void PlacePiecePrefabs()
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				Piece piece = Board.GetPiece(i * 8 + j);
				if (piece.Type != Piece.PieceType.None)
				{
					PlacePiecePrefab(piece, j - 4, i - 4);
				}
			}
		}
	}

	private void PlacePiecePrefab(Piece piece, float x, float y)
	{
		GameObject[] array = ((piece.Color == Piece.PieceColor.White) ? WhitePieceGameobjects : BlackPieceGameobjects);
		int num = (int)(piece.Type - 1);
		if (num >= 0 && num < array.Length)
		{
			Object.Instantiate(array[num], new Vector2(x, y), rotation).transform.SetParent(base.transform);
		}
		else
		{
			Debug.LogError($"Invalid piece index: {num} for piece type: {piece.Type}");
		}
	}

	public void UpdateBoard()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Piece");
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
		PlacePiecePrefabs();
	}

	public void UpdateMovedPiece(Move move)
	{
		Board.IsUpdating = true;
		Vector3 vector = IndexToCoordinates(move.From);
		Vector3 targetPosition = IndexToCoordinates(move.To);
		GameObject gameObject = null;
		foreach (Transform item in base.transform)
		{
			if (item.position == vector && item.CompareTag("Piece"))
			{
				gameObject = item.gameObject;
				break;
			}
		}
		if (gameObject != null)
		{
			StartCoroutine(MovePieceSmoothly(gameObject, targetPosition, move));
		}
		else
		{
			Debug.LogError("No piece found at the 'From' position.");
		}
	}

	private Vector2 IndexToCoordinates(int index)
	{
		int num = index / 8;
		return new Vector2(index % 8 - 4, num - 4);
	}

	private IEnumerator MovePieceSmoothly(GameObject piece, Vector3 targetPosition, Move move)
	{
		pieceIsMoving = true;
		float duration = 0.05f;
		float elapsed = 0f;
		Vector3 initialPosition = piece.transform.position;
		while (elapsed < duration)
		{
			piece.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		piece.transform.position = targetPosition;
		UpdateBoard();
		yield return null;
		Board.IsUpdating = false;
		pieceIsMoving = false;
	}
}
