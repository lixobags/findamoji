using UnityEngine;
using UnityEditor;
using System.Collections;

public class BoardFileCreatorWindow : EditorWindow
{
	#region Member Variables

	private GameManager	gameManagerReference;
	private bool		recreateAllBoards;

	private bool				isCreatingBoards;
	private WordBoardCreator	wordBoardCreator;
	private int					totalFilesToCreate;
	private int					totalFilesCreated;
	private int					categoryIndex;
	private int					categoryLevelIndex;

	#endregion

	#region Unity Methods

	[MenuItem ("Window/Board File Creator")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow<BoardFileCreatorWindow>("Board File Creator").Show();
	}
	
	private void OnDestroy()
	{
		// If we were creating boards then destroy the WordBoardCreator
		if (isCreatingBoards)
		{
			StopCreatingBoards();
			Debug.LogWarning("Board creation was stopped because the window was closed.");
		}
	}

	private void Update()
	{
		// Call the update loop on the WordBoardCreator
		if (isCreatingBoards && wordBoardCreator != null)
		{
			string	title		= string.Format("Creating Board File {0}/{1}", totalFilesCreated, totalFilesToCreate);
			string	info		= string.Format("Category: {0}, Level: {1}", gameManagerReference.CategoryInfos[categoryIndex].name, categoryLevelIndex + 1);
			float	progress	= (float)totalFilesCreated / (float)totalFilesToCreate;

			bool cancelled = EditorUtility.DisplayCancelableProgressBar(title, info, progress);

			if (cancelled)
			{
				wordBoardCreator.AbortAll();
				StopCreatingBoards();

				Debug.Log("Board creation was stopped by the user");
			}

			wordBoardCreator.Update();
		}
	}

	private void OnGUI()
	{
		GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
		labelStyle.fontSize = 18;
		labelStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Board File Creator", labelStyle, GUILayout.Height(22));
		EditorGUILayout.HelpBox("The Board File Creator is a tool that is used to create board files which are used at run time in the application to display boards to the player. Creating boards could take a couple seconds (or minutes depending on how big the boards are).", MessageType.Info);

		EditorGUILayout.Space();

		// If boards are currently being created disable the buttons
		if (isCreatingBoards)
		{
			GUI.enabled = false;
		}

		// Try and find a reference of the GameManager in the current scene
		if (gameManagerReference == null)
		{
			gameManagerReference = GameObject.FindObjectOfType<GameManager>();
		}

		gameManagerReference = EditorGUILayout.ObjectField("Game Manager:", gameManagerReference, typeof(GameManager), true) as GameManager;

		if (gameManagerReference == null)
		{
			EditorGUILayout.HelpBox("Could not find a GameManager component in the current scene, please open a scene with a GameManager in it or assign the Game Manager field above manually.", MessageType.Error);
			GUI.enabled = false;
		}

		recreateAllBoards = EditorGUILayout.Toggle(new GUIContent("Re-Create All", "Check this if you would like to delete all board files and re-create them."), recreateAllBoards);
	
		EditorGUILayout.Space();

		if (GUILayout.Button("Create Board Files"))
		{
			StartCreatingBoardFiles();
		}

		GUI.enabled = true;
	}

	#endregion

	#region Private Methods

	private void StartCreatingBoardFiles()
	{
		// Make sure all values are set back to default
		StopCreatingBoards();

		isCreatingBoards = true;

		// Create a new GameObject in the current scene and attach a WordBoardCreator script to it
		wordBoardCreator = new GameObject("WordBoardCreator").AddComponent<WordBoardCreator>();

		// Get the total number of files
		for (int i = 0; i < gameManagerReference.CategoryInfos.Count; i++)
		{
			totalFilesToCreate += gameManagerReference.CategoryInfos[i].levelInfos.Count;
		}

		CreateBoardFile();
	}

	private void CreateBoardFile()
	{
		// Get the board id for the board we want to generate
		CategoryInfo	currentCategoryInfo	= gameManagerReference.CategoryInfos[categoryIndex];
		string			boardId				= Utilities.FormatBoardId(currentCategoryInfo.name, categoryLevelIndex);

		// If we are re-creating all boards or the board does not exist then run the algo
		if ((recreateAllBoards || Resources.Load<TextAsset>(Utilities.BoardFilesDirectory + "/" + boardId) == null) && categoryLevelIndex < currentCategoryInfo.levelInfos.Count)
		{
			wordBoardCreator.StartCreatingBoard(boardId, currentCategoryInfo.levelInfos[categoryLevelIndex].words, OnWordBoardFinished, 5000L);
		}
		else
		{
			// Just call this to move to the next word board
			OnWordBoardFinished(null);
		}
	}

	private void OnWordBoardFinished(WordBoard wordBoard)
	{
		if (!isCreatingBoards)
		{
			return;
		}

		totalFilesCreated++;

		if (wordBoard != null)
		{
			Utilities.SaveWordBoard(wordBoard, Utilities.BoardFilesDirectory);
			AssetDatabase.Refresh();
		}
		
		categoryLevelIndex++;
		
		if (categoryLevelIndex >= gameManagerReference.CategoryInfos[categoryIndex].levelInfos.Count)
		{
			categoryIndex++;
			categoryLevelIndex = 0;
			
			if (categoryIndex >= gameManagerReference.CategoryInfos.Count)
			{
				StopCreatingBoards();

				Debug.Log("Successfully created all boards");

				return;
			}
		}
		
		CreateBoardFile();
	}

	private void StopCreatingBoards()
	{
		if (wordBoardCreator != null)
		{
			DestroyImmediate(wordBoardCreator.gameObject);
		}
		
		isCreatingBoards	= false;
		wordBoardCreator	= null;
		totalFilesToCreate	= 0;
		totalFilesCreated	= 0;
		categoryIndex		= 0;
		categoryLevelIndex	= 0;

		// Null out the DailyPuzzleInfo since its being saved in editor
		gameManagerReference.CategoryInfos.Remove(gameManagerReference.DailyPuzzleInfo);
		gameManagerReference.DailyPuzzleInfo = null;

		EditorUtility.ClearProgressBar();
	}

	#endregion
}
