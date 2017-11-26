using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
	#region Methods

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("startingHints"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("adLevelCompleteAmount"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("letterBoard"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("wordGrid"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("letterTilePrefab"));

		DrawCategoryInfos();
		DrawDailyPuzzleLevels();

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("iapHintInfos"), true);

		EditorGUILayout.Space();

		if (GUILayout.Button("Delete Saved Data"))
		{
			System.IO.File.Delete(GameManager.SaveDataPath);
			PlayerPrefs.DeleteAll();
		}
		
		if (GUILayout.Button("Open Board File Creator Window"))
		{
			BoardFileCreatorWindow.ShowWindow();
		}

		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawCategoryInfos()
	{
		SerializedProperty categoryInfos = serializedObject.FindProperty("categoryInfos");

		// Make sure all the displayName fields are not blank. This is mainly so when a customer updates the asset it will automatically set the display names for them
		for (int i = 0; i < categoryInfos.arraySize; i++)
		{
			SerializedProperty categoryInfo = categoryInfos.GetArrayElementAtIndex(i);
			SerializedProperty id			= categoryInfo.FindPropertyRelative("name");
			SerializedProperty displayName	= categoryInfo.FindPropertyRelative("displayName");

			// If the displayName is not set but the name is then set the displayName to the name
			if (string.IsNullOrEmpty(displayName.stringValue) && !string.IsNullOrEmpty(id.stringValue))
			{
				displayName.stringValue = id.stringValue;
			}
		}

		// Draw the categoryInfos but not its children, this will just draw the foldout (little arrow thingy)
		EditorGUILayout.PropertyField(categoryInfos, false);
		
		// If categoryInfos is expanded then draw its children
		if (categoryInfos.isExpanded)
		{
			EditorGUI.indentLevel++;
			
			// Draw the "Size" property for the array
			EditorGUILayout.PropertyField(categoryInfos.FindPropertyRelative("Array.size"));
			
			// Draw each of the CategoryInfos in the categoryInfos list
			for (int i = 0; i < categoryInfos.arraySize; i++)
			{
				SerializedProperty categoryInfo = categoryInfos.GetArrayElementAtIndex(i);
				
				EditorGUILayout.BeginHorizontal();
				
				// This will just draw the "Category Infos" foldout and the tooltip for categoryInfos
				EditorGUILayout.PropertyField(categoryInfo, false);
				
				bool deleted = false;
				
				// Draw the remove button so you can remove elements in the middle of the list
				if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
				{
					categoryInfos.DeleteArrayElementAtIndex(i);
					deleted = true;
				}
				
				EditorGUILayout.EndHorizontal();
				
				// Now if it was not deleted and is expanded then draw its properties
				if (!deleted && categoryInfo.isExpanded)
				{
					EditorGUI.indentLevel++;
					
					/* If you want to add properties to CategoryInfo then add them here. */

					// Check the category id for specail characters, if there are then remove them
					SerializedProperty idProperty = categoryInfo.FindPropertyRelative("name");

					for (int j = idProperty.stringValue.Length - 1; j >= 0; j--)
					{
						char c = idProperty.stringValue[j];

						if ((c < '0' || c > '9') &&
							(c < 'A' || c > 'Z') &&
							(c < 'a' || c > 'z'))
						{
							idProperty.stringValue = idProperty.stringValue.Remove(j, 1);
						}
					}

					EditorGUILayout.PropertyField(idProperty);
					EditorGUILayout.PropertyField(categoryInfo.FindPropertyRelative("displayName"));
					EditorGUILayout.PropertyField(categoryInfo.FindPropertyRelative("description"));
					EditorGUILayout.PropertyField(categoryInfo.FindPropertyRelative("icon"));
					
					DrawCategoryLevelInfos(categoryInfo);
					
					EditorGUI.indentLevel--;
				}
			}
			
			EditorGUI.indentLevel--;
		}
	}

	private void DrawCategoryLevelInfos(SerializedProperty categoryInfo)
	{
		SerializedProperty levelInfos = categoryInfo.FindPropertyRelative("levelInfos");

		// Draw the levelInfos but not its children, this will just draw the foldout (little arrow thingy)
		EditorGUILayout.PropertyField(levelInfos, false);

		DrawLevelInfos(levelInfos, "Level");
	}

	private void DrawLevelInfos(SerializedProperty levelInfos, string prefix)
	{
		// If its expanded then draw its children
		if (levelInfos.isExpanded)
		{
			EditorGUI.indentLevel++;

			// Draw the "Size" property for the levelInfos array
			EditorGUILayout.PropertyField(levelInfos.FindPropertyRelative("Array.size"));

			// Draw each of the LevelInfos
			for (int i = 0; i < levelInfos.arraySize; i++)
			{
				SerializedProperty levelInfo = levelInfos.GetArrayElementAtIndex(i);

				EditorGUILayout.BeginHorizontal();

				// Draw the foldout for the LevelInfo
				EditorGUILayout.PropertyField(levelInfo, new GUIContent(prefix + " " + (i + 1) + " Words"), false);

				bool deleted = false;

				// Draw the remove button so you can remove elements in the middle of the list
				if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
				{
					levelInfos.DeleteArrayElementAtIndex(i);
					deleted = true;
				}

				EditorGUILayout.EndHorizontal();

				// If its not deleted and is expanded
				if (!deleted && levelInfo.isExpanded)
				{
					EditorGUI.indentLevel++;

					DrawLevelInfoWords(levelInfo);

					EditorGUI.indentLevel--;
				}
			}

			EditorGUI.indentLevel--;
		}
	}

	private void DrawLevelInfoWords(SerializedProperty levelInfo)
	{
		SerializedProperty levelWords = levelInfo.FindPropertyRelative("words");

		// Draw the "Size" property for words array
		EditorGUILayout.PropertyField(levelWords.FindPropertyRelative("Array.size"));

		// Draw each of the words in a TextField and a Button beside it that deletes the word when pressed
		for (int i = 0; i < levelWords.arraySize; i++)
		{
			EditorGUILayout.BeginHorizontal();

			// Draw the TextField
			levelWords.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(levelWords.GetArrayElementAtIndex(i).stringValue);

			// Draw the delete button
			if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
			{
				levelWords.DeleteArrayElementAtIndex(i);
			}

			EditorGUILayout.EndHorizontal();
		}
	}

	private void DrawDailyPuzzleLevels()
	{
		SerializedProperty dailyPuzzles = serializedObject.FindProperty("dailyPuzzles");

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("dailyPuzzleDisplayName"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("dailyPuzzleIcon"));

		EditorGUILayout.PropertyField(dailyPuzzles, false);

		DrawLevelInfos(dailyPuzzles, "Puzzle");
	}

	#endregion
}
