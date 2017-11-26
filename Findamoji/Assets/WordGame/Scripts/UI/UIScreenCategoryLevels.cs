using UnityEngine;
using System.Collections;

public class UIScreenCategoryLevels : UIScreen
{
	#region Inspector Variables

	[Space]

	[SerializeField] private Transform		levelListContainer;
	[SerializeField] private LevelListItem	levelListItemPrefab;
	
	#endregion
	
	#region Member Variables
	
	private ObjectPool levelItemObjectPool;
	
	#endregion
	
	#region Public Methods
	
	public override void Initialize()
	{
		base.Initialize();

		levelItemObjectPool = new ObjectPool(levelListItemPrefab.gameObject, 10, levelListContainer);
	}
	
	public override void OnShowing(object data)
	{
		base.OnShowing(data);

		levelItemObjectPool.ReturnAllObjectsToPool();

		CategoryInfo	categoryInfo	= GameManager.Instance.GetCategoryInfo((string)data);
		bool			completed		= true;

		for (int i = 0; i < categoryInfo.levelInfos.Count; i++)
		{
			LevelListItem.Type type = completed ? LevelListItem.Type.Completed : LevelListItem.Type.Locked;

			if (completed && !GameManager.Instance.IsLevelCompleted(categoryInfo, i))
			{
				completed	= false;
				type		= LevelListItem.Type.Normal;
			}

			LevelListItem levelListItem = levelItemObjectPool.GetObject().GetComponent<LevelListItem>();
			
			levelListItem.Setup(categoryInfo, i, type);
			levelListItem.gameObject.SetActive(true);
		}
	}
	
	public void OnBackClicked()
	{
		// Go back to main screen
		UIScreenController.Instance.Show(UIScreenController.CategoriesScreenId, true);
	}
	
	#endregion
}
