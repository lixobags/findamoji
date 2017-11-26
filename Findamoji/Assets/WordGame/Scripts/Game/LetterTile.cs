using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LetterTile : MonoBehaviour
{
	#region Inspector Variables

	[SerializeField] private Image 	backgroundImage;
	[SerializeField] private Text 	letterText;
	[SerializeField] private Color 	backgroundNormalColor;
	[SerializeField] private Color 	backgroundSelectedColor;
	[SerializeField] private Color 	letterNormalColor;
	[SerializeField] private Color 	letterSelectedColor;
	[SerializeField] private Sprite normalSprite;
	[SerializeField] private Sprite selectedSprite;

	#endregion

	#region Properties

	public Text 			LetterText		{ get { return letterText; } }
	public int				TileIndex		{ get; set; }
	public bool				Selected		{ get; set; }
	public bool				Found			{ get; set; }
	public char				Letter			{ get; set; }

	#endregion

	#region Public Methods

	public void SetSelected(bool selected)
	{
		Selected = selected;

		backgroundImage.sprite 	= selected ? selectedSprite : normalSprite;
		backgroundImage.color	= selected ? backgroundSelectedColor : backgroundNormalColor;
		letterText.color		= selected ? letterSelectedColor : letterNormalColor;
	}

	#endregion
}
