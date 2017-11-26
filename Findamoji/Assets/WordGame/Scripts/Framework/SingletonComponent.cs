using UnityEngine;
using System.Collections;

/// <summary>
/// Gets a static instance of the Component that extends this class and makes it accessible through the Instance property.
/// </summary>
public class SingletonComponent<T> : MonoBehaviour where T : Object
{
	#region Member Variables

	private static T instance;

	#endregion

	#region Properties

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				Debug.LogWarningFormat("[SingletonComponent] Returning null instance for component of type {0}.", typeof(T));
			}

			return instance;
		}
	}

	#endregion

	#region Unity Methods

	protected virtual void Awake()
	{
		SetInstance();
	}

	#endregion

	#region Public Methods

	public static bool Exists()
	{
		return instance != null;
	}

	public void SetInstance()
	{
		if (instance != null && instance != gameObject.GetComponent<T>())
		{
			Debug.LogWarning("[SingletonComponent] Instance already set for type " + typeof(T));
			return;
		}

		instance = gameObject.GetComponent<T>();
	}

	#endregion
}
