using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIScreenMain : UIScreen
{
	#region Inspector Variables

	[Space]

	[SerializeField] private ProgressRing	progressRing;
	[SerializeField] private Text			continueBtnTopText;
	[SerializeField] private Text			continueBtnBottomText;
	[SerializeField] private Image			continueBtnImage;

	#endregion

	#region Member Variables

	private string	continueBtnCategory;
	private int		continueBtnLevelIndex;

	#endregion

	#region Public Methods

	public override void OnShowing(object data)
	{
		base.OnShowing(data);

		// Set the progress rings percentage to the number of completed levels from all categories
		int totalNumberOfLevels				= 0;
		int totalNumberOfCompletedLevels	= 0;

		for (int i = 0; i < GameManager.Instance.CategoryInfos.Count; i++)
		{
			CategoryInfo categoryInfo = GameManager.Instance.CategoryInfos[i];

			// Only include levels that are not part of the paily puzzle category
			if (categoryInfo.name != GameManager.dailyPuzzleId)
			{
				totalNumberOfLevels				+= categoryInfo.levelInfos.Count;
				totalNumberOfCompletedLevels	+= GameManager.Instance.GetCompletedLevelCount(categoryInfo);
			}
		}

		progressRing.SetProgress((float)totalNumberOfCompletedLevels / (float)totalNumberOfLevels);

		// Set the Continue button to the active category
		if (string.IsNullOrEmpty(GameManager.Instance.ActiveCategory) || GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
		{
			bool foundUncompletedLevel = false;

			for (int i = 0; i < GameManager.Instance.CategoryInfos.Count; i++)
			{
				CategoryInfo categoryInfo = GameManager.Instance.CategoryInfos[i];

				if (categoryInfo.name == GameManager.dailyPuzzleId)
				{
					continue;
				}

				for (int j = 0; j < categoryInfo.levelInfos.Count; j++)
				{
					if (!GameManager.Instance.IsLevelCompleted(categoryInfo, j))
					{
						continueBtnCategory		= categoryInfo.name;
						continueBtnLevelIndex	= j;
						foundUncompletedLevel	= true;

						break;
					}
				}

				if (foundUncompletedLevel)
				{
					break;
				}
			}

			// If all levels are completed then set the button to the first category and first level
			if (!foundUncompletedLevel)
			{
				continueBtnCategory		= GameManager.Instance.CategoryInfos[0].name;
				continueBtnLevelIndex	= 0;
			}
			
			continueBtnTopText.text	= "PLAY";
		}
		else
		{
			continueBtnCategory		= GameManager.Instance.ActiveCategory;
			continueBtnLevelIndex	= GameManager.Instance.ActiveLevelIndex;

			continueBtnTopText.text = "CONTINUE";
		}

		CategoryInfo contineCategoryInfo = GameManager.Instance.GetCategoryInfo(continueBtnCategory);

		continueBtnBottomText.text	= string.Format("{0} LEVEL {1}", contineCategoryInfo.displayName.ToUpper(), continueBtnLevelIndex + 1);
		continueBtnImage.sprite		= contineCategoryInfo.icon;
	}

	public void OnCategoryButtonClicked()
	{
		// Show the main screen
		UIScreenController.Instance.Show(UIScreenController.CategoriesScreenId);
	}

	public void OnContinueButtonClicked()
	{
		// Start the level the button is tied to
		GameManager.Instance.StartLevel(continueBtnCategory, continueBtnLevelIndex);

		// Show the game screen
		UIScreenController.Instance.Show(UIScreenController.GameScreenId);
	}

	#endregion
}
