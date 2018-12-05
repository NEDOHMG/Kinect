using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Track : MonoBehaviour
{

    Dictionary<Windows.Kinect.JointType, HumanBodyBones> kinectJointTypeToHumanBodyBone = new Dictionary<Windows.Kinect.JointType, HumanBodyBones>()
    {
        {Windows.Kinect.JointType.SpineBase, HumanBodyBones.Hips},
        {Windows.Kinect.JointType.SpineMid, HumanBodyBones.Spine},
        {Windows.Kinect.JointType.SpineShoulder, HumanBodyBones.Chest},
        {Windows.Kinect.JointType.Neck, HumanBodyBones.Neck},
        {Windows.Kinect.JointType.Head, HumanBodyBones.Head}, //TODO: Need facial tracking or Oculus for head orientation

		{Windows.Kinect.JointType.ShoulderRight, HumanBodyBones.RightShoulder},
        {Windows.Kinect.JointType.ElbowRight, HumanBodyBones.RightUpperArm},
        {Windows.Kinect.JointType.WristRight, HumanBodyBones.RightLowerArm},
        {Windows.Kinect.JointType.HandRight, HumanBodyBones.RightHand},
		//{Windows.Kinect.JointType.HandTipRight, HumanBodyBones.RightMiddleProximal},
		{Windows.Kinect.JointType.ShoulderLeft, HumanBodyBones.LeftShoulder},
        {Windows.Kinect.JointType.ElbowLeft, HumanBodyBones.LeftUpperArm},
        {Windows.Kinect.JointType.WristLeft, HumanBodyBones.LeftLowerArm},
        {Windows.Kinect.JointType.HandLeft, HumanBodyBones.LeftHand},
		//{Windows.Kinect.JointType.HandTipLeft, HumanBodyBones.LeftMiddleProximal},

		//{Windows.Kinect.JointType.HipRight, }, // No HumanBodyBones analog
		{Windows.Kinect.JointType.KneeRight, HumanBodyBones.RightUpperLeg},
        {Windows.Kinect.JointType.AnkleRight, HumanBodyBones.RightLowerLeg},
        {Windows.Kinect.JointType.FootRight, HumanBodyBones.RightFoot}, 

		//{Windows.Kinect.JointType.HipLeft, }, // No HumanBodyBones analog
		{Windows.Kinect.JointType.KneeLeft, HumanBodyBones.LeftUpperLeg},
        {Windows.Kinect.JointType.AnkleLeft, HumanBodyBones.LeftLowerLeg},
        {Windows.Kinect.JointType.FootLeft, HumanBodyBones.LeftFoot},
    };

    // Assign in editor
    public BodySourceManager bodySourceManager;
    //Windows.Kinect.Body[] bodies;
    Quaternion[] jointOrientations;
    Transform[] avatarBones;
    int numJointsTracked = 25;
    Animator animator;
    private Dictionary<Windows.Kinect.JointType, JointManipulation> jointManipulations;

    void Start()
    {
        jointOrientations = new Quaternion[numJointsTracked];
        avatarBones = new Transform[numJointsTracked];
        animator = GetComponent<Animator>();
        foreach (var joint in kinectJointTypeToHumanBodyBone.Keys)
        {
            avatarBones[(int)joint] = animator.GetBoneTransform(kinectJointTypeToHumanBodyBone[joint]);
        }
        InitializeJointManipulationData();
    }

    void Update()
    {
        if (!bodySourceManager)
            return;

        var data = bodySourceManager.GetData();
        if (data == null)
        {
            return;
        }

        // Use the data of the first body 
        var body = data.FirstOrDefault(b => b.IsTracked);
        if (body == null)
        {
            return;
        }

        // Look for a tracked body

        // Save the orientation data of each joint
        foreach (var joint in kinectJointTypeToHumanBodyBone.Keys)
        {
            var x = body.JointOrientations[joint].Orientation.X;
            var y = body.JointOrientations[joint].Orientation.Y;
            var z = body.JointOrientations[joint].Orientation.Z;
            var w = body.JointOrientations[joint].Orientation.W;
            var rawJointOrientation = new Quaternion(x, y, z, w);

            jointOrientations[(int)joint] = ManipulateKinectJointOrientation(rawJointOrientation, joint);
        }

        UpdateJoints();
    }

    void UpdateJoints()
    {
        // For each Kinect Joint...
        foreach (var joint in kinectJointTypeToHumanBodyBone.Keys)
        {
            avatarBones[(int)joint].rotation = jointOrientations[(int)joint];
        }
    }

    // Manipulate the kinect quaternions on a per-joint basis to unite the orientation data under a common coordinate system.
    private Quaternion ManipulateKinectJointOrientation(Quaternion kinectQuaternion, Windows.Kinect.JointType jointType)
    {
        var euler = kinectQuaternion.eulerAngles;
        if (jointManipulations.ContainsKey(jointType))
        {
            var jointManipulation = jointManipulations[jointType];
            euler = new Vector3(euler.x * jointManipulation.Sign.x + jointManipulation.Offset.x,
                euler.y * jointManipulation.Sign.y + jointManipulation.Offset.y,
                euler.z * jointManipulation.Sign.z + jointManipulation.Offset.z);
            if (jointManipulation.SwapXAndZ)
            {
                euler = new Vector3(euler.z, euler.y, euler.x);
            }
        }

        return Quaternion.Euler(euler);
    }

    // Find the pattern and you win a prize.
    private void InitializeJointManipulationData()
    {
        jointManipulations = new Dictionary<Windows.Kinect.JointType, JointManipulation>();
        var jointManipulation = new JointManipulation();

        jointManipulation.Offset = new Vector3(0, 180, 90);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.WristRight, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, -90);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.WristLeft, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, 90);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.HandRight, jointManipulation);

        jointManipulation.Offset = new Vector3(180, 0, 90);
        jointManipulation.Sign = new Vector3(-1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.HandLeft, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 0, 90);
        jointManipulation.Sign = new Vector3(-1, -1, 1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.ElbowRight, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 0, -90);
        jointManipulation.Sign = new Vector3(-1, -1, 1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.ElbowLeft, jointManipulation);

        jointManipulation.Offset = new Vector3(180, 0, -90);
        jointManipulation.Sign = new Vector3(-1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.ShoulderRight, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, -90);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.ShoulderLeft, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, 0);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.SpineShoulder, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, 0);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.SpineMid, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, 0);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.Neck, jointManipulation);

        jointManipulation.Offset = new Vector3(0, -180, 0);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.SpineBase, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, 90);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.HipRight, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 180, -90);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = false;
        jointManipulations.Add(Windows.Kinect.JointType.HipLeft, jointManipulation);

        jointManipulation.Offset = new Vector3(0, 90, 180);
        jointManipulation.Sign = new Vector3(-1, -1, -1);
        jointManipulation.SwapXAndZ = true;
        jointManipulations.Add(Windows.Kinect.JointType.KneeRight, jointManipulation);

        jointManipulation.Offset = new Vector3(180, 90, 0);
        jointManipulation.Sign = new Vector3(1, -1, -1);
        jointManipulation.SwapXAndZ = true;
        jointManipulations.Add(Windows.Kinect.JointType.KneeLeft, jointManipulation);

        jointManipulation.Offset = new Vector3(180, -90, 0);
        jointManipulation.Sign = new Vector3(-1, -1, 1);
        jointManipulation.SwapXAndZ = true;
        jointManipulations.Add(Windows.Kinect.JointType.AnkleRight, jointManipulation);

        jointManipulation.Offset = new Vector3(0, -90, 180);
        jointManipulation.Sign = new Vector3(1, -1, 1);
        jointManipulation.SwapXAndZ = true;
        jointManipulations.Add(Windows.Kinect.JointType.AnkleLeft, jointManipulation);
    }

    // Used to unite the kinect joint orientations under a common coordinate system.
    private struct JointManipulation
    {
        public Vector3 Offset;
        public Vector3 Sign;
        public bool SwapXAndZ;
    };
}
