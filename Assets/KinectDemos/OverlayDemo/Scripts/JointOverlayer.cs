using UnityEngine;
using System.Collections;
using Windows.Kinect;


public class JointOverlayer : MonoBehaviour 
{
	[Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
	public GUITexture backgroundImage;

	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Kinect joint that is going to be overlayed.")]
	public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;

	[Tooltip("Game object used to overlay the joint.")]
	public Transform overlayObject;
	//public float smoothFactor = 10f;
	
	//public GUIText debugText;

	private Quaternion initialRotation = Quaternion.identity;

	
	void Start()
	{
		if(overlayObject)
		{
			// always mirrored
			initialRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
			overlayObject.rotation = Quaternion.identity;
		}
	}
	
	void Update () 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			//backgroundImage.renderer.material.mainTexture = manager.GetUsersClrTex();
			if(backgroundImage && (backgroundImage.texture == null))
			{
				backgroundImage.texture = manager.GetUsersClrTex();
			}
			
			int iJointIndex = (int)trackedJoint;

			if(manager.IsUserDetected())
			{
				long userId = manager.GetUserIdByIndex(playerIndex);
				
				if(manager.IsJointTracked(userId, iJointIndex))
				{
					Vector3 posJoint = manager.GetJointPosColorOverlay(userId, iJointIndex, Camera.main, Camera.main.pixelRect);

					if(posJoint != Vector3.zero)
					{
//						if(debugText)
//						{
//							debugText.GetComponent<GUIText>().text = string.Format("{0} - {1}", trackedJoint, posJoint);
//						}

						if(overlayObject)
						{
							//overlayObject.position = Vector3.Lerp(overlayObject.position, posJoint, smoothFactor * Time.deltaTime);
							overlayObject.position = posJoint;

							Quaternion rotJoint = manager.GetJointOrientation(userId, iJointIndex, false);
							rotJoint = initialRotation * rotJoint;

							//overlayObject.rotation = Quaternion.Slerp(overlayObject.rotation, rotJoint, smoothFactor * Time.deltaTime);
							overlayObject.rotation = rotJoint;
						}
					}
				}
				
			}
			
		}
	}
}
