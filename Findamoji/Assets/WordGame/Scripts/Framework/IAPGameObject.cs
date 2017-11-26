using UnityEngine;
using System.Collections;

public class IAPGameObject : MonoBehaviour
{
	#region Unity Methods

	private void Start()
	{
		// Deactivates this GameObject if IAP is not enabled
		gameObject.SetActive(IAPController.IsEnabled);
	}

	#endregion
}
