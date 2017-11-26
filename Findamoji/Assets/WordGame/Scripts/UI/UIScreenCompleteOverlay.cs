using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIScreenCompleteOverlay : UIScreen
{
	#region Inspector Variables

	[Space]

	[SerializeField] private Image		categoryIconImage;
	[SerializeField] private Text		categoryNameText;
	[SerializeField] private Text		categoryLevelText;
	[SerializeField] private GameObject	plusOneHintText;

	#endregion

	#region Public Methods

	public override void OnShowing(object data)
	{
		base.OnShowing(data);

		CategoryInfo categoryInfo = GameManager.Instance.GetCategoryInfo(GameManager.Instance.ActiveCategory);

		categoryIconImage.sprite	= categoryInfo.icon;
		categoryNameText.text		= categoryInfo.displayName;

		if (GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
		{
			categoryLevelText.gameObject.SetActive(false);
		}
		else
		{
			categoryLevelText.gameObject.SetActive(true);
			categoryLevelText.text = "Level " + (GameManager.Instance.ActiveLevelIndex + 1).ToString();
		}

		plusOneHintText.SetActive((bool)data);
	}

	#endregion
}
