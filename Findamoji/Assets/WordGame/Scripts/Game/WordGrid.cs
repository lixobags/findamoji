using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WordGrid : MonoBehaviour
{
	#region Data Classes

	private class GridTile
	{
		public GameObject	gridTileObject;
		public GameObject	letterTileObject;
		public bool			displayed;
		public char			letter;
	}

	#endregion

	#region Inspector Variables

	[Tooltip("The prefab that will be instantiate and used as an empty placeholder for each letter in a word.")]
	[SerializeField] private GameObject		tilePrefab;

	[Tooltip("The container that each tile will be placed in. Should be a RectTransform with the desired width, height of 0, and placed where the center of the grid should go.")]
	[SerializeField] private RectTransform	tileContainer;

	[Tooltip("The container that will be used for animating the letter tiles from the LetterBoard to their place on the letter grid.")]
	[SerializeField] private RectTransform	animationContainer;

	[Tooltip("The size of the each of the tiles in the grid.")]
	[SerializeField] private float			tileSize;

	[Tooltip("The amount of space between each of the tiles.")]
	[SerializeField] private float			spaceBetweenLetters;

	[Tooltip("The amount of space between each group of tiles that make up a word.")]
	[SerializeField] private float			spaceBetweenWords;

	[Tooltip("The amount of space between each row of tiles.")]
	[SerializeField] private float			spaceBetweenRows;

	#endregion

	#region Member Variables

	private ObjectPool							tilePool;
	private List<string>						currentWords;
	private Dictionary<string, List<GridTile>>	allGridTiles;
	private List<GameObject>					rowObjects;

	#endregion

	#region Public Methods

	public void Initialize()
	{
		tilePool		= new ObjectPool(tilePrefab, 25, transform);
		allGridTiles	= new Dictionary<string, List<GridTile>>();
		currentWords	= new List<string>();
		rowObjects		= new List<GameObject>();

		SetupTileContainer();
	}

	public void Setup(GameManager.BoardState boardState)
	{
		Reset();

		bool		wordAddedToRow	= false;
		float		currentRowWidth	= 0f;
		GameObject	currentTileRow	= CreateNewTileRow();

		// Go through every word that is on the board, we need to add a tilePrefab for each letter in each word
		for (int i = 0; i < boardState.words.Length; i++)
		{
			// Get the word we are adding tiles for and the space those tiles will take up
			string	word		= boardState.words[i];
			float	wordWidth	= word.Length * tileSize + (word.Length - 1) * spaceBetweenLetters;

			// If a word has already been added to the current row, then we need to account for the spacing between words
			if (wordAddedToRow)
			{
				wordWidth += spaceBetweenWords;
			}

			// Check if the adding the current word to the current row will make the row larger that the width of the overall container
			bool rowToLarge = (currentRowWidth + wordWidth > tileContainer.rect.width);

			// If the current row is now wider than the container then we need to add a new row
			if (rowToLarge)
			{
				// Check if we havent added a word to the current row yet, if we havent then that means the tiles for the single word are larger than the container
				// If this happens then we will add the word anyway but the tiles will be squised and when the word is revealed the tiles will overlap.
				// To prevent this from happeneing make sure the largest word in the game using the given tileSize does not cause this error to appear. If it does
				// then reduce the tileSize of reduce the max number of letters for a word
				if (!wordAddedToRow)
				{
					Debug.LogWarningFormat("The word \"{0}\" is to large to fit in the tileContainer using a size of {1}", word, tileSize);
				}
				else
				{
					// Create a new row and set the wordAddedToRow and currentRowWidth values back to default
					currentTileRow	= CreateNewTileRow();
					wordAddedToRow	= false;
					currentRowWidth	= 0f;
				}
			}

			// If we added a word to the row already then we need to add a space GameObject
			if (wordAddedToRow)
			{
				// Create the space GameObject
				GameObject wordSpaceObject = new GameObject("word_space");

				// Add a LayoutElement to it and give it a preferred width equal to spaceBetweenWords
				LayoutElement le	= wordSpaceObject.AddComponent<LayoutElement>();
				le.preferredWidth	= spaceBetweenWords;

				// Add the space GameObject to the row
				wordSpaceObject.transform.SetParent(currentTileRow.transform, false);
			}

			// Need to create a new list of GridTiles for the tiles we are about to Instantiate
			List<GridTile> gridTiles = new List<GridTile>();

			// Add a new tile to the row for every letter in the word
			for (int j = 0; j < word.Length; j++)
			{
				// Get a new tile object, set its parent to the current row, and active it
				GameObject gridTileObject = tilePool.GetObject();
				gridTileObject.transform.SetParent(currentTileRow.transform, false);
				gridTileObject.transform.localScale = Vector3.one;
				gridTileObject.gameObject.SetActive(true);

				// If the tile didnt have a LayoutElement on it, add one and set the preferred size
				if (gridTileObject.GetComponent<LayoutElement>() == null)
				{
					AddTileLayoutElement(gridTileObject);
				}

				// Create a new GridTile and set the references
				GridTile gridTile		= new GridTile();
				gridTile.gridTileObject = gridTileObject;
				gridTile.letter			= word[j];

				gridTiles.Add(gridTile);
			}

			// Add the list of GridTiles to the dictionary indexed by the word they are for
			allGridTiles.Add(word, gridTiles);
			currentWords.Add(word);

			wordAddedToRow	= true;
			currentRowWidth += wordWidth;
		}

		// Display all the found words
		for (int i = 0; i < boardState.words.Length; i++)
		{
			if (boardState.foundWords[i])
			{
				// Get the word we need to display then the list of GridTiles for that word
				string			word		= boardState.words[i];
				List<GridTile>	gridTiles	= allGridTiles[word];
				
				// Loop through each grid tile and display the letter for it
				for (int j = 0; j < gridTiles.Count; j++)
				{
					DisplayLetter(gridTiles[j]);
				}
			}
		}
		
		// Display all the letters that have been show as a hint
		for (int i = 0; i < boardState.hintLettersShown.Count; i++)
		{
			int[]			indexes		= boardState.hintLettersShown[i];
			string			word		= currentWords[indexes[0]];
			List<GridTile>	gridTiles	= allGridTiles[word];
			
			DisplayLetter(gridTiles[indexes[1]]);
		}
	}

	/// <summary>
	/// Called when the player has selected a word. The letterTiles list will contain all the GameTiles on the board that
	/// make up the word. This is where the animation of the tiles from the board to the grid happens.
	/// </summary>
	public void FoundWord(string word, List<LetterTile> letterTiles, Tween.OnTweenFinished onTweenFinished)
	{
		if (!allGridTiles.ContainsKey(word))
		{
			Debug.LogErrorFormat("There is no word \"{0}\" on the WordGrid. Hidding the GameTiles.", word);

			// Just hide all the GameTiles and their letters
			for (int i = 0; i < letterTiles.Count; i++)
			{
				letterTiles[i].gameObject.SetActive(false);
			}

			return;
		}

		// Get the grid tiles for the word
		List<GridTile> gridTiles = allGridTiles[word];

		// Loop through each of the LetterTiles and animate them to the location of the GridTile
		for (int i = 0; i < letterTiles.Count; i++)
		{
			gridTiles[i].displayed = true;

			RectTransform letterTileRectT	= letterTiles[i].transform as RectTransform;
			RectTransform gridTileRectT		= gridTiles[i].gridTileObject.transform as RectTransform;

			// This fixes an issue where if there was a saved hint letter shown, then when the word for the hint was found the letters would animate to the
			// corner of the screen. This is because when the game started up the letterTile was automatically placed in the spot of the word grid tile and
			// the grid tile was not positioned properly buy the layout components.
			if (gridTiles[i].letterTileObject != null)
			{
				gridTileRectT = gridTiles[i].letterTileObject.transform as RectTransform;
			}

			TransitionAnimateOver(letterTileRectT, gridTileRectT, (i == 0) ? onTweenFinished : null);
		}
	}

	/// <summary>
	/// Displays the next letter in an unfound word, returns the word and letter index
	/// </summary>
	public bool DisplayNextHint(ref int nextHintIndex, out int wordIndex, out int letterIndex)
	{
		for (int i = 0; i < currentWords.Count; i++)
		{
			int		index	= (nextHintIndex + i) % currentWords.Count;
			string	word	= currentWords[index];

			List<GridTile> gridTiles = allGridTiles[word];

			for (int j = 0; j < gridTiles.Count; j++)
			{
				if (!gridTiles[j].displayed)
				{
					DisplayLetter(gridTiles[j]);

					nextHintIndex	= (index + 1) % currentWords.Count;
					wordIndex		= index;
					letterIndex		= j;

					return true;
				}
			}
		}

		wordIndex	= -1;
		letterIndex	= -1;

		// All letters are showing therefore we did not display a hint
		return false;
	}

	/// <summary>
	/// Reset the board by removing all the GridTiles
	/// </summary>
	public void Reset()
	{
		foreach (KeyValuePair<string, List<GridTile>> pair in allGridTiles)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				// De-activate the grid tile so it can be fetched from the pool again and set its parent to this WordGrid so it isn't
				// destroyed when we destroy the row Obejcts
				pair.Value[i].gridTileObject.gameObject.SetActive(false);
				pair.Value[i].gridTileObject.transform.SetParent(transform);

				if (pair.Value[i].letterTileObject != null)
				{
					// Set the LetterTile to de-active so the object pooler can use it again.
					pair.Value[i].letterTileObject.SetActive(false);

					// Right now the LetterTile is a child of a row GameObject, we destroy the row Objects below. But we don't want
					// to destroy the LetterTiles (we want to be able to re-use them) so lets just set the LetterTiles parent to be
					// this WordGrid so we don;t lose them
					pair.Value[i].letterTileObject.transform.SetParent(transform);
				}
			}
		}

		// Destroy all the rows
		for (int i = 0; i < rowObjects.Count; i++)
		{
			Destroy(rowObjects[i]);
		}

		allGridTiles.Clear();
		currentWords.Clear();
	}

	#endregion

	#region Private Methods

	private void DisplayLetter(GridTile gridTile)
	{
		// If there is already a letter being displayed on this grid tile then don't instantiate a new one
		if (gridTile.displayed)
		{
			return;
		}

		// Get an instance of LetterTile, set the letter, then active it
		LetterTile letterTile		= GameManager.Instance.LetterTilePool.GetObject().GetComponent<LetterTile>();
		letterTile.LetterText.text	= gridTile.letter.ToString();
		letterTile.gameObject.SetActive(true);

		// Set a reference to them so when we reset the board we can remove them
		gridTile.letterTileObject	= letterTile.gameObject;
		gridTile.displayed			= true;

		// Create a container GameObject that will be the parent of the LetterTile, this way we can scale down the LetterTile and have the font scale down and still be visible
		GameObject letterTileContainer = new GameObject("letter_tile_container");

		// Add a tile LayoutElement to the container so its sized properly in the layout group
		AddTileLayoutElement(letterTileContainer);

		// Set the letter tile containers parent to the parent of the grid tile then set the letter tiles sibling index to that of the grid tile, then de-activate the grid tile.
		// This will essently replaces the grid tile with the scaled down letter tile
		letterTileContainer.transform.SetParent(gridTile.gridTileObject.transform.parent, false);
		letterTileContainer.transform.SetSiblingIndex(gridTile.gridTileObject.transform.GetSiblingIndex());
		gridTile.gridTileObject.gameObject.SetActive(false);
		
		// Get the scale of the letter tile so that it will be scaled down to the size of a grid tile
		float scale = tileSize / (letterTile.transform as RectTransform).rect.width;

		// Set the scale of the LetterTIle, set its localPosition to zero, then set its parent to be the letter_tile_container we created befor
		letterTile.transform.localScale		= new Vector3(scale, scale, 1f);
		letterTile.transform.localPosition	= Vector3.zero;
		letterTile.transform.SetParent(letterTileContainer.transform, false);
	}

	private void TransitionAnimateOver(RectTransform letterTileRectT, RectTransform wordTileRectT, Tween.OnTweenFinished onTweenFinished)
	{
		float duration = 400f;
		
		letterTileRectT.SetParent(animationContainer);

		float xScale = tileSize / letterTileRectT.rect.width;
		float yScale = tileSize / letterTileRectT.rect.height;

		Tween.ScaleX(letterTileRectT, Tween.TweenStyle.EaseOut, letterTileRectT.localScale.x, xScale, duration);
		Tween.ScaleY(letterTileRectT, Tween.TweenStyle.EaseOut, letterTileRectT.localScale.y, yScale, duration);
		Tween.PositionX(letterTileRectT, Tween.TweenStyle.EaseOut, letterTileRectT.position.x, wordTileRectT.position.x, duration);
		Tween.PositionY(letterTileRectT, Tween.TweenStyle.EaseOut, letterTileRectT.position.y, wordTileRectT.position.y, duration).SetFinishCallback(onTweenFinished);
	}

	/// <summary>
	/// Adds the necessary layout components to the tileContainer
	/// </summary>
	private void SetupTileContainer()
	{
		VerticalLayoutGroup vlg		= tileContainer.gameObject.AddComponent<VerticalLayoutGroup>();
		vlg.spacing					= spaceBetweenRows;
		vlg.childForceExpandWidth	= true;
		vlg.childForceExpandHeight	= false;

		ContentSizeFitter csf	= tileContainer.gameObject.AddComponent<ContentSizeFitter>();
		csf.horizontalFit		= ContentSizeFitter.FitMode.Unconstrained;
		csf.verticalFit			= ContentSizeFitter.FitMode.PreferredSize;
	}

	/// <summary>
	/// Adds a LayoutElement to the given GameObject and sets the preferred width/height to tileSize
	/// </summary>
	private void AddTileLayoutElement(GameObject obj)
	{
		LayoutElement le	= obj.AddComponent<LayoutElement>();
		le.preferredWidth	= tileSize;
		le.preferredHeight	= tileSize;
	}

	/// <summary>
	/// Adds the necessary layout components to the tileRow GameObject
	/// </summary>
	private GameObject CreateNewTileRow()
	{
		// Create the row and add it to tileContainer
		GameObject tileRow = new GameObject("tile_row", typeof(RectTransform));
		tileRow.transform.SetParent(tileContainer, false);

		// Add a HorizontalLayoutGroup to the row
		HorizontalLayoutGroup hlg	= tileRow.AddComponent<HorizontalLayoutGroup>();
		hlg.childAlignment			= TextAnchor.MiddleCenter;
		hlg.spacing					= spaceBetweenLetters;
		hlg.childForceExpandWidth	= false;
		hlg.childForceExpandHeight	= false;

		rowObjects.Add(tileRow);

		return tileRow;
	}

	#endregion
}
