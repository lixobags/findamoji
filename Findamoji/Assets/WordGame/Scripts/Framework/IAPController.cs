using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

#if UNITY_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

public class IAPController : SingletonComponent<IAPController>
#if UNITY_IAP
, IStoreListener
#endif
{
	#region Classes

	[System.Serializable]
	private class ProductInfo
	{
		public string	displayName	= "";
		public string	productId	= "";
		public bool		consumable	= false;
	}

	#endregion

	#region Inspector Variables

	[Space]

	[SerializeField] private bool enableIAP;

	[Space]

	[SerializeField] private IAPButtonUI	iapButtonUIPrefab;
	[SerializeField] private Transform		iapButtonUIParent;
	[SerializeField] private Button			restorePurchaseButton;

	[Space]

	[SerializeField] private string	removeAdsDisplayName;
	[SerializeField] private string	removeAdsProductId;
	[SerializeField] private bool	anyPurchaseRemovesAds;

	[Space]

	[SerializeField] private List<ProductInfo> productInfos;

	#endregion

	#region Member Variables

	#if UNITY_IAP
	private IStoreController	storeController;
	private IExtensionProvider 	extensionProvider;
	private List<IAPButtonUI> 	iapButtonUIs;
	#endif


	#endregion

	#region Properties

	/// <summary>
	/// Returns true id IAP is enabled, false otherwise
	/// </summary>
	public static bool IsEnabled
	{
		get
		{
			#if UNITY_IAP
			return IAPController.Exists() && IAPController.Instance.enableIAP;
			#else
			return false;
			#endif
		}
	}

	/// <summary>
	/// Gets the remove ads product id
	/// </summary>
	public string RemoveAdsProductId { get { return removeAdsProductId; } }

	/// <summary>
	/// Callback this is invoked when a product is purchased, passes the product id that was purchased
	/// </summary>
	public System.Action<string> OnProductPurchased	{ get; set; }

	/// <summary>
	/// Returns true if IAP is initialized
	/// </summary>
	private bool IsInitialized
	{
		#if UNITY_IAP
		get { return storeController != null && extensionProvider != null; }
		#else
		get { return false; }
		#endif
	}

	/// <summary>
	/// Gets a list of all product (including remove ads)
	/// </summary>
	private List<ProductInfo> Products
	{
		get
		{
			List<ProductInfo> products = new List<ProductInfo>(productInfos);

			if (!string.IsNullOrEmpty(removeAdsProductId))
			{
				ProductInfo productInfo	= new ProductInfo();
				productInfo.displayName	= removeAdsDisplayName;
				productInfo.productId	= removeAdsProductId;
				productInfo.consumable	= false;

				// Insert the remove ads at the beginning of the list so it appears
				products.Insert(0, productInfo);
			}

			return products;
		}
	}

	/// <summary>
	/// String of non-consumable product ids that have been purchase seperated by tabs
	/// </summary>
	private string PurchasedProductIdsString
	{
		get { return PlayerPrefs.GetString("crossword_purchased_products", ""); }
		set { PlayerPrefs.SetString("crossword_purchased_products", value); }
	}

	/// <summary>
	/// List of non-consumable product ids that have been purchased
	/// </summary>
	private List<string> PurchasedProductIds
	{
		get { return string.IsNullOrEmpty(PurchasedProductIdsString) ? new List<string>() : new List<string>(PurchasedProductIdsString.Split('\t')); }
	}

	#endregion

	#region Unity Methods

	private void Start()
	{
		#if UNITY_IAP
		iapButtonUIs = new List<IAPButtonUI>();

		// Show the restore purchase button if this platform is iOS
		restorePurchaseButton.gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer);
		restorePurchaseButton.onClick.AddListener(RestorePurchases);

		// Initialize IAP
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

		// Get the list of products and add each one to the builder
		List<ProductInfo> products = Products;

		for (int i = 0; i < products.Count; i++)
		{
			builder.AddProduct(products[i].productId, products[i].consumable ? ProductType.Consumable : ProductType.NonConsumable);
		}

		UnityPurchasing.Initialize(this, builder);
		#endif
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Returns true if the given product id has been purchased, only for non-consumable products, consumable products will always return false.
	/// </summary>
	public bool IsProductPurchased(string productId)
	{
		return PurchasedProductIds.Contains(productId);
	}

	#region IStoreListener

	#if UNITY_IAP
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		Debug.Log("[IAPController] Initializion Successful");

		storeController		= controller;
		extensionProvider	= extensions;

		UpdateIAPButtons();
	}

	public void OnInitializeFailed(InitializationFailureReason failureReason)
	{
		Debug.LogError("[IAPController] Initializion Failed: " + failureReason);
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		Debug.LogError("[IAPController] Purchase Failed: productId: " + product.definition.id + ", reason: " + failureReason);
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		// Get the product id for the product that was just purchased
		ProductPurchased(args.purchasedProduct.definition.id, args.purchasedProduct.definition.type == ProductType.Consumable);

		UpdateIAPButtons();

		return PurchaseProcessingResult.Complete;
	}
	#endif

	#endregion

	#endregion

	#region Private Methods

	#if UNITY_IAP
	private void UpdateIAPButtons()
	{
		for (int i = 0; i < iapButtonUIs.Count; i++)
		{
			Destroy(iapButtonUIs[i].gameObject);
		}

		iapButtonUIs.Clear();

		// Get the list of products and add a button for each one
		List<ProductInfo> products = Products;

		for (int i = 0; i < products.Count; i++)
		{
			ProductInfo productInfo = products[i];
			Product		product		= storeController.products.WithID(productInfo.productId);
			IAPButtonUI	iapButtonUI	= Instantiate(iapButtonUIPrefab);

			iapButtonUI.transform.SetParent(iapButtonUIParent, false);

			iapButtonUI.nameText.text		= productInfo.displayName;
			iapButtonUI.priceText.text		= product.metadata.localizedPriceString;
			iapButtonUI.ProductId			= product.definition.id;
			iapButtonUI.OnIAPButtonClicked	= BuyProduct;

			bool purchased = IsProductPurchased(product.definition.id);

			iapButtonUI.purchasedIndicator.SetActive(purchased);
			iapButtonUI.button.interactable	= !purchased;

			iapButtonUIs.Add(iapButtonUI);
		}
	}

	private void BuyProduct(string productId)
	{
		if (IsInitialized)
		{
			Product product = storeController.products.WithID(productId);

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				storeController.InitiatePurchase(product);
			}
		}
	}
	#endif

	/// <summary>
	/// Restores the purchases if platform is iOS or OSX
	/// </summary>
	private void RestorePurchases()
	{
		if (IsInitialized && (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer))
		{
			#if UNITY_IAP
			extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result) => {});
			#endif
		}
	}

	private void ProductPurchased(string productId, bool consumable)
	{
		if (!consumable)
		{
			// Add the product id to the list of purchased products so it appears as purchased in the store
			if (!string.IsNullOrEmpty(PurchasedProductIdsString))
			{
				PurchasedProductIdsString += "\t";
			}

			PurchasedProductIdsString += productId;
		}

		// Invoke the callback so other controllers can update their state
		if (OnProductPurchased != null)
		{
			OnProductPurchased(productId);
		}

		// If something other than the remove ads product was purchased and anyPurchaseRemovesAds is true call
		// ProductPurchased on the remove ads product id to set it as purchased aswell
		if (productId != removeAdsProductId && anyPurchaseRemovesAds && !IsProductPurchased(removeAdsProductId))
		{
			ProductPurchased(removeAdsProductId, false);
		}
	}

	#endregion
}
