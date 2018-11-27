using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class KinectAvatar : MonoBehaviour
{

    public bool IsMirror = true;

    // Acces to the body manager
    public BodySourceManager _BodyManager;

    // Reference of the avatar 
    public GameObject _UnityChan;

    // Joints reference of the avatar 
    public GameObject Ref;
    public GameObject Hips;
    public GameObject LeftUpLeg;
    public GameObject LeftLeg;
    public GameObject RightUpLeg;
    public GameObject RightLeg;
    public GameObject Spine1;
    public GameObject Spine2;
    public GameObject LeftShoulder;
    public GameObject LeftArm;
    public GameObject LeftForeArm;
    public GameObject LeftHand;
    public GameObject RightShoulder;
    public GameObject RightArm;
    public GameObject RightForeArm;
    public GameObject RightHand;
    public GameObject Neck;
    public GameObject Head;

    // Quaternion used in the rotation of the joints
    Quaternion SpineBase;
    Quaternion SpineMid;
    Quaternion SpineShoulder;
    Quaternion ShoulderLeft;
    Quaternion ShoulderRight;
    Quaternion ElbowLeft;
    Quaternion WristLeft;
    Quaternion HandLeft;
    Quaternion ElbowRight;
    Quaternion WristRight;
    Quaternion HandRight;
    Quaternion KneeLeft;
    Quaternion AnkleLeft;
    Quaternion KneeRight;
    Quaternion AnkleRight;

    // References for the canvas 
    public Text LeftAnkleText;
    public Text LeftKneeText;
    public Text LeftHipText;
    public Text RightAnkleText;
    public Text RightKneeText;
    public Text RightHipText;

    // Vectors for the angles
    Vector3 _footLeft;
    Vector3 _ankleLeft;
    Vector3 _kneeLeft;
    Vector3 _hipLeft;

    // Center
    Vector3 _spine;

    // Right
    Vector3 _footRight;
    Vector3 _ankleRight;
    Vector3 _kneeRight;
    Vector3 _hipRight;

    // Object of angle calculation
    private AnglesCalculation _anglesCalculation;

    // Variables used to calculte the flexion and extension angles of knees and hip
    public double LeftKneeFlexionAngle;
    public double LeftKneeExtensionAngle;
    public double RightKneeFlexionAngle;
    public double RightKneeExtensionAngle;
    public double LeftHipFlexionAngle;
    public double RightHipFlexionAngle;
    public double LeftHipExtensionAngle;
    public double RightHipExtensionAngle;
    public double FlexionTime;
    public double ExtensionTime;


    void Awake()
    {
        // Initialize the vectors 
        // Left
        _footLeft = new Vector3(0.0f, 0.0f, 0.0f);
        _ankleLeft = new Vector3(0.0f, 0.0f, 0.0f);
        _kneeLeft = new Vector3(0.0f, 0.0f, 0.0f);
        _hipLeft = new Vector3(0.0f, 0.0f, 0.0f);

        // Center
        _spine = new Vector3(0.0f, 0.0f, 0.0f);

        // Right
        _footRight = new Vector3(0.0f, 0.0f, 0.0f);
        _ankleRight = new Vector3(0.0f, 0.0f, 0.0f);
        _kneeRight = new Vector3(0.0f, 0.0f, 0.0f);
        _hipRight = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Use this for initialization
    void Start()
    {
        _anglesCalculation = gameObject.GetComponent<AnglesCalculation>(); // Initialize the AnglesCalculation class

        // Initialize the objects of the avatar 
        Ref = _UnityChan.transform.Find("Character1_Reference").gameObject;
        Hips = Ref.gameObject.transform.Find("Character1_Hips").gameObject;
        LeftUpLeg = Hips.transform.Find("Character1_LeftUpLeg").gameObject;
        LeftLeg = LeftUpLeg.transform.Find("Character1_LeftLeg").gameObject;
        RightUpLeg = Hips.transform.Find("Character1_RightUpLeg").gameObject;
        RightLeg = RightUpLeg.transform.Find("Character1_RightLeg").gameObject;
        Spine1 = Hips.transform.Find("Character1_Spine").
        gameObject.transform.Find("Character1_Spine1").gameObject;
        Spine2 = Spine1.transform.Find("Character1_Spine2").gameObject;
        LeftShoulder = Spine2.transform.Find("Character1_LeftShoulder").gameObject;
        LeftArm = LeftShoulder.transform.Find("Character1_LeftArm").gameObject;
        LeftForeArm = LeftArm.transform.Find("Character1_LeftForeArm").gameObject;
        LeftHand = LeftForeArm.transform.Find("Character1_LeftHand").gameObject;
        RightShoulder = Spine2.transform.Find("Character1_RightShoulder").gameObject;
        RightArm = RightShoulder.transform.Find("Character1_RightArm").gameObject;
        RightForeArm = RightArm.transform.Find("Character1_RightForeArm").gameObject;
        RightHand = RightForeArm.transform.Find("Character1_RightHand").gameObject;
        Neck = Spine2.transform.Find("Character1_Neck").gameObject;
        Head = Neck.transform.Find("Character1_Head").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the component of body manager is located
        if (_BodyManager == null)
        {
            Debug.Log("BodyManager == null");
            return;
        }

        //  Track the user 
        Kinect.Body[] data = _BodyManager.GetData();

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

        // Get the information of the angles
        ShowAngles(body);
        // Apply the rotation in the joints of the avatar 
        CalculateRotationAvatar(body);
       
    }

    void CalculateRotationAvatar(Kinect.Body body)
    {

        // Obtein the plane 
        var floorPlane = _BodyManager.FloorClipPlane;
        var comp = Quaternion.FromToRotation(new Vector3(floorPlane.X, floorPlane.Y, floorPlane.Z), Vector3.up);

        // Hold the data of the orientation of the body joints 
        var joints = body.JointOrientations;

        // Check if the mirror is activated 
        if (IsMirror)
        {
            SpineBase = joints[Kinect.JointType.SpineBase].Orientation.ToMirror().ToQuaternion(comp);
            SpineMid = joints[Kinect.JointType.SpineMid].Orientation.ToMirror().ToQuaternion(comp);
            SpineShoulder = joints[Kinect.JointType.SpineShoulder].Orientation.ToMirror().ToQuaternion(comp);
            ShoulderLeft = joints[Kinect.JointType.ShoulderRight].Orientation.ToMirror().ToQuaternion(comp);
            ShoulderRight = joints[Kinect.JointType.ShoulderLeft].Orientation.ToMirror().ToQuaternion(comp);
            ElbowLeft = joints[Kinect.JointType.ElbowRight].Orientation.ToMirror().ToQuaternion(comp);
            WristLeft = joints[Kinect.JointType.WristRight].Orientation.ToMirror().ToQuaternion(comp);
            HandLeft = joints[Kinect.JointType.HandRight].Orientation.ToMirror().ToQuaternion(comp);
            ElbowRight = joints[Kinect.JointType.ElbowLeft].Orientation.ToMirror().ToQuaternion(comp);
            WristRight = joints[Kinect.JointType.WristLeft].Orientation.ToMirror().ToQuaternion(comp);
            HandRight = joints[Kinect.JointType.HandLeft].Orientation.ToMirror().ToQuaternion(comp);
            KneeLeft = joints[Kinect.JointType.KneeRight].Orientation.ToMirror().ToQuaternion(comp);
            AnkleLeft = joints[Kinect.JointType.AnkleRight].Orientation.ToMirror().ToQuaternion(comp);
            KneeRight = joints[Kinect.JointType.KneeLeft].Orientation.ToMirror().ToQuaternion(comp);
            AnkleRight = joints[Kinect.JointType.AnkleLeft].Orientation.ToMirror().ToQuaternion(comp);
        }
        // If not
        else
        {
            SpineBase = joints[Kinect.JointType.SpineBase].Orientation.ToQuaternion(comp);
            SpineMid = joints[Kinect.JointType.SpineMid].Orientation.ToQuaternion(comp);
            SpineShoulder = joints[Kinect.JointType.SpineShoulder].Orientation.ToQuaternion(comp);
            ShoulderLeft = joints[Kinect.JointType.ShoulderLeft].Orientation.ToQuaternion(comp);
            ShoulderRight = joints[Kinect.JointType.ShoulderRight].Orientation.ToQuaternion(comp);
            ElbowLeft = joints[Kinect.JointType.ElbowLeft].Orientation.ToQuaternion(comp);
            WristLeft = joints[Kinect.JointType.WristLeft].Orientation.ToQuaternion(comp);
            HandLeft = joints[Kinect.JointType.HandLeft].Orientation.ToQuaternion(comp);
            ElbowRight = joints[Kinect.JointType.ElbowRight].Orientation.ToQuaternion(comp);
            WristRight = joints[Kinect.JointType.WristRight].Orientation.ToQuaternion(comp);
            HandRight = joints[Kinect.JointType.HandRight].Orientation.ToQuaternion(comp);
            KneeLeft = joints[Kinect.JointType.KneeLeft].Orientation.ToQuaternion(comp);
            AnkleLeft = joints[Kinect.JointType.AnkleLeft].Orientation.ToQuaternion(comp);
            KneeRight = joints[Kinect.JointType.KneeRight].Orientation.ToQuaternion(comp);
            AnkleRight = joints[Kinect.JointType.AnkleRight].Orientation.ToQuaternion(comp);
        }

        // Calculate the rotation of the joints 
        var q = transform.rotation;
        transform.rotation = Quaternion.identity;

        var comp2 = Quaternion.AngleAxis(90, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));

        Spine1.transform.rotation = SpineMid * comp2;

        RightArm.transform.rotation = ElbowRight * comp2;
        RightForeArm.transform.rotation = WristRight * comp2;
        RightHand.transform.rotation = HandRight * comp2;

        LeftArm.transform.rotation = ElbowLeft * comp2;
        LeftForeArm.transform.rotation = WristLeft * comp2;
        LeftHand.transform.rotation = HandLeft * comp2;

        RightUpLeg.transform.rotation = KneeRight * comp2;
        RightLeg.transform.rotation = AnkleRight * comp2;

        LeftUpLeg.transform.rotation = KneeLeft * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));
        LeftLeg.transform.rotation = AnkleLeft * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));

        // Set the rotation of the model 
        transform.rotation = q;

        // Move the position of the joints 
        var pos = body.Joints[Kinect.JointType.SpineMid].Position;
        Ref.transform.position = new Vector3(-pos.X, pos.Y, -pos.Z);
    }

    void ShowAngles(Kinect.Body body)
    {

        // Left
        _footLeft = new Vector3(body.Joints[Kinect.JointType.FootLeft].Position.X, body.Joints[Kinect.JointType.FootLeft].Position.Y, body.Joints[Kinect.JointType.FootLeft].Position.Z);
        _ankleLeft = new Vector3(body.Joints[Kinect.JointType.AnkleLeft].Position.X, body.Joints[Kinect.JointType.AnkleLeft].Position.Y, body.Joints[Kinect.JointType.AnkleLeft].Position.Z);
        _kneeLeft = new Vector3(body.Joints[Kinect.JointType.KneeLeft].Position.X, body.Joints[Kinect.JointType.KneeLeft].Position.Y, body.Joints[Kinect.JointType.KneeLeft].Position.Z);
        _hipLeft = new Vector3(body.Joints[Kinect.JointType.HipLeft].Position.X, body.Joints[Kinect.JointType.HipLeft].Position.Y, body.Joints[Kinect.JointType.HipLeft].Position.Z);

        // Center
        _spine = new Vector3(body.Joints[Kinect.JointType.SpineBase].Position.X, body.Joints[Kinect.JointType.SpineBase].Position.Y, body.Joints[Kinect.JointType.SpineBase].Position.Z);

        // Right
        _footRight = new Vector3(body.Joints[Kinect.JointType.FootRight].Position.X, body.Joints[Kinect.JointType.FootRight].Position.Y, body.Joints[Kinect.JointType.FootRight].Position.Z);
        _ankleRight = new Vector3(body.Joints[Kinect.JointType.AnkleRight].Position.X, body.Joints[Kinect.JointType.AnkleRight].Position.Y, body.Joints[Kinect.JointType.AnkleRight].Position.Z);
        _kneeRight = new Vector3(body.Joints[Kinect.JointType.KneeRight].Position.X, body.Joints[Kinect.JointType.KneeRight].Position.Y, body.Joints[Kinect.JointType.KneeRight].Position.Z);
        _hipRight = new Vector3(body.Joints[Kinect.JointType.HipRight].Position.X, body.Joints[Kinect.JointType.HipRight].Position.Y, body.Joints[Kinect.JointType.HipRight].Position.Z);

        // Calculation of the angles
        float _ankleLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_ankleLeft - _kneeLeft, _ankleLeft - _footLeft);
        float _kneeLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_kneeLeft - _hipLeft, _kneeLeft - _ankleLeft);
        float _hipLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_hipLeft - _spine, _hipLeft - _kneeLeft);
        float _ankleRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_ankleRight - _kneeRight, _ankleRight - _footRight);
        float _kneeRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_kneeRight - _hipRight, _kneeRight - _ankleRight);
        float _hipRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_hipRight - _spine, _hipRight - _kneeRight);

        LeftAnkleText.text = "The left ankle angle is: " + _ankleLeftAngle.ToString();
        LeftKneeText.text = "The left knee angle is: " + _kneeLeftAngle.ToString();
        LeftHipText.text = "The left hip angle is: " + _hipLeftAngle.ToString();
        RightAnkleText.text = "The right ankle angle is: " + _ankleRightAngle.ToString();
        RightKneeText.text = "The right knee is: " + _kneeRightAngle.ToString();
        RightHipText.text = "The right hip angle is: " + _hipRightAngle.ToString();

    }

}