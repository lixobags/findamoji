using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class UIScreen : MonoBehaviour
{
	#region Inspector Variables

	public string						id;
	public List<GameObject>				worldObjects;

	[Space]

	public bool							showBannerAd;
	public AdsController.BannerPosition	bannerPosition;
	public Color						bannerPlacementColor;
	public int							referenceHeight	= 1920;
	public int							bannerHeight	= 130;

	#endregion

	#region Member Variables

	private GameObject adPlacement;

	#endregion

	#region Properties

	public RectTransform RectT { get { return gameObject.GetComponent<RectTransform>(); } }

	#endregion

	#region Unity Methods

	private void OnDestroy()
	{
		#if ADMOB
		if (AdsController.Exists())
		{
		AdsController.Instance.OnAdsRemoved -= OnAdsRemoved;
		}
		#endif
	}

	#endregion

	#region Public Methods

	public virtual void Initialize() 
	{
		#if ADMOB
		if (AdsController.Exists() && AdsController.Instance.IsBannerAdsEnabled && showBannerAd)
		{
		// Need to setup the UI so the new ad doesnt block anything
		SetupScreenToShowBannerAds();

		// Add a listener so we can remove the ad placement object if ads are removed
		AdsController.Instance.OnAdsRemoved += OnAdsRemoved;
		}
		#endif
	}

	public virtual void OnShowing(object data)
	{
		#if ADMOB
		if (AdsController.Exists() && AdsController.Instance.IsBannerAdsEnabled)
		{
		if (showBannerAd)
		{
		AdsController.Instance.ShowBannerAd(bannerPosition);
		}
		else
		{
		AdsController.Instance.HideBannerAd();
		}
		}
		#endif
	}

	#endregion

	#region Private Methods

	private void OnAdsRemoved()
	{
		// Destroy the ad placement object if ads are removed
		if (adPlacement != null)
		{
			Destroy(adPlacement);
		}
	}

	private void SetupScreenToShowBannerAds()
	{
		GameObject screenContent = new GameObject("screen_content");

		// The banner adds take up 130 pixels on a canvas whos scale is set to 1080x1920, so the remaining height for the screen is 1920 - 130 = 1790
		screenContent.AddComponent<LayoutElement>().preferredHeight = referenceHeight - bannerHeight;

		// Add the new screen content object to this screen
		screenContent.transform.SetParent(transform, false);

		// Move all the children of this screen to the new screen content object
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			Transform childTransform = transform.GetChild(i);

			if (childTransform != screenContent.transform)
			{
				childTransform.SetParent(screenContent.transform, false);
				childTransform.SetAsFirstSibling();
			}
		}

		// Create a spacer for where the add will go
		adPlacement = new GameObject("ad_placement");

		// Ads take up 130 pixels on a canvas whos scale is set to 1080x1920
		adPlacement.AddComponent<LayoutElement>().preferredHeight	= bannerHeight;
		adPlacement.AddComponent<Image>().color						= bannerPlacementColor;

		// Add the ad placement as a child of this screen
		adPlacement.transform.SetParent(transform, false);

		// Set the ads position
		if (bannerPosition == AdsController.BannerPosition.Top)
		{
			adPlacement.transform.SetAsFirstSibling();
		}
		else
		{
			adPlacement.transform.SetAsLastSibling();
		}

		// Add a vertical layout group to auto layout the screen content
		gameObject.AddComponent<VerticalLayoutGroup>();
	}

	#endregion
}
