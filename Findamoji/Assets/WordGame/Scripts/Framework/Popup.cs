using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Popup : MonoBehaviour
{
	#region Inspector Variables

	[SerializeField] private float			backgroundAlpha		= 0.35f;
	[SerializeField] private float			animationDuration	= 350f;
	[SerializeField] private Image			background;
	[SerializeField] private RectTransform	uiContainer;

	#endregion

	#region Member Variables

	private bool isShowing;

	#endregion

	#region Public Methods

	public void Show()
	{
		if (isShowing)
		{
			return;
		}

		isShowing = true;

		gameObject.SetActive(true);

		Color fromColor	= background.color;
		Color toColor	= background.color;

		fromColor.a	= 0f;
		toColor.a	= backgroundAlpha ;

		background.color		= fromColor;
		uiContainer.localScale	= Vector3.zero;

		Tween.Colour(background, Tween.TweenStyle.EaseOut, fromColor, toColor, animationDuration);

		Tween.ScaleX(uiContainer, Tween.TweenStyle.EaseOut, 0f, 1f, animationDuration);
		Tween.ScaleY(uiContainer, Tween.TweenStyle.EaseOut, 0f, 1f, animationDuration);
	}

	public void Hide()
	{
		if (!isShowing)
		{
			return;
		}

		isShowing = false;

		Color fromColor	= background.color;
		Color toColor	= background.color;

		fromColor.a	= backgroundAlpha;
		toColor.a	= 0f;

		background.color		= fromColor;
		uiContainer.localScale	= Vector3.one;

		Tween.Colour(background, Tween.TweenStyle.EaseOut, fromColor, toColor, animationDuration);

		Tween.ScaleX(uiContainer, Tween.TweenStyle.EaseOut, 1f, 0f, animationDuration);
		Tween.ScaleY(uiContainer, Tween.TweenStyle.EaseOut, 1f, 0f, animationDuration).SetFinishCallback((tweenedObject, bundleObjects) => 
		{
			gameObject.SetActive(false);
		});
	}

	#endregion
}
