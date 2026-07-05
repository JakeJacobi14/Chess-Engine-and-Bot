using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class CalculateAllMoves : MonoBehaviour
{
	private readonly Stopwatch stopwatch = new Stopwatch();

	[SerializeField]
	private TMP_Text _outputText;

	[SerializeField]
	private TMP_Dropdown _depthDropdown;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			stopwatch.Restart();
			int num = CalculateTotalNumberOfMoves(_depthDropdown.value);
			stopwatch.Stop();
			_outputText.text = $"Total moves: {num}, Time: {stopwatch.ElapsedMilliseconds}ms";
		}
	}

	private List<Move> GenerateMoves()
	{
		List<Move> list = new List<Move>(218);
		bool isWhitesTurn = Board.IsWhitesTurn;
		for (int i = 0; i < 64; i++)
		{
			if (Board.GetPiece(i).Color != (Piece.PieceColor)(isWhitesTurn ? 1 : 2))
			{
				continue;
			}
			foreach (int item in Board.FindValidMoves(i))
			{
				list.Add(new Move
				{
					From = i,
					To = item,
					PreviousPiece = Board.GetPiece(item),
					WasFirstMove = Board.GetPiece(i).IsFirstMove
				});
			}
		}
		return list;
	}

	private int CalculateTotalNumberOfMoves(int depth)
	{
		if (depth == 0)
		{
			return 1;
		}
		List<Move> list = GenerateMoves();
		int num = 0;
		foreach (Move item in list)
		{
			Board.MakeMove(item);
			num += CalculateTotalNumberOfMoves(depth - 1);
			Board.UnMakeMove(item);
		}
		return num;
	}
}
