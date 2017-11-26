using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utilities
{
	#region Member Variables

	public const string BoardFilesDirectory = "WordGameBoardFiles"; // This should be a non-empty string with no leading or trailing slashes (ie. /)

	#endregion

	#region Properties

	public static double SystemTimeInMilliseconds { get { return (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalMilliseconds; } }

	public static float WorldWidth	{ get { return 2f * Camera.main.orthographicSize * Camera.main.aspect; } }
	public static float WorldHeight	{ get { return 2f * Camera.main.orthographicSize; } }

	#endregion

	#region Public Methods

	public static void SaveWordBoard(WordBoard wordBoard, string directoryInResources)
	{
		#if !UNITY_EDITOR
		Debug.LogError("Can only save WordBoards in the Unity Editor.");
		#else
		/* Important things we need to save in order to create a game board: id, size, words, and wordTiles */

		// Get the words as one long string seperated by _ to save space
		string wordsStr = "";
		for (int i = 0; i < wordBoard.words.Length; i++)
		{
			if (i != 0)
			{
				wordsStr += "_";
			}

			wordsStr += wordBoard.words[i];
		}

		// Get the word tile states as one long string to save space
		string wordTilesStr = "";
		for (int i = 0; i < wordBoard.wordTiles.Length; i++)
		{
			string usedStr		= wordBoard.wordTiles[i].used ? "1" : "0";
			string hasLetterStr	= wordBoard.wordTiles[i].hasLetter ? "1" : "0";

			wordTilesStr += string.Format("{0}{1}{2}", usedStr, hasLetterStr, (hasLetterStr == "1") ? wordBoard.wordTiles[i].letter : '-');
		}

		// Create the text that defines a WordBoard
		string text = "";
		text += string.Format("{0},", wordBoard.id);
		text += string.Format("{0},", wordBoard.size);
		text += string.Format("{0},", wordsStr);
		text += string.Format("{0}", wordTilesStr);

		// Get the full path
		string directoyPath	= Application.dataPath + "/WordGame/Resources/" + BoardFilesDirectory;
		string ioPath		= directoyPath + "/" + wordBoard.id + ".csv";

		if (!System.IO.Directory.Exists(directoyPath))
		{
			System.IO.Directory.CreateDirectory(directoyPath);
		}

		// If there is already a board, then delete it to create a new one
		if (System.IO.File.Exists(ioPath))
		{
			System.IO.File.Delete(ioPath);
		}

		// Open a StreamWriter and write the text to the file
		System.IO.StreamWriter stream = System.IO.File.CreateText(ioPath);
		stream.WriteLine(text);
		stream.Close();
		#endif
	}

	public static WordBoard LoadWordBoard(string boardId)
	{
		TextAsset textAsset = Resources.Load<TextAsset>(Utilities.BoardFilesDirectory + "/" + boardId);

		if (textAsset != null)
		{
			string[] text = textAsset.text.Split(',');

			WordBoard wordBoard = new WordBoard();
			wordBoard.id		= text[0];
			wordBoard.size		= System.Convert.ToInt32(text[1]);
			wordBoard.words		= text[2].Split('_');

			wordBoard.wordTiles = new WordBoard.WordTile[text[3].Length / 3];

			for (int i = 0; i < wordBoard.wordTiles.Length; i++)
			{
				WordBoard.WordTile wordTile = new WordBoard.WordTile();

				wordTile.used		= text[3][0] == '1';
				wordTile.hasLetter	= text[3][1] == '1';
				wordTile.letter		= text[3][2];

				wordBoard.wordTiles[i] = wordTile;

				// Remove the first 3 characters of text[3]
				text[3] = text[3].Substring(3, text[3].Length - 3);
			}

			return wordBoard;
		}

		return null;
	}

	/// <summary>
	/// Creates a board id using a category and level name
	/// </summary>
	public static string FormatBoardId(string category, int index)
	{
		return string.Format("{0}_{1}", category, index).Replace(" ", "_");
	}

	/// <summary>
	/// Returns to mouse position
	/// </summary>
	public static Vector2 MousePosition()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		return (Vector2)Input.mousePosition;
		#else
		if (Input.touchCount > 0)
		{
			return Input.touches[0].position;
		}

		return Vector2.zero;
		#endif
	}

	/// <summary>
	/// Returns true if a mouse down event happened, false otherwise
	/// </summary>
	public static bool MouseDown()
	{
		return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began);
	}
	
	/// <summary>
	/// Returns true if a mouse up event happened, false otherwise
	/// </summary>
	public static bool MouseUp()
	{
		return (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended));
	}
	
	/// <summary>
	/// Returns true if no mouse events are happening, false otherwise
	/// </summary>
	public static bool MouseNone()
	{
		return (!Input.GetMouseButton(0) && Input.touchCount == 0);
	}

	/// <summary>
	/// Converts to json string.
	/// </summary>
	public static string ConvertToJsonString(object data)
	{
		string jsonString = "";
		
		if (data is IDictionary)
		{
			Dictionary<string, object> dic = data as Dictionary<string, object>;
			
			jsonString += "{";
			
			List<string> keys = new List<string>(dic.Keys);
			
			for (int i = 0; i < keys.Count; i++)
			{
				if (i != 0)
				{
					jsonString += ",";
				}
				
				jsonString += string.Format("\"{0}\":{1}", keys[i], ConvertToJsonString(dic[keys[i]]));
			}
			
			jsonString += "}";
		}
		else if (data is IList)
		{
			IList list = data as IList;
			
			jsonString += "[";
			
			for (int i = 0; i < list.Count; i++)
			{
				if (i != 0)
				{
					jsonString += ",";
				}
				
				jsonString += ConvertToJsonString(list[i]);
			}
			
			jsonString += "]";
		}
		else if (data is string)
		{
			// If the data is a string then we need to inclose it in quotation marks
			jsonString += "\"" + data + "\"";
		}
		else
		{
			// Else just return what ever data is as a string
			jsonString += data.ToString();
		}
		
		return jsonString;
	}

	#endregion
}
