using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IAPController))]
public class IAPControllerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		SerializedProperty enabledProp = serializedObject.FindProperty("enableIAP");

		#if !UNITY_IAP
		if (enabledProp.boolValue)
		{
			EditorGUILayout.Space();

			EditorGUILayout.HelpBox("IAP has not been setup for this project. Please refer to the documentation for how to setup IAP.", MessageType.Warning);

			EditorGUILayout.Space();
		}
		#endif

		base.OnInspectorGUI();
	}
}
