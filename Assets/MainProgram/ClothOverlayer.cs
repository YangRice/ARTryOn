using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClothOverlayer : MonoBehaviour
{
    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture backgroundImage;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Game object used to overlay the joint.")]
    public Transform clothObject;

    [Tooltip("Game object used to rendering.")]
    public MeshFilter clothMeshFilter;
    public Mesh clothMesh;
    public Bounds ClothBounds { get { return clothMesh.bounds; } }
    //public float smoothFactor = 10f;

    //public GUIText debugText;

    private Quaternion initialRotation = Quaternion.identity;

    [Tooltip("Joints that need to be tracked.")]
    public KinectInterop.JointType[] checkJoints;

    private KinectManager Manager { get { return KinectManager.Instance; } }
    private long UserID { get { return Manager.GetUserIdByIndex(playerIndex); } }
    private Vector3 ClothAnchorPosition
    {
        get
        {
            return Manager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineShoulder, Camera.main, Camera.main.pixelRect);
        }
    }
    private Quaternion ClothAnchorRotation
    {
        get
        {
            var axisX =
                Manager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.ShoulderLeft, Camera.main, Camera.main.pixelRect) -
                Manager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.ShoulderRight, Camera.main, Camera.main.pixelRect);
            var axisY =
                Manager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineShoulder, Camera.main, Camera.main.pixelRect) -
                Manager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineMid, Camera.main, Camera.main.pixelRect);
            var axisZ = Vector3.Cross(axisX, axisY);
            return Quaternion.LookRotation(-axisZ.normalized, axisY.normalized);
        }
    }

    Vector3[] originVertices = null;
    Vector3[] deformedVertices = null;

    void Start()
    {
        if (clothObject)
        {
            // always mirrored
            initialRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
            clothObject.rotation = Quaternion.identity;
        }

        if (clothMeshFilter)
        {
            clothMesh = clothMeshFilter.mesh;
            originVertices = (Vector3[])clothMesh.vertices.Clone();
            deformedVertices = (Vector3[])clothMesh.vertices.Clone();
            //originVertices = new Vector3[clothMesh.vertexCount];
        }
    }

    void Tracking(KinectManager manager)
    {
        //backgroundImage.renderer.material.mainTexture = manager.GetUsersClrTex();
        if (backgroundImage && (backgroundImage.texture == null))
        {
            backgroundImage.texture = manager.GetUsersClrTex();
        }

        if (!manager.IsUserDetected())
        {
            return;
        }

        if (checkJoints.Any(i => !Manager.IsJointTracked(UserID, (int)i)))
        {
            return;
        }

        if (clothObject)
        {
            clothObject.position = ClothAnchorPosition;
            clothObject.rotation = ClothAnchorRotation;
        }
    }

    public float shoulderScale = 1.0f;
    public float breastScale = 1.0f;
    public float hipScale = 1.0f;
    public float tallScale = 1.0f;
    public Transform shoulderAnchor;
    public Transform breastAnchor;
    public Transform hipAnchor;

    float CalcDistScore(float x, float y, float max = float.MaxValue, float min = float.MinValue)
    {
        float d = x - y;
        d = ((d > max ? max : d) < min ? min : d);
        return Mathf.Exp(-0.5f * d * d / 0.04f);
    }

    float[] Softmax(params float[] scores)
    {
        float[] probs = new float[scores.Length];
        float psum = 0.0f;
        for (int i = 0; i < scores.Length; i++)
        {
            psum += (probs[i] = Mathf.Exp(scores[i]));
        }
        for (int i = 0; i < scores.Length; i++)
        {
            probs[i] /= psum;
        }
        return probs;
    }

    void Deform()
    {
        var bounds = ClothBounds;
        Vector3 center = bounds.center;
        for (int i = 0; i < originVertices.Length; i++)
        {
            var vertex = originVertices[i];
            float shoulderScore = CalcDistScore(vertex.y, shoulderAnchor.localPosition.y, max: 0.0f);
            float breastScore = CalcDistScore(vertex.y, breastAnchor.localPosition.y);
            float hipScore = CalcDistScore(vertex.y, hipAnchor.localPosition.y, min: 0.0f);
            float[] softmax = Softmax(shoulderScore, breastScore, hipScore);
            float shoulderProb = softmax[0];
            float breastProb = softmax[1];
            float hipProb = softmax[2];
            float scaleVector =
                shoulderProb * shoulderScale +
                breastProb * breastScale +
                hipProb * hipScale;
            Vector3 displacement = (vertex - center);
            deformedVertices[i] = center + displacement * scaleVector;
            deformedVertices[i].y = bounds.max.y + tallScale * (vertex.y - bounds.max.y);
        }
        clothMesh.vertices = deformedVertices;
    }

    void Update()
    {
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            Tracking(manager);
        }

        if (clothMeshFilter)
        {
            Deform();
        }
    }
}
