using UnityEngine;
using System.Collections;

/// <summary>
/// Simple Tween class that provides the ability to quickly add dynamic animations to GameObjects.
/// </summary>
public class Tween : MonoBehaviour
{
	#region Enums

	public enum TweenType
	{
		PositionX,
		PositionY,
		PositionZ,
		ScaleX,
		ScaleY,
		ScaleZ,
		Rotation,
		RotationPoint,
		ColourImage,
		ColourText,
		ColourMaterial
	}

	public enum TweenStyle
	{
		Linear,
		EaseIn,
		EaseOut
	}

	public enum LoopType
	{
		None,	// When the Tween finishes it removes itself from the GameObject
		Reset,	// When the Tween finishes it sets the values back to the fromValue and starts over
		Reverse	// When the Tween finishes it "reverses" the Tween by setting the fromValue to equal the toValue and vis-versa
	}

	#endregion

	#region Delegates

	public delegate void OnTweenFinished(GameObject tweenedObject, object[] bundleObjects);

	#endregion

	#region Member Variables

	// Used by all tween types
	private TweenType		tweenType;
	private TweenStyle		tweenStyle;
	private float			duration;
	private double			startTime;
	private double			endTime;
	private LoopType		loopType;
	private bool			useRectTransform;
	private OnTweenFinished	finishCallback;
	private bool			isDestroyed;
	private object[] 		bundleObjects;

	// Used by position and scale tweens
	private float	fromValue;
	private float	toValue;
	private bool	useLocal;

	// Used by rotation tweens
	private Vector3		point;
	private Transform	pointT;
	private Vector3		axis;
	private float		angleSoFar;

	// Used by rotation point tweens
	private Vector3 fromPoint;
	private Vector3 toPoint;

	// Used by colour tweens
	private Color	fromColour;
	private Color	toColour;

	#endregion

	#region Properties

	public Vector3 Point { get { return point; } set { point = value; } }

	#endregion

	#region Unity Methods

	private void Start()
	{
		SetTimes();
	}

	private void Update()
	{
		// Check if the tween has finished
		if (Utilities.SystemTimeInMilliseconds >= endTime)
		{
			switch (loopType)
			{
			case LoopType.None:
				SetToValue();		// Set the value to the toValue
				DestroyTween();
				break;
			case LoopType.Reset:
				SetTimes();			// Reset the startTime and endTime
				Reset();			// Set the values be the the fromValue
				break;
			case LoopType.Reverse:
				SetTimes();			// Reset the startTime and endTime
				SetToValue();		// Set the values to the toValue
				Reverse();			// Swap the from and to values so the tween plays in reverse
				break;
			}

			// Call the finish callback if one was set
			if (finishCallback != null)
			{
				finishCallback(this.gameObject, bundleObjects);
			}
		}
		else
		{
			// Update the values
			switch (tweenType)
			{
			case TweenType.PositionX:
			case TweenType.PositionY:
			case TweenType.PositionZ:
				UpdatePosition(Mathf.Lerp(fromValue, toValue, GetLerpT()));
				break;
			case TweenType.ScaleX:
			case TweenType.ScaleY:
			case TweenType.ScaleZ:
				UpdateScale(Mathf.Lerp(fromValue, toValue, GetLerpT()));
				break;
			case TweenType.Rotation:
				UpdateRotation();
				break;
			case TweenType.RotationPoint:
				UpdateRotationPoint(Vector3.Lerp(fromPoint, toPoint, GetLerpT()));
				break;
			case TweenType.ColourImage:
			case TweenType.ColourText:
			case TweenType.ColourMaterial:
				UpdateColour(Color.Lerp(fromColour, toColour, GetLerpT()));
				break;
			}
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Gets the Tween component on the given GameObject with the given TweenType
	/// </summary>
	public static Tween GetTween(GameObject obj, TweenType tweenType)
	{
		if (obj == null)
		{
			return null;
		}

		Tween[] tweens = obj.GetComponents<Tween>();

		for (int i = 0; i < tweens.Length; i++)
		{
			if (tweens[i].tweenType == tweenType)
			{
				return !tweens[i].isDestroyed ? tweens[i] : null;
			}
		}

		return null;
	}

	/// <summary>
	/// Removes the Tween component on the given GameObject with the given TweenType
	/// </summary>
	public static void RemoveTween(GameObject obj, TweenType tweenType)
	{
		Tween tweenObject = GetTween(obj, tweenType);

		if (tweenObject != null)
		{
			Destroy(tweenObject);
		}
	}

	/// <summary>
	/// Tweens the X position of the GameObject
	/// </summary>
	public static Tween PositionX(Transform transform, TweenStyle tweenStyle, float fromValue, float toValue, float duration, bool transformLocal = false, LoopType loopType = LoopType.None)
	{
		return CreateTween(transform.gameObject, TweenType.PositionX, tweenStyle, fromValue, toValue, duration, transformLocal, loopType);
	}

	/// <summary>
	/// Tweens the Y position of the GameObject
	/// </summary>
	public static Tween PositionY(Transform transform, TweenStyle tweenStyle, float fromValue, float toValue, float duration, bool transformLocal = false, LoopType loopType = LoopType.None)
	{
		return CreateTween(transform.gameObject, TweenType.PositionY, tweenStyle, fromValue, toValue, duration, transformLocal, loopType);
	}

	/// <summary>
	/// Tweens the Z position of the GameObject
	/// </summary>
	public static Tween PositionZ(Transform transform, TweenStyle tweenStyle, float fromValue, float toValue, float duration, bool transformLocal = false, LoopType loopType = LoopType.None)
	{
		return CreateTween(transform.gameObject, TweenType.PositionZ, tweenStyle, fromValue, toValue, duration, transformLocal, loopType);
	}

	/// <summary>
	/// Tweens the X scale of the GameObject
	/// </summary>
	public static Tween ScaleX(Transform transform, TweenStyle tweenStyle, float fromValue, float toValue, float duration, LoopType loopType = LoopType.None)
	{
		return CreateTween(transform.gameObject, TweenType.ScaleX, tweenStyle, fromValue, toValue, duration, true, loopType);
	}
	
	/// <summary>
	/// Tweens the Y scale of the GameObject
	/// </summary>
	public static Tween ScaleY(Transform transform, TweenStyle tweenStyle, float fromValue, float toValue, float duration, LoopType loopType = LoopType.None)
	{
		return CreateTween(transform.gameObject, TweenType.ScaleY, tweenStyle, fromValue, toValue, duration, true, loopType);
	}
	
	/// <summary>
	/// Tweens the Z scale of the GameObject
	/// </summary>
	public static Tween ScaleZ(Transform transform, TweenStyle tweenStyle, float fromValue, float toValue, float duration, LoopType loopType = LoopType.None)
	{
		return CreateTween(transform.gameObject, TweenType.ScaleZ, tweenStyle, fromValue, toValue, duration, true, loopType);
	}

	/// <summary>
	/// Rotates the GameObject around a point on the axis by the given angle
	/// </summary>
	public static Tween RotateAround(Transform transform, TweenStyle tweenStyle, Vector3 point, Vector3 axis, float angle, float duration, LoopType loopType = LoopType.None)
	{
		return CreateRotationTween(transform.gameObject, TweenType.Rotation, tweenStyle, point, axis, angle, duration, loopType);
	}

	/// <summary>
	/// Rotates the GameObject around a point on the axis by the given angle
	/// </summary>
	public static Tween RotateAround(Transform transform, TweenStyle tweenStyle, Transform point, Vector3 axis, float angle, float duration, LoopType loopType = LoopType.None)
	{
		return CreateRotationTween(transform.gameObject, TweenType.Rotation, tweenStyle, point, axis, angle, duration, loopType);
	}

	/// <summary>
	/// Tweens the color of the UI Image
	/// </summary>
	public static Tween Colour(UnityEngine.UI.Image uiImage, TweenStyle tweenStyle, Color fromValue, Color toValue, float duration, LoopType loopType = LoopType.None)
	{
		return CreateColourTween(uiImage.gameObject, TweenType.ColourImage, tweenStyle, fromValue, toValue, duration, loopType);
	}
	
	/// <summary>
	/// Tweens the color of the UI Text
	/// </summary>
	public static Tween Colour(UnityEngine.UI.Text uiText, TweenStyle tweenStyle, Color fromValue, Color toValue, float duration, LoopType loopType = LoopType.None)
	{
		return CreateColourTween(uiText.gameObject, TweenType.ColourText, tweenStyle, fromValue, toValue, duration, loopType);
	}
	
	/// <summary>
	/// Tweens the color of the Material
	/// </summary>
	public static Tween Colour(Renderer renderer, TweenStyle tweenStyle, Color fromValue, Color toValue, float duration, LoopType loopType = LoopType.None)
	{
		return CreateColourTween(renderer.gameObject, TweenType.ColourMaterial, tweenStyle, fromValue, toValue, duration, loopType);
	}

	/// <summary>
	/// Sets the method to call when the Tween has finished (If the tween loops then this callback will be called at the end of each loop).
	/// </summary>
	public void SetFinishCallback(OnTweenFinished finishCallback, params object[] bundleObjects)
	{
		this.finishCallback = finishCallback;
		this.bundleObjects = bundleObjects;
	}

	/// <summary>
	/// If set to true then the transform on the object will be cast to a RectTransform and anchorPosition will be used (If the TweenType is a Position tween)
	/// </summary>
	public void SetUseRectTransform(bool useRectTransform)
	{
		this.useRectTransform = useRectTransform;
	}

	/// <summary>
	/// This will tween the "point" value on a Rotation Tween. Use this if you have a Rotation tween on a GameObject that is moving.
	/// </summary>
	public Tween TweenRotationPoint(TweenStyle tweenStyle, Vector3 fromPoint, Vector3 toPoint, float duration, LoopType loopType = LoopType.None)
	{
		// If the TweenType for the current Tween is not a Rotation then do nothing
		if (tweenType != TweenType.Rotation)
		{
			Debug.LogWarning("Cannot set a TweenType.RotationPoint on a Tween that is not a TweenType.Rotation.");
			return null;
		}

		Tween tween = GetTween(this.gameObject, TweenType.RotationPoint);

		if (tween == null)
		{
			tween = this.gameObject.AddComponent<Tween>();
		}

		tween.tweenType		= TweenType.RotationPoint;
		tween.tweenStyle	= tweenStyle;
		tween.fromPoint		= fromPoint;
		tween.toPoint		= toPoint;
		tween.duration		= duration;
		tween.loopType		= loopType;

		return tween;
	}

	public void DestroyTween()
	{
		Destroy(this);		// Remove the Tween component
		isDestroyed = true;	// Set destroy flag
	}

	#endregion

	#region Private Methods

	private static Tween CreateTween(GameObject obj, TweenType tweenType, TweenStyle tweenStyle, float fromValue, float toValue, float duration, bool transformLocal, LoopType loopType)
	{
		Tween tween = GetTween(obj, tweenType);

		if (tween == null)
		{
			tween = obj.AddComponent<Tween>();
		}

		tween.tweenType			= tweenType;
		tween.tweenStyle		= tweenStyle;
		tween.fromValue			= fromValue;
		tween.toValue			= toValue;
		tween.duration			= duration;
		tween.useLocal			= transformLocal;
		tween.loopType			= loopType;

		return tween;
	}

	private static Tween CreateRotationTween(GameObject obj, TweenType tweenType, TweenStyle tweenStyle, Vector3 point, Vector3 axis, float angle, float duration, LoopType loopType)
	{
		Tween tween = GetTween(obj, tweenType);

		if (tween == null)
		{
			tween = obj.AddComponent<Tween>();
		}

		tween.angleSoFar = 0;

		tween.tweenType			= tweenType;
		tween.tweenStyle		= tweenStyle;
		tween.point				= point;
		tween.pointT			= null;
		tween.axis				= axis;
		tween.fromValue			= 0;
		tween.toValue			= angle;
		tween.duration			= duration;
		tween.loopType			= loopType;

		return tween;
	}

	private static Tween CreateRotationTween(GameObject obj, TweenType tweenType, TweenStyle tweenStyle, Transform point, Vector3 axis, float angle, float duration, LoopType loopType)
	{
		Tween tween = GetTween(obj, tweenType);

		if (tween == null)
		{
			tween = obj.AddComponent<Tween>();
		}

		tween.angleSoFar = 0;

		tween.tweenType			= tweenType;
		tween.tweenStyle		= tweenStyle;
		tween.pointT			= point;
		tween.axis				= axis;
		tween.fromValue			= 0;
		tween.toValue			= angle;
		tween.duration			= duration;
		tween.loopType			= loopType;

		return tween;
	}

	private static Tween CreateColourTween(GameObject obj, TweenType tweenType, TweenStyle tweenStyle, Color fromValue, Color toValue, float duration, LoopType loopType)
	{
		Tween tween = GetTween(obj, tweenType);

		if (tween == null)
		{
			tween = obj.AddComponent<Tween>();
		}

		tween.tweenType			= tweenType;
		tween.tweenStyle		= tweenStyle;
		tween.fromColour		= fromValue;
		tween.toColour			= toValue;
		tween.duration			= duration;
		tween.loopType			= loopType;

		return tween;
	}

	private void SetTimes()
	{
		startTime	= Utilities.SystemTimeInMilliseconds;
		endTime		= startTime + duration;;
	}

	private void Reset()
	{
		switch (tweenType)
		{
		case TweenType.PositionX:
		case TweenType.PositionY:
		case TweenType.PositionZ:
			UpdatePosition(fromValue);
			break;
		case TweenType.ScaleX:
		case TweenType.ScaleY:
		case TweenType.ScaleZ:
			UpdateScale(fromValue);
			break;
		case TweenType.Rotation:
			transform.RotateAround(pointT == null ? point : pointT.position, axis, -toValue);
			angleSoFar = 0;
			break;
		case TweenType.RotationPoint:
			UpdateRotationPoint(fromPoint);
			break;
		case TweenType.ColourImage:
		case TweenType.ColourText:
		case TweenType.ColourMaterial:
			UpdateColour(fromColour);
			break;
		}
	}

	private void SetToValue()
	{
		switch (tweenType)
		{
		case TweenType.PositionX:
		case TweenType.PositionY:
		case TweenType.PositionZ:
			UpdatePosition(toValue);
			break;
		case TweenType.ScaleX:
		case TweenType.ScaleY:
		case TweenType.ScaleZ:
			UpdateScale(toValue);
			break;
		case TweenType.Rotation:
			transform.RotateAround(pointT == null ? point : pointT.position, axis, toValue - angleSoFar);
			angleSoFar = 0;
			break;
		case TweenType.RotationPoint:
			UpdateRotationPoint(toPoint);
			break;
		case TweenType.ColourImage:
		case TweenType.ColourText:
		case TweenType.ColourMaterial:
			UpdateColour(toColour);
			break;
		}
	}

	private void Reverse()
	{
		switch (tweenType)
		{
		case TweenType.PositionX:
		case TweenType.PositionY:
		case TweenType.PositionZ:
		case TweenType.ScaleX:
		case TweenType.ScaleY:
		case TweenType.ScaleZ:
		case TweenType.Rotation:
			float temp	= fromValue;
			fromValue	= toValue;
			toValue		= temp;
			break;
		case TweenType.RotationPoint:
			Vector3 tempV	= fromPoint;
			fromPoint		= toPoint;
			toPoint			= tempV;
			break;
		case TweenType.ColourImage:
		case TweenType.ColourText:
		case TweenType.ColourMaterial:
			Color tempC	= fromColour;
			fromColour	= toColour;
			toColour	= tempC;
			break;
		}
	}

	private void UpdatePosition(float pos)
	{
		switch (tweenType)
		{
		case TweenType.PositionX:
			if (useLocal)
			{
				transform.localPosition = new Vector3(pos, transform.localPosition.y, transform.localPosition.z);
			}
			else if (useRectTransform)
			{
				(transform as RectTransform).anchoredPosition = new Vector2(pos, (transform as RectTransform).anchoredPosition.y);
			}
			else
			{
				transform.position = new Vector3(pos, transform.position.y, transform.position.z);
			}

			break;
		case TweenType.PositionY:
			if (useLocal)
			{
				transform.localPosition = new Vector3(transform.localPosition.x, pos, transform.localPosition.z);
			}
			else if (useRectTransform)
			{
				(transform as RectTransform).anchoredPosition = new Vector2((transform as RectTransform).anchoredPosition.x, pos);
			}
			else
			{
				transform.position = new Vector3(transform.position.x, pos, transform.position.z);
			}

			break;
		case TweenType.PositionZ:
			if (useLocal)
			{
				transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, pos);
			}
			else
			{
				transform.position = new Vector3(transform.position.x, transform.position.y, pos);
			}

			break;
		}
	}

	private void UpdateScale(float scale)
	{
		switch (tweenType)
		{
		case TweenType.ScaleX:
			transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
			break;
		case TweenType.ScaleY:
			transform.localScale = new Vector3(transform.localScale.x, scale, transform.localScale.z);
			break;
		case TweenType.ScaleZ:
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scale);
			break;
		}
	}

	private void UpdateRotation()
	{
		float angle		= Mathf.Lerp(fromValue, toValue, GetLerpT());
		float amount	= angleSoFar - angle;

		transform.RotateAround(pointT == null ? point : pointT.position, axis, amount);

		angleSoFar = angle;
	}

	private void UpdateRotationPoint(Vector3 point)
	{
		Tween rotationTween = GetTween(this.gameObject, TweenType.Rotation);

		if (rotationTween == null)
		{
			DestroyTween();
		}
		else
		{
			rotationTween.Point = point;
		}
	}

	private void UpdateColour(Color colour)
	{
		switch (tweenType)
		{
		case TweenType.ColourImage:
			gameObject.GetComponent<UnityEngine.UI.Image>().color = colour;
			break;
		case TweenType.ColourText:
			gameObject.GetComponent<UnityEngine.UI.Text>().color = colour;
			break;
		case TweenType.ColourMaterial:
			gameObject.GetComponent<Renderer>().material.color = colour;
			break;
		}
	}

	private float GetLerpT()
	{
		float lerpT = (float)(Utilities.SystemTimeInMilliseconds - startTime) / duration;

		switch (tweenStyle)
		{
		case TweenStyle.EaseIn:
			lerpT = lerpT * lerpT * lerpT;
			break;
		case TweenStyle.EaseOut:
			lerpT = 1.0f - (1.0f - lerpT) * (1.0f - lerpT) * (1.0f - lerpT);
			break;
		}

		return lerpT;
	}

	#endregion
}
