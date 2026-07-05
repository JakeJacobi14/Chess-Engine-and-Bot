using UnityEngine;

public class InfoButton : MonoBehaviour
{
	private bool _isActivated;

	[SerializeField]
	private GameObject _infoPanel;

	private void OnMouseDown()
	{
		_isActivated = !_isActivated;
		_infoPanel.SetActive(_isActivated);
	}
}
