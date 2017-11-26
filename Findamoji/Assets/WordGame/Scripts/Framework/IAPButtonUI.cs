using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IAPButtonUI : MonoBehaviour
{
	#region Inspector Variables

	public Text			nameText;
	public Text			priceText;
	public GameObject	purchasedIndicator;
	public Button		button;

	#endregion

	#region Properties

	public string					ProductId			{ get; set; }
	public System.Action<string>	OnIAPButtonClicked	{ get; set; }

	#endregion

	#region Public Methods

	public void OnClicked()
	{
		if (OnIAPButtonClicked != null)
		{
			OnIAPButtonClicked(ProductId);
		}
	}

	#endregion
}
