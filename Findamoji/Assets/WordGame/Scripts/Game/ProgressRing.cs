using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressRing : MonoBehaviour
{
	#region Inspector Variables

	[SerializeField] private RectTransform	firstHalf;
	[SerializeField] private RectTransform	secondHalf;
	[SerializeField] private Text 			percentText;

	#endregion

	#region Member Variables
	#endregion

	#region Properties
	#endregion

	#region Unity Methods

	private void Awake()
	{
		SetProgress(0f);
	}

	#endregion

	#region Public Methods

	public void SetProgress(float percent)
	{
		percentText.text = Mathf.RoundToInt(percent * 100f) + "%";

		float z1 = Mathf.Lerp(180f, 0f, Mathf.Clamp01(percent * 2f));
		float z2 = Mathf.Lerp(180f, 0f, Mathf.Clamp01((percent - 0.5f) * 2f));

		firstHalf.localEulerAngles	= new Vector3(firstHalf.localEulerAngles.x, firstHalf.localEulerAngles.y, z1);
		secondHalf.localEulerAngles	= new Vector3(secondHalf.localEulerAngles.x, secondHalf.localEulerAngles.y, z2);
	}

	#endregion

	#region Protected Methods
	#endregion

	#region Private Methods
	#endregion
}
