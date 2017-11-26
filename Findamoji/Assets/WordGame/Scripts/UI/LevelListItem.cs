using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelListItem : MonoBehaviour
{
	#region Enums

	public enum Type
	{
		Normal,
		Locked,
		Completed
	}

	#endregion

	#region Inspector Variables
	
	[SerializeField] private Text		levelText;
	[SerializeField] private Image		iconImage;
	[SerializeField] private GameObject	completedImage;
	[SerializeField] private GameObject	lockedImage;
	
	#endregion
	
	#region Member Variables
	
	private string	categoryName;
	private int		levelIndex;
	private Type	type;
	
	#endregion
	
	#region Public Methods
	
	public void Setup(CategoryInfo categoryInfo, int levelIndex, Type type)
	{
		this.categoryName	= categoryInfo.name;
		this.levelIndex		= levelIndex;
		this.type			= type;

		levelText.text		= string.Format("{0} - LEVEL {1}", categoryInfo.displayName.ToUpper(), levelIndex + 1);
		iconImage.sprite	= categoryInfo.icon;

		completedImage.gameObject.SetActive(type == Type.Completed);
		lockedImage.gameObject.SetActive(type == Type.Locked);
	}
	
	public void OnClick()
	{
		if (type != Type.Locked)
		{
			// Start the level the button is tied to
			GameManager.Instance.StartLevel(categoryName, levelIndex);
			
			// Show the game screen
			UIScreenController.Instance.Show(UIScreenController.GameScreenId);
		}
	}
	
	#endregion
}
