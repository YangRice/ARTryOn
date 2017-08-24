using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PresentationScript : MonoBehaviour 
{
	[Tooltip("Whether the presentation slides can be changed with gestures (SwipeLeft & SwipeRight).")]
	public bool slideChangeWithGestures = true;
	[Tooltip("Whether the presentation slides can be changed with keys (PgDown & PgUp).")]
	public bool slideChangeWithKeys = true;
	[Tooltip("Speed of spinning, when presentation slides change.")]
	public float spinSpeed = 5;

	[Tooltip("Whether the presentation slides change automatically after the specified delay time.")]
	public bool autoChangeAlfterDelay = false;
	[Tooltip("Delay time (in seconds) after which the slides change, if AutoChangeAlfterDelay is enabled.")]
	public float slideChangeAfterDelay = 10;

	[Tooltip("List of the available presentation textures (slides).")]
	public List<Texture> slideTextures;
	[Tooltip("List of the side planes, comprising the presentation cube.")]
	public List<GameObject> horizontalSides;
	
	[Tooltip("Whether the presentation cube is behind the user or not.")]
	public bool isBehindUser = false;


	private int maxSides = 0;
	private int maxTextures = 0;
	private int side = 0;
	private int tex = 0;
	private bool isSpinning = false;
	private float slideWaitUntil;
	private Quaternion targetRotation;
	
	private GestureListener gestureListener;
	

	void Start() 
	{
		// hide mouse cursor
		Cursor.visible = false;
		
		// calculate max slides and textures
		maxSides = horizontalSides.Count;
		maxTextures = slideTextures.Count;
		
		// delay the first slide
		slideWaitUntil = Time.realtimeSinceStartup + slideChangeAfterDelay;
		
		targetRotation = transform.rotation;
		isSpinning = false;
		
		tex = 0;
		side = 0;
		
		if(horizontalSides[side] && horizontalSides[side].GetComponent<Renderer>())
		{
			horizontalSides[side].GetComponent<Renderer>().material.mainTexture = slideTextures[tex];
		}
		
		// get the gestures listener
		gestureListener = Camera.main.GetComponent<GestureListener>();
	}
	
	void Update() 
	{
		// dont run Update() if there is no user
		KinectManager kinectManager = KinectManager.Instance;
		if(autoChangeAlfterDelay && (!kinectManager || !kinectManager.IsInitialized() || !kinectManager.IsUserDetected()))
			return;
		
		if(!isSpinning)
		{
			if(slideChangeWithKeys)
			{
				if(Input.GetKeyDown(KeyCode.PageDown))
					RotateToNext();
				else if(Input.GetKeyDown(KeyCode.PageUp))
					RotateToPrevious();
			}
			
			if(slideChangeWithGestures && gestureListener)
			{
				if(gestureListener.IsSwipeLeft())
					RotateToNext();
				else if(gestureListener.IsSwipeRight())
					RotateToPrevious();
			}
			
			// check for automatic slide-change after a given delay time
			if(autoChangeAlfterDelay && Time.realtimeSinceStartup >= slideWaitUntil)
			{
				RotateToNext();
			}
		}
		else
		{
			// spin the presentation
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, spinSpeed * Time.deltaTime);
			
			// check if transform reaches the target rotation. If yes - stop spinning
			float deltaTargetX = Mathf.Abs(targetRotation.eulerAngles.x - transform.rotation.eulerAngles.x);
			float deltaTargetY = Mathf.Abs(targetRotation.eulerAngles.y - transform.rotation.eulerAngles.y);
			
			if(deltaTargetX < 1f && deltaTargetY < 1f)
			{
				// delay the slide
				slideWaitUntil = Time.realtimeSinceStartup + slideChangeAfterDelay;
				isSpinning = false;
			}
		}
	}
	
	
	private void RotateToNext()
	{
		// set the next texture slide
		tex = (tex + 1) % maxTextures;
		
		if(!isBehindUser)
		{
			side = (side + 1) % maxSides;
		}
		else
		{
			if(side <= 0)
				side = maxSides - 1;
			else
				side -= 1;
		}

		if(horizontalSides[side] && horizontalSides[side].GetComponent<Renderer>())
		{
			horizontalSides[side].GetComponent<Renderer>().material.mainTexture = slideTextures[tex];
		}
		
		// rotate the presentation
		float yawRotation = !isBehindUser ? 360f / maxSides : -360f / maxSides;
		Vector3 rotateDegrees = new Vector3(0f, yawRotation, 0f);
		targetRotation *= Quaternion.Euler(rotateDegrees);
		isSpinning = true;
	}
	
	
	private void RotateToPrevious()
	{
		// set the previous texture slide
		if(tex <= 0)
			tex = maxTextures - 1;
		else
			tex -= 1;
		
		if(!isBehindUser)
		{
			if(side <= 0)
				side = maxSides - 1;
			else
				side -= 1;
		}
		else
		{
			side = (side + 1) % maxSides;
		}
		
		if(horizontalSides[side] && horizontalSides[side].GetComponent<Renderer>())
		{
			horizontalSides[side].GetComponent<Renderer>().material.mainTexture = slideTextures[tex];
		}
		
		// rotate the presentation
		float yawRotation = !isBehindUser ? -360f / maxSides : 360f / maxSides;
		Vector3 rotateDegrees = new Vector3(0f, yawRotation, 0f);
		targetRotation *= Quaternion.Euler(rotateDegrees);
		isSpinning = true;
	}
	
	
}
