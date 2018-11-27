using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using UnityEditor;
using System.IO;

public class KinectAvatar : MonoBehaviour
{
    #region

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
    //public Text LeftAnkleText;
    public Text LeftKneeText;
    public Text LeftHipText;
    //public Text RightAnkleText;
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

    // Variables used to hold the value of the angles
    float _kneeLeftAngle = 0.0f;
    float _hipLeftAngle = 0.0f;
    float _kneeRightAngle = 0.0f;
    float _hipRightAngle = 0.0f;

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

    //Variable used to determine the number of seconds we are tracking joint deltas
    public int TrackingWindow = 3;

    //Boolean set to true if the subject has stopped moving (average  of spine base y-delta list is less than threshold value)
    public bool Calibrated = false;
    public bool Stopped = false;

    //Subject state. 0 = stopped, extended. 1 = flexing. 2 = stopped, flexed. 3 = extending.
    public int ExerciseState = 0;
    public static Color StatusLightColor = Color.red;
    public int colorActuator = 1001;

    //fps variable stores frames per second, used for size of tracking and delta lists
    private double deltaTime = 0.0;
    public double fps = 0.0;
    public double MotionTimer = 0.0;

    //Threshold for determining stopped motion. Needs to be calibrated.
    public double threshold = 0.0;
    public double emergencythreshold = 0.3;
    public int emergencycounter = 0;

    // Variables used to save the Y values of the SpineBase
    Vector3 _spinBase;
    private List<double> _spineList;
    private List<double> _differencesSpinY;
    private double _differencesSpinYMaxAverage;
    private double _differencesSpinYMinAverage;
    private bool _spinTracking = false;
    private bool _spinTrackingMax = true;
    private bool _spinTrackingMin = false;

    // Variables to detect the time of execute the exercise
    private float _ySpinTimerStart = 0f;
    private float _ySpinTimerDownStop = 0f;
    private float _ySpinTimerHighStop = 0f;

    // Stop engine
    private Vector3 _jointHandL;
    private Vector3 _jointHead;

    //Variables for calculating threshold during start
    public int calTrackingWindow = 3;
    public List<double> _calspineList;
    public List<double> _deltacalspineY;
    private double _caldeltaAVG = 0.0;

    //Variables used to calculate knee lateral inputs
    public int LateralsWindowSize = 0;
    public List<double> LeftKneeLateralPositions;
    public List<double> LeftKneeLateralDeltas;
    public double LeftKneeAverageDelta = 0.0;
    public List<double> RightKneeLateralPositions;
    public List<double> RightKneeLateralDeltas;
    public double RightKneeAverageDelta = 0.0;

    //Variables for experimental motion stop detection
    public int StopWindowSize = 0;
    public List<double> SpineBaseVerticalPositions;
    public List<double> SpineBaseVerticalDeltas;
    public double SpineBaseAverageDelta = 0.0;

    //Variables for output to log file
    public static string preamble = "";
    public static string filename = "";
    public static string logfile = "";

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    public double SpineBaseCurrentAverage = 0.0;

    #endregion

    // Note that this function is only meant to be called from OnGUI() functions.
    public static void GUIDrawRect(Rect position, Color color)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(position, GUIContent.none, _staticRectStyle);
    }

    void OnGUI()
    {
        GUIDrawRect(new Rect(20, 220, 100, 100), StatusLightColor);
        GUI.Label(new Rect(20, 340, 100, 20), "Status: " + ExerciseState);
        GUI.Label(new Rect(20, 360, 100, 20), "Stopped: " + Stopped);
        GUI.Label(new Rect(20, 380, 100, 20), "Calibrated: " + Calibrated);
        GUI.Label(new Rect(20, 400, 900, 20), "Spine Base Average Delta: " + SpineBaseCurrentAverage.ToString());
        GUI.Label(new Rect(20, 420, 900, 20), "Threshold: " + threshold.ToString());
    }

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

        //initialize the knee lateral lists
        LeftKneeLateralPositions = new List<double>();
        RightKneeLateralPositions = new List<double>();
        LeftKneeLateralDeltas = new List<double>();
        RightKneeLateralDeltas = new List<double>();

        // Initialize the list for the Spin
        _spineList = new List<double>();
        _differencesSpinY = new List<double>();
        _calspineList = new List<double>();

        //initialize log file name based on current date and time
        DateTime dt = DateTime.Now;
        preamble = "Assets/Resources/";
        filename = "log_" + dt.ToString("yyyy-MM-dd_HH-mm-ss");
        logfile = preamble + filename + ".txt";

    }

    //write an entry to the log file
    static void WriteLog(string entry)
    {
        if (!File.Exists(logfile))
        {
            StreamWriter testing = File.CreateText(logfile);
            testing.Close();
        }
        //Write some text to the test.txt file
        StreamWriter writer = File.AppendText(logfile);
        writer.WriteLine(entry);
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(logfile);
        TextAsset asset = Resources.Load(filename) as TextAsset;

        //Print the text from the file
        //Debug.Log(asset.text);
    }

    // Update is called once per frame
    void Update()
    {
        //FPS calculation
        deltaTime += (double)Time.deltaTime;
        deltaTime /= 2.0;
        fps = 1.0 / deltaTime;

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
        // Emergency stop
        //EmergencyStop(body);

        //calibrate the Threshold
        //CalibrateThreshold(_spine.y);
        calibrate(_spinBase.y);

        //track knee lateral motion
        TrackKneeLateralMotion(_kneeLeft.x, _kneeRight.x);

        //detect if Stopped
        DetectStop(_spine.y);

        //update the exercise state
        UpdateExerciseState(_kneeLeftAngle, _kneeRightAngle, _hipLeftAngle, _hipRightAngle);

    }

    #region Events

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
        //float _ankleLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_ankleLeft - _kneeLeft, _ankleLeft - _footLeft);
        _kneeLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_kneeLeft - _hipLeft, _kneeLeft - _ankleLeft);
        _hipLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_hipLeft - _spine, _hipLeft - _kneeLeft);
        //float _ankleRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_ankleRight - _kneeRight, _ankleRight - _footRight);
        _kneeRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_kneeRight - _hipRight, _kneeRight - _ankleRight);
        _hipRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_hipRight - _spine, _hipRight - _kneeRight);

        //LeftAnkleText.text = "The left ankle angle is: " + _ankleLeftAngle.ToString();
        LeftKneeText.text = "The left knee angle is: " + _kneeLeftAngle.ToString();
        LeftHipText.text = "The left hip angle is: " + _hipLeftAngle.ToString();
        //RightAnkleText.text = "The right ankle angle is: " + _ankleRightAngle.ToString();
        RightKneeText.text = "The right knee is: " + _kneeRightAngle.ToString();
        RightHipText.text = "The right hip angle is: " + _hipRightAngle.ToString();

    }

    //Experimental Function to Calibrate the threshold variable
    private void CalibrateThreshold(double CurrentSpineBaseVertical)
    {
        if (Calibrated)
            return;

        //get the size of the tracking window
        if (StopWindowSize == 0)
        {
            StopWindowSize = (int)(fps * TrackingWindow);
            return;
        }
        int trackingsize = StopWindowSize;

        //add the new positions to the positions lists
        SpineBaseVerticalPositions.Add(CurrentSpineBaseVertical);

        //still need more values
        if (SpineBaseVerticalPositions.Count < trackingsize)
            return;

        //calculate our first delta list
        for (int i = 0; i < trackingsize - 1; i++)
        {
            SpineBaseVerticalDeltas.Add(SpineBaseVerticalPositions[i + 1] - SpineBaseVerticalPositions[i]);
        }

        //get the absolute value delta maximum
        double AverageDelta = 0.0;
        for (int i = 0; i < SpineBaseVerticalDeltas.Count; i++)
        {
            AverageDelta += Math.Abs(SpineBaseVerticalDeltas[i]);
        }
        AverageDelta = AverageDelta / SpineBaseVerticalDeltas.Count;

        //set Threshold
        threshold = AverageDelta;

        //calibrated
        Calibrated = true;
        StatusLightColor = Color.green;
        colorActuator = 100;


    }

    private void calibrate(double currentSpinePoint)
    {
        if (Calibrated == true)
        {
            //Console.WriteLine("Already calibrated!!");
            return;
        }

        else if (Calibrated == false)
        {
            //calculate the number of samples for the tracking time (3 seconds in this case)
            if (StopWindowSize == 0)
            {
                StopWindowSize = (int)(fps * calTrackingWindow);
                return;
            }
            int trackingtime = StopWindowSize;

            //add newest data point to our list
            _calspineList.Add(currentSpinePoint);

            //if we dont have 3 seconds of values yet, stop here
            if (_calspineList.Count < trackingtime)
                return;


            //get the delta values
            for (int i = 0; i < trackingtime - 1; i++)
            {
                _deltacalspineY.Add(_calspineList[i + 1] - _calspineList[i]);
            }

            //calculate the average of the delta values
            double temp = 0.0;
            for (int i = 0; i < _deltacalspineY.Count; i++)
            {
                temp += Math.Abs(_deltacalspineY[i]); //if needed, the value must be rounded off
            }
            _caldeltaAVG = temp / _deltacalspineY.Count;

            //Threshold value
            threshold = Math.Abs(_caldeltaAVG);
            Calibrated = true;
            StatusLightColor = Color.green;
            colorActuator = 100;
        }
    }

    //Update the exercise state depending on the current state and whether or not the subject has stopped or started moving.
    private void UpdateExerciseState(double leftkneeangle, double rightkneeangle, double lefthipangle, double righthipangle)
    {

        MotionTimer += (double)Time.deltaTime;

        //if subject starts moving down, change state to 1
        if (Calibrated && !Stopped && ExerciseState == 0)
        {
            ExerciseState = 1;

            //reset the lateral variables
            LeftKneeAverageDelta = 0.0;
            RightKneeAverageDelta = 0.0;
            LeftKneeLateralPositions = new List<double>();
            RightKneeLateralPositions = new List<double>();
            LeftKneeLateralDeltas = new List<double>();
            RightKneeLateralDeltas = new List<double>();

            //reset the motion timer
            MotionTimer = 0.0;

            StatusLightColor = Color.yellow;
            colorActuator = 1010;
        }
        //if subject stops moving down, change state to 2
        else if (Stopped && ExerciseState == 1)
        {
            ExerciseState = 2;

            LeftKneeFlexionAngle = leftkneeangle;
            RightKneeFlexionAngle = rightkneeangle;
            LeftHipFlexionAngle = lefthipangle;
            RightHipFlexionAngle = righthipangle;
            FlexionTime = MotionTimer;

            StatusLightColor = Color.green;
            colorActuator = 100;
        }
        //if subject starts moving up, change state to 3
        else if (!Stopped && ExerciseState == 2)
        {
            ExerciseState = 3;

            //reset the lateral variables
            LeftKneeAverageDelta = 0.0;
            RightKneeAverageDelta = 0.0;
            LeftKneeLateralPositions = new List<double>();
            RightKneeLateralPositions = new List<double>();
            LeftKneeLateralDeltas = new List<double>();
            RightKneeLateralDeltas = new List<double>();

            //reset the motion timer
            MotionTimer = 0.0;

            StatusLightColor = Color.yellow;
            colorActuator = 1010;
        }
        //if subject stops moving up, change state to 4
        else if (Stopped && ExerciseState == 3)
        {
            ExerciseState = 0;

            LeftKneeExtensionAngle = leftkneeangle;
            RightKneeExtensionAngle = rightkneeangle;
            LeftHipExtensionAngle = lefthipangle;
            RightHipExtensionAngle = righthipangle;
            ExtensionTime = MotionTimer;

            StatusLightColor = Color.green;
            colorActuator = 100;

            //upload current values to neural network here
            WriteLog(LeftKneeFlexionAngle + " " + LeftKneeExtensionAngle + " " + RightKneeFlexionAngle + " " + RightKneeExtensionAngle + " " + LeftHipFlexionAngle + " " + LeftHipExtensionAngle + " " + RightHipFlexionAngle + " " + RightHipExtensionAngle + " " + LeftKneeAverageDelta + " " + RightKneeAverageDelta + " " + SpineBaseAverageDelta + " " + FlexionTime + " " + ExtensionTime);
        }

        if (!Calibrated)
        {
            StatusLightColor = Color.red;
            colorActuator = 1001;
        }
    }

    //Experimental function to potentially detect when a subject has stopped motion
    private void DetectStop(double CurrentSpineBaseVertical)
    {
        if (!Calibrated)
            return;

        //get the size of the tracking window
        if (StopWindowSize == 0)
        {
            StopWindowSize = (int)(fps * TrackingWindow);
            return;
        }
        int trackingsize = StopWindowSize;

        //add the new positions to the positions lists
        SpineBaseVerticalPositions.Add(CurrentSpineBaseVertical);

        //still need more values
        if (SpineBaseVerticalPositions.Count < trackingsize)
            return;
        else if (SpineBaseVerticalPositions.Count > trackingsize)
        {

            //update the positions lists
            SpineBaseVerticalPositions.RemoveAt(0);

            //update the deltas lists
            SpineBaseVerticalDeltas.RemoveAt(0);
            SpineBaseVerticalDeltas.Add(SpineBaseVerticalPositions[trackingsize - 1] - SpineBaseVerticalPositions[trackingsize - 2]);

            //get the current average
            //SpineBaseCurrentAverage = SpineBaseVerticalDeltas.Average();

            SpineBaseCurrentAverage = 0;

            //if(Math.Abs(SpineBaseCurrentAverage) > SpineBaseAverageDelta)

            double AverageDelta = 0.0;
            for (int i = 0; i < SpineBaseVerticalDeltas.Count; i++)
            {
                AverageDelta += Math.Abs(SpineBaseVerticalDeltas[i]);
            }
            AverageDelta = AverageDelta / SpineBaseVerticalDeltas.Count;

            SpineBaseCurrentAverage = AverageDelta;

            //    SpineBaseAverageDelta = Math.Abs(SpineBaseCurrentAverage);

            //update stopped state
            if (Math.Abs(SpineBaseCurrentAverage) <= Math.Abs(threshold) && !Stopped)
            {
                Stopped = true;
            }
            else if (Math.Abs(SpineBaseCurrentAverage) > Math.Abs(threshold) && Stopped)
            {
                Stopped = false;
            }

        }
        else
        {
            //calculate our first delta list
            for (int i = 0; i < trackingsize - 1; i++)
            {
                SpineBaseVerticalDeltas.Add(SpineBaseVerticalPositions[i + 1] - SpineBaseVerticalPositions[i]);
            }
        }

    }

    private void TrackKneeLateralMotion(double CurrentLeftKneeLateral, double CurrentRightKneeLateral)
    {

        //get the size of the tracking window
        if (LateralsWindowSize == 0)
        {
            LateralsWindowSize = (int)(fps * TrackingWindow);
            return;
        }
        int trackingsize = LateralsWindowSize;

        //add the new positions to the positions lists
        LeftKneeLateralPositions.Add(CurrentLeftKneeLateral);
        RightKneeLateralPositions.Add(CurrentRightKneeLateral);

        //still need more values
        if (LeftKneeLateralPositions.Count < trackingsize)
            return;
        else if (LeftKneeLateralPositions.Count > trackingsize)
        {
            //update the positions lists
            LeftKneeLateralPositions.RemoveAt(0);
            RightKneeLateralPositions.RemoveAt(0);

            //update the deltas lists
            LeftKneeLateralDeltas.RemoveAt(0);
            RightKneeLateralDeltas.RemoveAt(0);
            LeftKneeLateralDeltas.Add(LeftKneeLateralPositions[trackingsize - 1] - LeftKneeLateralPositions[trackingsize - 2]);
            RightKneeLateralDeltas.Add(RightKneeLateralPositions[trackingsize - 1] - RightKneeLateralPositions[trackingsize - 2]);

            //get the delta averages
            double LeftKneeCurrentAverage = LeftKneeLateralDeltas.Average();
            double RightKneeCurrentAverage = RightKneeLateralDeltas.Average();

            //update the deltas
            if (Math.Abs(LeftKneeCurrentAverage) > LeftKneeAverageDelta)
                LeftKneeAverageDelta = Math.Abs(LeftKneeCurrentAverage);
            if (Math.Abs(RightKneeCurrentAverage) > RightKneeAverageDelta)
                RightKneeAverageDelta = Math.Abs(RightKneeCurrentAverage);
        }
        else
        {

            //calculate our first delta list for knees
            for (int i = 0; i < trackingsize - 1; i++)
            {
                LeftKneeLateralDeltas.Add(LeftKneeLateralPositions[i + 1] - LeftKneeLateralPositions[i]);
                RightKneeLateralDeltas.Add(RightKneeLateralPositions[i + 1] - RightKneeLateralPositions[i]);
            }

        }


    }

    private void SpinDeltaYTracking(Vector3 _spinBase)
    {

        // Calculate the max for 3 seconds
        if (_spinTrackingMax == true && fps < TrackingWindow)
        {

            // Add the y spin values in the list
            _spineList.Add(_spinBase.y);
            // Calculate the deltas
            for (int i = 0; i < _spineList.Count - 1; i++)
            {
                _differencesSpinY.Add((_spineList[i + 1] - _spineList[i]));
            }
            // This variable has the delta average of the spin user
            _differencesSpinYMaxAverage = _differencesSpinY.Average();
            // Activate the flag to track the minimum
            _spinTrackingMin = true;
            // refresh the list to use them again
            _spineList.Clear();
            _differencesSpinY.Clear();
            _spinTrackingMax = false;

        }

        // Start the time to detect if the user is moving here
        // initialize the timer of squat exercise
        _ySpinTimerStart += Time.deltaTime;

        // Calculate the min
        if (_spinTrackingMin == true && Stopped)
        {

            _ySpinTimerDownStop = _ySpinTimerStart;
            _ySpinTimerStart = 0;
            _ySpinTimerStart += Time.deltaTime;
            // Add the y spin values in the list
            _spineList.Add(_spinBase.y);

        }

        // Calculate the delta of the min value
        for (int i = 0; i < _spineList.Count - 1; i++)
        {
            _differencesSpinY.Add((_spineList[i + 1] - _spineList[i]));
        }

        // Calculate the average of the min delta
        _differencesSpinYMinAverage = _differencesSpinY.Average();
        _spinTrackingMin = false;

        // Calculate the time to achieve the normal posture again
        if (Stopped)
        {

            _ySpinTimerHighStop = _ySpinTimerStart;
            _ySpinTimerStart = 0;
            _spinTracking = false;
            _spineList.Clear();
            _differencesSpinY.Clear();
        }
    }

    private void EmergencyStop(Kinect.Body body)
    {
        _jointHandL = new Vector3(body.Joints[Kinect.JointType.HandLeft].Position.X, body.Joints[Kinect.JointType.HandLeft].Position.Y, body.Joints[Kinect.JointType.HandLeft].Position.Z);
        _jointHead = new Vector3(body.Joints[Kinect.JointType.Head].Position.X, body.Joints[Kinect.JointType.Head].Position.Y, body.Joints[Kinect.JointType.Head].Position.Z);

        float w = Math.Abs(_jointHandL.y - _jointHead.y);
        //Debug.Log("This is the abs value " + w + "and this is the threshold " + emergencythreshold);

        if (Math.Abs(_jointHandL.y - _jointHead.y) < emergencythreshold)
        {
            emergencycounter++;
            if (emergencycounter > 200)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            {
                emergencycounter = 0;
            }
        }
    }

    #endregion
}