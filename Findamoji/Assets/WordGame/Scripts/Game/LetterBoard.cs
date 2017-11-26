using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

public class LetterBoard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	#region Inspector Variables

	[Tooltip("The Canvas that letterTileContainer is in. This is used to get the size of the tiles in relation to the actual screen size so we can tell when the mouse is over a tile.")]
	[SerializeField] private Canvas				uiCanvas;

	[Tooltip("The GridLayoutGroup that each of the tiles will be added to.")]
	[SerializeField] private GridLayoutGroup	letterTileContainer;

	[Range(0, 1)]
	[Tooltip("This is a percentage of the tile (starting from the center) that can be touched when the mouse is dragging over the tiles to select them.")]
	[SerializeField] private float				tileTouchOffset;

	[Tooltip("The amount of spacing between the tiles. (Applied to the GridLayoutGroup spacing)")]
	[SerializeField] private float				tileSpacing;

	[Tooltip("If this is selected then a line will be draw showing the selected tiles.")]
	[SerializeField] private bool				enableLine;

	[Tooltip("The prefab to use when creating the line through selected letters, should be a circle so the corners don't look weird.")]
	[SerializeField] private Image				lineSegmentPrefab;

	[Tooltip("The prefab to use for the end of the line through selected letters.")]
	[SerializeField] private Image				lineEndPrefab;

	[Tooltip("The transform that the line segments will be added to.")]
	[SerializeField] private RectTransform		lineContainer;

	#endregion

	#region Member Variables

	public System.Action<string, List<LetterTile>, bool>	OnWordFound				= null;
	public System.Action<string>							OnSelectedWordChanged	= null;

	private List<LetterTile>	letterTiles;
	private List<LetterTile>	selectedLetterTiles;
	private ObjectPool			lineSegmentPool;
	private Image				lineEnd;
	private List<string>		currentWords;
	private int					currentBoardSize;
	private float				currentTileSize;
	private string				selectedWord = "";
	private List<GameObject>	gridGameObjects;


	#endregion

	#region Unity Methods

	public void OnBeginDrag(PointerEventData eventData)
	{
		UpdateSelectedTiles(eventData.position);
	}

	public void OnDrag(PointerEventData eventData)
	{
		UpdateSelectedTiles(eventData.position);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		TrySelectWord();
	}

	#endregion

	#region Public Methods

	public void Initialize()
	{
		letterTiles			= new List<LetterTile>();
		selectedLetterTiles	= new List<LetterTile>();
		currentWords		= new List<string>();
		gridGameObjects		= new List<GameObject>();

		lineSegmentPool = new ObjectPool(lineSegmentPrefab.gameObject, 5, lineContainer);
		lineEnd			= Instantiate(lineEndPrefab);

		lineEnd.transform.SetParent(lineContainer, false);
		lineEnd.gameObject.SetActive(false);
	}

	public void Setup(GameManager.BoardState boardState)
	{
		// Reset the game tiles so they can be selected again
		Reset();

		// Make sure the tile lists are cleared
		letterTiles.Clear();
		selectedLetterTiles.Clear();

		// Set the size of the current board
		currentBoardSize = boardState.wordBoardSize;

		// Get the maximum width and height a tile can be for this board without overflowing the container
		float maxTileWidth	= ((letterTileContainer.transform as RectTransform).rect.width - (boardState.wordBoardSize - 1) * tileSpacing) / boardState.wordBoardSize;
		float maxTileHeight	= ((letterTileContainer.transform as RectTransform).rect.height - (boardState.wordBoardSize - 1) * tileSpacing) / boardState.wordBoardSize;

		// The final tile size will be the minimum between the max width/height so that the tiles do not overflow out of the containers bounds
		currentTileSize = Mathf.Min(maxTileWidth, maxTileHeight);

		letterTileContainer.cellSize		= new Vector2(currentTileSize, currentTileSize);
		letterTileContainer.spacing			= new Vector2(tileSpacing, tileSpacing);
		letterTileContainer.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
		letterTileContainer.constraintCount	= boardState.wordBoardSize;

		// Place all the tiles on the board
		for (int i = 0; i < boardState.wordBoardSize; i++)
		{
			for (int j = 0; j < boardState.wordBoardSize; j++)
			{
				int tileIndex = i * boardState.wordBoardSize + j;

				// Create a GameObject that will go in the grid and be the parent for any LetterTile that needs to go in its place
				GameObject gridGameObject = new GameObject("grid_object", typeof(RectTransform));
				gridGameObject.transform.SetParent(letterTileContainer.transform, false);
				gridGameObjects.Add(gridGameObject);

				switch (boardState.tileStates[tileIndex])
				{
				case GameManager.BoardState.TileState.UsedButNotFound:
					LetterTile letterTile = GameManager.Instance.LetterTilePool.GetObject().GetComponent<LetterTile>();
				
					letterTile.TileIndex		= tileIndex;
					letterTile.Letter			= boardState.tileLetters[tileIndex];
					letterTile.LetterText.text	= letterTile.Letter.ToString();

					// Set it as a child of the gridGameObject we created before (so its in the correct position)
					letterTile.transform.SetParent(gridGameObject.transform, false);
					letterTile.transform.localPosition = Vector3.zero;
					letterTile.gameObject.SetActive(true);

					// Now we need to scale the LetterTile so its size is relative to currentTileSize. We can't just set the size of the RectTransform
					// because then the font on the Text component for the letter might be to big and the letter will disappear
					float scale = currentTileSize / (letterTile.transform as RectTransform).rect.width;
					letterTile.transform.localScale	= new Vector3(scale, scale, 1f);

					letterTiles.Add(letterTile);

					break;
				default:
					// We add null so when we are selecting tiles we can easily determine what tiles are beside each other by checking the indexes in boardTiles.
					letterTiles.Add(null);
					break;
				}
			}
		}

		for (int i = 0; i < boardState.words.Length; i++)
		{
			currentWords.Add(boardState.words[i]);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Reset the board
	/// </summary>
	private void Reset()
	{
		for (int i = 0; i < letterTiles.Count; i++)
		{
			if (letterTiles[i] != null)
			{
				// De-select the tile (if it was selected) and set it to not found
				letterTiles[i].SetSelected(false);
				letterTiles[i].Found = false;
				
				// De-active the GameObjects so they can be retrieved from the pool
				letterTiles[i].gameObject.SetActive(false);
				letterTiles[i].transform.SetParent(transform, false);
			}
		}

		// Destroy all the grid_objects
		for (int i = 0; i < gridGameObjects.Count; i++)
		{
			Destroy(gridGameObjects[i]);
		}

		gridGameObjects.Clear();
		currentWords.Clear();
	}

	/// <summary>
	/// Updates the list of selected letter tiles based on the position given.
	/// </summary>
	private void UpdateSelectedTiles(Vector2 position)
	{
		for (int i = 0; i < letterTiles.Count; i++)
		{
			if (letterTiles[i] == null)
			{
				continue;
			}

			Vector2	tilePosition	= letterTiles[i].transform.position;
			float	scaleTileSize	= currentTileSize * uiCanvas.scaleFactor * tileTouchOffset;

			float top		= tilePosition.y + scaleTileSize / 2f;
			float bottom	= tilePosition.y - scaleTileSize / 2f;
			float left		= tilePosition.x - scaleTileSize / 2f;
			float right		= tilePosition.x + scaleTileSize / 2f;

			// Check if the mouse if over this tile
			if (position.x > left &&
				position.x < right &&
				position.y > bottom &&
				position.y < top)
			{
				// Now check if the tile is not selected and not found and the player can select the tile
				if (!letterTiles[i].Selected && !letterTiles[i].Found && CheckCanSelectTile(i))
				{
					// Add the letter as a selected letter
					AddSelectedLetter(letterTiles[i]);
				}

				// The mouse can only be over one tile at a time so dont check any of the other tiles
				break;
			}
		}
	}

	/// <summary>
	/// Checks if the tile at tileIndex can be selected.
	/// </summary>
	private bool CheckCanSelectTile(int tileIndex)
	{
		// If there are no currently selected tiles then any tile can be selected
		if (selectedLetterTiles.Count == 0)
		{
			return true;
		}

		// Get the index of the last selected tile, only tiles adjacent to this one can be selected
		int lastSelctedIndex = selectedLetterTiles[selectedLetterTiles.Count - 1].TileIndex;

		// Get the row and column of the last selected tile
		int lastSelectedRow	= Mathf.FloorToInt((float)lastSelctedIndex / (float)currentBoardSize);
		int lastSelectedCol	= lastSelctedIndex % currentBoardSize;

		// Get the row and column of the tile being checked
		int tileIndexRow	= Mathf.FloorToInt((float)tileIndex / (float)currentBoardSize);
		int tileIndexCol	= tileIndex % currentBoardSize;

		// Check if the difference in row and column between the last selected tile and tile being checked is less than or equal to 1
		return Mathf.Abs(lastSelectedRow - tileIndexRow) <= 1 && Mathf.Abs(lastSelectedCol - tileIndexCol) <= 1;
	}

	/// <summary>
	/// Adds the letterTile as a selected letter
	/// </summary>
	private void AddSelectedLetter(LetterTile letterTile)
	{
		letterTile.SetSelected(true);
		selectedLetterTiles.Add(letterTile);
		selectedWord += letterTile.Letter;

		UpdateLine();

		// Call the event with the new selected word
		if(OnSelectedWordChanged != null)
		{
			OnSelectedWordChanged(selectedWord);
		}
	}

	/// <summary>
	/// Checks if the currently selected word is a word on the board.
	/// </summary>
	private void TrySelectWord()
	{
		// Go through each of the words on the board and check them against the selected word
		for (int i = 0; i < currentWords.Count; i++)
		{
			if (selectedWord == currentWords[i])
			{
				FoundWord(selectedWord, selectedLetterTiles);

				break;
			}
		}

		// Set all the selected BoardTiles to false
		for (int i = 0; i < selectedLetterTiles.Count; i++)
		{
			selectedLetterTiles[i].SetSelected(false);
		}

		selectedLetterTiles.Clear();
		selectedWord = "";

		UpdateLine();

		// The selected word was just changed to blank so call the event
		if(OnSelectedWordChanged != null)
		{
			OnSelectedWordChanged(selectedWord);
		}
	}

	/// <summary>
	/// Called when the player has selected a word that is in the WordBoard
	/// </summary>
	private void FoundWord(string word, List<LetterTile> letterTilesForWord)
	{
		// Set all the LetterTiles to found
		for (int i = 0; i < letterTilesForWord.Count; i++)
		{
			letterTilesForWord[i].Found = true;
		}

		if (OnWordFound != null)
		{
			OnWordFound(word, letterTilesForWord, FoundAllWords());
		}
	}

	/// <summary>
	/// Checks if all the words on the board have been found.
	/// </summary>
	private bool FoundAllWords()
	{
		for (int i = 0; i < letterTiles.Count; i++)
		{
			if (letterTiles[i] != null && !letterTiles[i].Found)
			{
				return false;
			}
		}

		return true;
	}
	/// <summary>
	/// Updates the line that runs through the selected letters.
	/// </summary>
	private void UpdateLine()
	{
		lineSegmentPool.ReturnAllObjectsToPool();
		lineEnd.gameObject.SetActive(false);

		if (!enableLine || selectedLetterTiles.Count == 0)
		{
			return;
		}

		float lineScale = 3f / (float)currentBoardSize;

		for (int i = 0; i < selectedLetterTiles.Count - 1; i++)
		{
			RectTransform	lineSegmentRectT	= lineSegmentPool.GetObject().transform as RectTransform;
			Vector2			startPosition		= (selectedLetterTiles[i].transform.parent as RectTransform).anchoredPosition;
			Vector2			endPosition			= (selectedLetterTiles[i + 1].transform.parent as RectTransform).anchoredPosition;

			// Set the scale of the line
			lineSegmentRectT.localScale = new Vector3(lineScale, lineScale, 1f);

			float angle		= Vector2.Angle(new Vector2(1f, 0f), endPosition - startPosition);
			float distance	= Vector2.Distance(startPosition, endPosition);
			float width		= distance / lineScale + lineSegmentRectT.sizeDelta.y;

			// Set position and size
			lineSegmentRectT.anchoredPosition	= startPosition + (endPosition - startPosition) / 2f;
			lineSegmentRectT.sizeDelta			= new Vector2(width, lineSegmentRectT.sizeDelta.y);

			lineSegmentRectT.gameObject.SetActive(true);

			// Set angle
			if (startPosition.y > endPosition.y)
			{
				angle = -angle;
			}

			lineSegmentRectT.eulerAngles = new Vector3(0f, 0f, angle);
		}

		// Now position the line end
		RectTransform lineEndRectT		= lineEnd.transform as RectTransform;
		lineEndRectT.anchoredPosition	= (selectedLetterTiles[selectedLetterTiles.Count - 1].transform.parent as RectTransform).anchoredPosition;
		lineEnd.gameObject.SetActive(true);
		lineEnd.transform.SetAsLastSibling();

		if (selectedLetterTiles.Count > 1)
		{
			Vector2	v1	= (selectedLetterTiles[selectedLetterTiles.Count - 1].transform.parent as RectTransform).anchoredPosition;
			Vector2	v2	= (selectedLetterTiles[selectedLetterTiles.Count - 2].transform.parent as RectTransform).anchoredPosition;
			Vector2 dir = (v1 - v2).normalized;

			float flip = (dir.x > 0) ? -1f : 1f;

			// Set the direction the line end is facing based on the direction of the last line segment
			lineEndRectT.localScale = new Vector3(flip * lineScale, lineScale, 1);

			float angle = Vector2.Angle(new Vector2(lineEndRectT.localScale.x, 0f), v2 - v1);

			if (v1.y * flip > v2.y * flip)
			{
				angle = -angle;
			}

			lineEndRectT.eulerAngles = new Vector3(0f, 0f, angle);
		}
		else
		{
			lineEndRectT.eulerAngles	= Vector3.zero;
			lineEndRectT.localScale		= new Vector3(lineScale, lineScale, 1);
		}
	}

	#endregion
}
