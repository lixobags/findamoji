using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIScreenGame : UIScreen
{
	#region Inspector Variables

	[Space]

	[SerializeField] private Text 			categoryText;
	[SerializeField] private Text 			levelText;
	[SerializeField] private Image			iconImage;
	[SerializeField] private Text 			hintBtnText;
	[SerializeField] private Text 			selectedWordText;
	[SerializeField] private LetterBoard	letterBoard;
	
	#endregion

	#region Unity Methods

	private void Update()
	{
		hintBtnText.text = string.Format("HINT ({0})", GameManager.Instance.CurrentHints);
	}

	#endregion

	#region Public Methods

	public override void Initialize()
	{
		base.Initialize();

		selectedWordText.text = "";

		letterBoard.OnSelectedWordChanged += (string word) => 
		{
			selectedWordText.text = word;
		};
	}

	public override void OnShowing(object data)
	{
		base.OnShowing(data);

		CategoryInfo categoryInfo = GameManager.Instance.GetCategoryInfo(GameManager.Instance.ActiveCategory);

		categoryText.text	= categoryInfo.displayName.ToUpper();
		hintBtnText.text	= string.Format("HINT ({0})", GameManager.Instance.CurrentHints);
		iconImage.sprite	= categoryInfo.icon;

		if (GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
		{
			levelText.text = string.Format("COMPLETE TO GAIN 1 HINT");
		}
		else
		{
			levelText.text = string.Format("LEVEL {0}", GameManager.Instance.ActiveLevelIndex + 1);
		}

	}
	
	public void OnBackClicked()
	{
		if (!GameManager.Instance.AnimatingWord)
		{
			if (GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
			{
				UIScreenController.Instance.Show(UIScreenController.MainScreenId, true);
			}
			else
			{
				UIScreenController.Instance.Show(UIScreenController.CategoryLevelsScreenId, true, true, false, Tween.TweenStyle.EaseOut, null, GameManager.Instance.ActiveCategory);
			}
		}
	}

	#endregion
}
