using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdsController))]
public class AdsControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();

		// Banner Ad properties
		{
			SerializedProperty enableBannerAdsProp = serializedObject.FindProperty("enableAdMobBannerAds");

			EditorGUILayout.PropertyField(enableBannerAdsProp);

			if (enableBannerAdsProp.boolValue)
			{
				#if !ADMOB
				EditorGUILayout.HelpBox("AdMob has not been setup for this project. Please refer to the documentation for how to setup AdMob.", MessageType.Warning);
				#endif
			}

			GUI.enabled = enableBannerAdsProp.boolValue;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("androidBannerAdUnitID"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("iosBannerAdUnitID"));

			GUI.enabled = true;
		}

		EditorGUILayout.Space();

		// Interstital Ad properties
		{
			SerializedProperty enableInterstitialAdsProp = serializedObject.FindProperty("enableInterstitialAds");
			SerializedProperty interstitialtypeProp = serializedObject.FindProperty("interstitialType");

			EditorGUILayout.PropertyField(enableInterstitialAdsProp);

			GUI.enabled = enableInterstitialAdsProp.boolValue;

			EditorGUILayout.PropertyField(interstitialtypeProp);

			if (interstitialtypeProp.enumValueIndex == 0)
			{
				#if !UNITY_ADS
				EditorGUILayout.HelpBox("Unity Ads is not enabled in Unity Services", MessageType.Warning);
				#endif

				EditorGUILayout.PropertyField(serializedObject.FindProperty("enableUnityAdsInEditor"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("zoneId"));
			}
			else
			{
				#if !ADMOB
				EditorGUILayout.HelpBox("AdMob has not been setup for this project. Please refer to the documentation for how to setup AdMob.", MessageType.Warning);
				#endif

				EditorGUILayout.PropertyField(serializedObject.FindProperty("androidInterstitialAdUnitID"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("iosInterstitialAdUnitID"));
			}

			GUI.enabled = true;
		}

		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();
	}
}
