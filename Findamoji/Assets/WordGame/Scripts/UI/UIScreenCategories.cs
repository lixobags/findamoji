using UnityEngine;
using System.Collections;

public class UIScreenCategories : UIScreen
{
	#region Inspector Variables

	[Space]

	[SerializeField] private Transform			categoriesListContainer;
	[SerializeField] private CategoryListItem	categoryListItemPrefab;

	#endregion

	#region Member Variables

	private ObjectPool categoryItemObjectPool;

	#endregion

	#region Public Methods

	public override void Initialize()
	{
		base.Initialize();

		categoryItemObjectPool = new ObjectPool(categoryListItemPrefab.gameObject, 10, categoriesListContainer);
	}

	public override void OnShowing(object data)
	{
		base.OnShowing(data);

		categoryItemObjectPool.ReturnAllObjectsToPool();

		for (int i = 0; i < GameManager.Instance.CategoryInfos.Count; i++)
		{
			CategoryInfo categoryInfo = GameManager.Instance.CategoryInfos[i];

			// If its the daily puzzle category the don't show it in the list of categories
			if (categoryInfo.name == GameManager.dailyPuzzleId)
			{
				continue;
			}

			CategoryListItem categoryListItem = categoryItemObjectPool.GetObject().GetComponent<CategoryListItem>();

			categoryListItem.Setup(categoryInfo);
			categoryListItem.gameObject.SetActive(true);
		}
	}

	public void OnBackClicked()
	{
		// Go back to main screen
		UIScreenController.Instance.Show(UIScreenController.MainScreenId, true);
	}

	#endregion
}
