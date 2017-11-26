using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class DailyPuzzleButton : MonoBehaviour
{
	#region Inspector Variables

	[SerializeField] private Text timeText;

	#endregion

	#region Member Variables

	private Button button;

	#endregion

	#region Unity Methods

	private void Start()
	{
		button = gameObject.GetComponent<Button>();

		button.onClick.AddListener(() =>
		{
			GameManager.Instance.StartDailyPuzzle();
			UIScreenController.Instance.Show(UIScreenController.GameScreenId);
		});

		Update();
	}

	private void Update()
	{
		if (System.DateTime.Now >= GameManager.Instance.NextDailyPuzzleAt)
		{
			timeText.gameObject.SetActive(false);
			button.interactable = true;
		}
		else
		{
			timeText.gameObject.SetActive(true);
			button.interactable = false;

			System.TimeSpan timeLeft = GameManager.Instance.NextDailyPuzzleAt - System.DateTime.Now;

			string hours	= string.Format("{0}{1}", (timeLeft.Hours < 10 ? "0" : ""), timeLeft.Hours);
			string mins		= string.Format("{0}{1}", (timeLeft.Minutes < 10 ? "0" : ""), timeLeft.Minutes);
			string secs		= string.Format("{0}{1}", (timeLeft.Seconds < 10 ? "0" : ""), timeLeft.Seconds);

			timeText.text = string.Format("{0}:{1}:{2}", hours, mins, secs);
		}
	}

	#endregion
}
