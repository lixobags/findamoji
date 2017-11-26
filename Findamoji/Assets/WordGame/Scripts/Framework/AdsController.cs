using System;
using UnityEngine;
using UnityEngine.UI;

#if ADMOB
using GoogleMobileAds;
using GoogleMobileAds.Api;
#endif

// Example script showing how to invoke the Google Mobile Ads Unity plugin.
public class AdsController : SingletonComponent<AdsController>
{
	#region Enums

	public enum BannerPosition
	{
		Top,
		Bottom
	}

	public enum InterstitialType
	{
		UnityAds,
		AdMob
	}

	private enum BannerState
	{
		Idle,
		Loading,
		Loaded
	}

	#endregion

	#region Inspector Variables

	[SerializeField] private bool				enableAdMobBannerAds;
	[SerializeField] private string				androidBannerAdUnitID;
	[SerializeField] private string				iosBannerAdUnitID;

	[SerializeField] private bool				enableInterstitialAds;
	[SerializeField] private InterstitialType 	interstitialType;
	[SerializeField] private string				androidInterstitialAdUnitID;
	[SerializeField] private string				iosInterstitialAdUnitID;
	[SerializeField] private bool				enableUnityAdsInEditor;
	[SerializeField] private string				zoneId;

	#endregion

	#region Member Variables

	#if ADMOB
	private BannerView		topBanner;
	private BannerView		bottomBanner;
	private InterstitialAd	interstitial;
	#endif

	private BannerState		topBannerState;
	private BannerState		bottomBannerState;

	private bool			isInterstitialAdLoaded;
	private System.Action	interstitialAdClosedCallback;

	#endregion

	#region Properties

	public bool IsAdsEanbledInPlatform
	{
		get
		{
			#if !UNITY_ANDROID && !UNITY_IPHONE
			return false;
			#else
			// We want to return true when in the editor so we can see the effects of layout changes with respect to banner ads
			return true;
			#endif
		}
	}

	public bool				IsBannerAdsEnabled			{ get { return !RemoveAds && IsAdsEanbledInPlatform && enableAdMobBannerAds; } }
	public bool				IsInterstitialAdsEnabled	{ get { return !RemoveAds && IsAdsEanbledInPlatform && enableInterstitialAds; } }
	public System.Action	OnAdsRemoved				{ get; set; }

	#if UNITY_ANDROID
	private string BannderAdUnitId { get { return androidBannerAdUnitID; } }
	#elif UNITY_IPHONE
	private string BannderAdUnitId { get { return iosBannerAdUnitID; } }
	#else
	private string BannderAdUnitId { get { return "unexpected_platform"; } }
	#endif

	#if UNITY_ANDROID
	private string InterstitialAdUnitId { get { return androidInterstitialAdUnitID; } }
	#elif UNITY_IPHONE
	private string InterstitialAdUnitId { get { return iosInterstitialAdUnitID; } }
	#else
	private string InterstitialAdUnitId { get { return "unexpected_platform"; } }
	#endif

	private bool RemoveAds
	{
		get { return IAPController.IsEnabled && IAPController.Instance.IsProductPurchased(IAPController.Instance.RemoveAdsProductId); }
	}

	#endregion

	#region Unity Methods

	private void Start()
	{
		#if ADMOB
		if (IsInterstitialAdsEnabled && interstitialType == InterstitialType.AdMob)
		{
			// Create an InterstitialAd
			interstitial = new InterstitialAd(InterstitialAdUnitId);

			interstitial.OnAdLoaded += OnInterstitialAdLoaded;
			interstitial.OnAdClosed += OnInterstitialAdClosed;

			// Pre-load the Interstitial Ad
			LoadInterstitialAd();
		}
		#endif

		if (IAPController.IsEnabled)
		{
			IAPController.Instance.OnProductPurchased += OnIAPProductPurchased;
		}
	}

	private void OnDestroy()
	{
		#if ADMOB
		DestroyAdMobObjects();
		#endif

		if (IAPController.IsEnabled)
		{
			IAPController.Instance.OnProductPurchased -= OnIAPProductPurchased;
		}
	}

	#endregion

	#region Public Methods

	#if ADMOB
	/// <summary>
	/// Shows the banner ad if banner ads are enabled
	/// </summary>
	public void ShowBannerAd(BannerPosition bannerPosition)
	{
		if (IsBannerAdsEnabled)
		{
			switch (bannerPosition)
			{
			case BannerPosition.Top:
				if (topBannerState != BannerState.Loaded)
				{
					LoadTopBanner();
				}
				else
				{
					topBanner.Show();
				}
				break;
			case BannerPosition.Bottom:
				if (bottomBannerState != BannerState.Loaded)
				{
					LoadBottomBanner();
				}
				else
				{
					bottomBanner.Show();
				}
				break;
			}
		}
	}

	/// <summary>
	/// Hides the banner ad is banner ads are enabled
	/// </summary>
	public void HideBannerAd()
	{
		if (IsBannerAdsEnabled)
		{
			if (topBanner != null)
			{
				topBanner.Hide();
			}

			if (bottomBanner != null)
			{
				bottomBanner.Hide();
			}
		}
	}
	#endif

	/// <summary>
	/// Shows the interstital ad but only it it's been loaded. Returns true if the ad is shown, false otherwise.
	/// </summary>
	public bool ShowInterstitialAd(System.Action onAdClosed = null)
	{
		bool adShown = false;

		if (IsInterstitialAdsEnabled)
		{
			switch (interstitialType)
			{
			case InterstitialType.UnityAds:
				#if UNITY_ADS
				{
					#if UNITY_EDITOR
					{
						if (!enableUnityAdsInEditor)
						{
							break;
						}
					}
					#endif
					
					interstitialAdClosedCallback = onAdClosed;

					UnityEngine.Advertisements.ShowOptions adShowOptions = new UnityEngine.Advertisements.ShowOptions();
					
					adShowOptions.resultCallback = OnUnityAdsInterstitalClosed;
					
					UnityEngine.Advertisements.Advertisement.Show(zoneId, adShowOptions);

					adShown = true;
				}
				#else
				Debug.LogError("[AdsController] Unity Ads are not enabled in services");
				#endif

				break;
			case InterstitialType.AdMob:
				#if ADMOB
				if (interstitial != null && isInterstitialAdLoaded)
				{
					interstitialAdClosedCallback = onAdClosed;

					interstitial.Show();

					adShown = true;
				}
				#endif

				break;
			}
		}

		return adShown;
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Called by the IAPController when the player makes a purchase
	/// </summary>
	private void OnIAPProductPurchased(string productId)
	{
		// Check if the player purchased the remove ads IAP
		if (productId == IAPController.Instance.RemoveAdsProductId)
		{
			#if ADMOB
			// Destroy all ad mob objects
			DestroyAdMobObjects();
			#endif

			if (OnAdsRemoved != null)
			{
				OnAdsRemoved();
			}
		}
	}

	#if UNITY_ADS
	private void OnUnityAdsInterstitalClosed(UnityEngine.Advertisements.ShowResult adShowResult)
	{
		if (interstitialAdClosedCallback != null)
		{
			interstitialAdClosedCallback();
		}
	}
	#endif

	#if ADMOB
	/// <summary>
	/// Loads the interstitial ad
	/// </summary>
	private void LoadInterstitialAd()
	{
		isInterstitialAdLoaded = false;

		interstitial.LoadAd(CreateAdRequest());
	}

	/// <summary>
	/// Loads the top banner if it is not already loaded or loading
	/// </summary>
	private void LoadTopBanner()
	{
		if (topBannerState == BannerState.Idle)
		{
			if (topBanner == null)
			{
				// Create the banner view 
				topBanner = new BannerView(BannderAdUnitId, AdSize.SmartBanner, AdPosition.Top);

				// Set the event callbacks for the top banner
				topBanner.OnAdLoaded		+= OnTopBannerLoaded;
				topBanner.OnAdFailedToLoad	+= OnTopBannerFailedToLoad;
			}

			// Set the state to loading
			topBannerState = BannerState.Loading;

			// Load and show the banner
			topBanner.LoadAd(CreateAdRequest());
		}
	}

	/// <summary>
	/// Loads the bottom banner if it is not already loaded or loading
	/// </summary>
	private void LoadBottomBanner()
	{
		if (bottomBannerState == BannerState.Idle)
		{
			if (bottomBanner == null)
			{
				// Create the banner view 
				bottomBanner = new BannerView(BannderAdUnitId, AdSize.SmartBanner, AdPosition.Bottom);

				// Set the event callbacks for the bottom banner
				bottomBanner.OnAdLoaded			+= OnBottomBannerLoaded;
				bottomBanner.OnAdFailedToLoad	+= OnBottomBannerFailedToLoad;
			}

			// Set the state to loading
			bottomBannerState = BannerState.Loading;

			// Load and show the banner
			bottomBanner.LoadAd(CreateAdRequest());
		}
	}

	/// <summary>
	/// Creates a new Ad request to be used by banners and interstitial ads
	/// </summary>
	/// <returns>The ad request.</returns>
	private AdRequest CreateAdRequest()
	{
		return new AdRequest.Builder()
			.AddTestDevice(AdRequest.TestDeviceSimulator)
			.AddTestDevice("D23859A2702727667C1848E7B932B4C4")
			.Build();
	}

	/// <summary>
	/// Destroys the ad mob objects
	/// </summary>
	private void DestroyAdMobObjects()
	{
		if (topBanner != null)
		{
			topBanner.Hide();
			topBanner.Destroy();
		}

		if (bottomBanner != null)
		{
			bottomBanner.Hide();
			bottomBanner.Destroy();
		}

		if (interstitial != null)
		{
			interstitial.Destroy();
		}
	}

	/// <summary>
	/// Invoked when the top banner has loaded
	/// </summary>
	private void OnTopBannerLoaded(object sender, EventArgs args)
	{
		// Top banner succeffully loaded
		topBannerState = BannerState.Loaded;
	}

	/// <summary>
	/// Invoked when the top banner fails to load
	/// </summary>
	private void OnTopBannerFailedToLoad(object sender, AdFailedToLoadEventArgs e)
	{
		// Failed to load top banner, set the state to Idle so we can try and load it again next time
		topBannerState = BannerState.Idle;
	}

	/// <summary>
	/// Invoked when the bottom banner has loaded
	/// </summary>
	private void OnBottomBannerLoaded(object sender, EventArgs args)
	{
		// Bottom banner succeffully loaded
		bottomBannerState = BannerState.Loaded;
	}

	/// <summary>
	/// Invoked when the bottom banner fails to load
	/// </summary>
	private void OnBottomBannerFailedToLoad(object sender, AdFailedToLoadEventArgs e)
	{
		// Failed to load bottom banner, set the state to Idle so we can try and load it again next time
		bottomBannerState = BannerState.Idle;
	}

	/// <summary>
	/// Invoked when the interstitial ad has loaded
	/// </summary>
	private void OnInterstitialAdLoaded(object sender, EventArgs args)
	{
		isInterstitialAdLoaded = true;
	}

	/// <summary>
	/// Invoked when the interstitial ad is closed
	/// </summary>
	private void OnInterstitialAdClosed(object sender, EventArgs args)
	{
		// Pre-load the next interstitial ad
		LoadInterstitialAd();

		// Call the callback that was passed in the show method
		if (interstitialAdClosedCallback != null)
		{
			interstitialAdClosedCallback();
		}
	}
	#endif

	#endregion
}