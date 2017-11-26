using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool
{
	#region Member Variables

	private GameObject			objectPrefab		= null;
	private List<GameObject>	instantiatedObjects = new List<GameObject>();
	private Transform			parent				= null;

	#endregion

	#region Public Methods

	/// <summary>
	/// Initializes a new instance of the <see cref="ObjectPooler"/> class.
	/// </summary>
	/// <param name="objectPrefab">The GameObject to instantiate.</param>
	/// <param name="initialSize">Initial amount of objects to instantiate.</param>
	public ObjectPool(GameObject objectPrefab, int initialSize, Transform parent = null)
	{
		this.objectPrefab	= objectPrefab;
		this.parent			= parent;

		for (int i = 0; i < initialSize; i++)
		{
			GameObject obj = CreateObject();
			obj.SetActive(false);
		}
	}

	/// <summary>
	/// Returns an object, it there is no object that can be returned from instantiatedObjects then it creates a new one.
	/// Objects are returned to the pool by setting their active state to false.
	/// </summary>
	public GameObject GetObject()
	{
		for (int i = 0; i < instantiatedObjects.Count; i++)
		{
			if (!instantiatedObjects[i].activeSelf)
			{
				return instantiatedObjects[i];
			}
		}

		return CreateObject();
	}

	/// <summary>
	/// Sets all instantiated GameObjects to de-active
	/// </summary>
	public void ReturnAllObjectsToPool()
	{
		for (int i = 0; i < instantiatedObjects.Count; i++)
		{
			instantiatedObjects[i].SetActive(false);
		}
	}

	#endregion

	#region Private Methods

	private GameObject CreateObject()
	{
		GameObject obj = GameObject.Instantiate(objectPrefab);
		obj.transform.SetParent(parent, false);
		instantiatedObjects.Add(obj);
		return obj;
	}

	#endregion
}
