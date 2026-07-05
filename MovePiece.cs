using UnityEngine;

public class MovePiece : MonoBehaviour
{
	public Move ParentMove;

	private void OnMouseDown()
	{
		Board.ReallyMakeMove(ParentMove, doAnimation: true);
	}
}
