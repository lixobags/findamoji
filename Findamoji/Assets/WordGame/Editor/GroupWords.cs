using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GroupWords : EditorWindow
{
	private static int boardSize = 3;
	private static List<string> words = new List<string>();

	private static Vector2 scrollPos;

	[MenuItem ("Window/Group Words Tool")]
	public static void Init()
	{
		EditorWindow.GetWindow(typeof(GroupWords));
	}

	private void OnGUI() 
	{
		EditorGUILayout.Space();

		EditorGUILayout.HelpBox("This is a tool I used to find word groupings for game levels. I'm leaving it in the project since it might be helpful to you but since I never " +
			"planed on having as part of the completed asset it is not that pretty. How it works is you give it the size of the board you want (say 3x3 board) then you add " +
			"a bunch of words. When you click Find Groupings it will go through the words and match any that fit the board. It does not try and find the optimal " +
			"grouping it just finds a random one so running Find Groupings will yeild different results each time.", MessageType.Info);

		boardSize = EditorGUILayout.IntField("Board Size:", boardSize);

		EditorGUILayout.LabelField("Words:");

		EditorGUI.indentLevel++;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		for (int i = words.Count - 1; i >= 0; i--)
		{
			EditorGUILayout.BeginHorizontal();

			words[i] = EditorGUILayout.TextField(words[i]);
			
			// Draw the remove button so you can remove elements in the middle of the list
			if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
			{
				words.RemoveAt(i);
			}
			
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Add Word", GUILayout.Height(40)))
		{
			words.Add("");
		}

		EditorGUI.indentLevel--;

		EditorGUILayout.Space();

		if (GUILayout.Button("Find Groupings", GUILayout.Height(40)))
		{
			FindGroupings();
		}

		EditorGUILayout.Space();
	}

	private void FindGroupings()
	{
		// Get the amount of letters
		int letterCount = boardSize * boardSize;

		// Create a copy of words
		List<string> tempWords = new List<string>(words);

		// Scramble the words so we get can get a new grouping each time
		Scramble(tempWords);

		List<List<string>> groupings = new List<List<string>>();

		while (tempWords.Count > 0)
		{
			List<string> grouping = FindGrouping(letterCount, 0, tempWords);

			if (grouping == null)
			{
				break;
			}

			for (int i = 0; i < grouping.Count; i++)
			{
				tempWords.Remove(grouping[i]);
			}

			groupings.Add(grouping);
		}

		Debug.Log("Here are all the groupings I came up with:");

		for (int i = 0; i < groupings.Count; i++)
		{
			string groupingStr = "";

			for (int j = 0; j < groupings[i].Count; j++)
			{
				if (j > 0)
				{
					groupingStr += " ";
				}

				groupingStr += groupings[i][j];
			}

			Debug.Log(groupingStr);
		}

		Debug.Log("=========================================");

		Debug.Log("Here are all the words that I didn't find a grouping for:");
		
		for (int i = 0; i < tempWords.Count; i++)
		{
			Debug.Log(tempWords[i]);
		}
	}

	private List<string> FindGrouping(int length, int startIndex, List<string> wordsToGroup)
	{
		for (int i = startIndex; i < wordsToGroup.Count; i++)
		{
			if (wordsToGroup[i].Length == length)
			{
				return new List<string>() { wordsToGroup[i] }; 
			}
			else if (wordsToGroup[i].Length < length)
			{
				List<string> grouping = FindGrouping(length - wordsToGroup[i].Length, i + 1, wordsToGroup);

				if (grouping != null)
				{
					grouping.Add(wordsToGroup[i]);

					return grouping;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Scramble the specified list of words.
	/// </summary>
	private void Scramble(List<string> words)
	{
		// Run it 10 times to get good scrambleness
		for (int j = 0; j < 10; j++)
		{
			// Quick and dirty, just go though the list of words and swap each index with another random index
			for (int i = 0; i < words.Count; i++)
			{
				int		swapIndex	= Random.Range(0, words.Count);
				string	temp		= words[i];

				words[i]			= words[swapIndex];
				words[swapIndex]	= temp;
			}
		}
	}
}






