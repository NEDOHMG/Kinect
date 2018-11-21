using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System;
using System.Linq;
using UnityEngine.UI;

//test

public class BodySourceView : MonoBehaviour
{
    #region Variables

    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public Text LeftAnkleText;
    public Text LeftKneeText;
    public Text LeftHipText;
    public Text RightAnkleText;
    public Text RightKneeText;
    public Text RightHipText;

    //Variable used to determine the number of seconds we are tracking joint deltas
    public int TrackingWindow = 3;

    //Boolean set to true if the subject has stopped moving (average  of spine base y-delta list is less than threshold value)
    public bool Calibrated = false;
    public bool Stopped = false;

    //Subject state. 0 = stopped, extended. 1 = flexing. 2 = stopped, flexed. 3 = extending.
    public int ExerciseState = 0;
    public static Color StatusLightColor = Color.red;
    public int colorActuator = 1001;

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
    public double MotionTimer = 0.0;

    //fps variable stores frames per second, used for size of tracking and delta lists
    private double deltaTime = 0.0;
    public double fps = 0.0;

    //Threshold for determining stopped motion. Needs to be calibrated.
    public double threshold = 0.0;

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

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private AnglesCalculation _anglesCalculation;

    #endregion

    #region Joints

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    #endregion


    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    public double SpineBaseCurrentAverage = 0.0;

    // Note that this function is only meant to be called from OnGUI() functions.
    public static void GUIDrawRect( Rect position, Color color )
    {
        if( _staticRectTexture == null )
        {
            _staticRectTexture = new Texture2D( 1, 1 );
        }

        if( _staticRectStyle == null )
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel( 0, 0, color );
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box( position, GUIContent.none, _staticRectStyle );
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

    void start()
    {
        //threshold = calibrate();

        //initialize the knee lateral lists
        LeftKneeLateralPositions = new List<double>();
        RightKneeLateralPositions = new List<double>();
        LeftKneeLateralDeltas = new List<double>();
        RightKneeLateralDeltas = new List<double>();

        // Initialize the list for the Spin
        _spineList = new List<double>();
        _differencesSpinY = new List<double>();

    }

    void Update ()
    {
        //FPS calculation
        deltaTime += (double)Time.deltaTime;
        deltaTime /= 2.0;
        fps = 1.0/deltaTime;

        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }

            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);

                // Get the location of the joints in 3D space

                // Left
                Vector3 _footLeft = new Vector3(body.Joints[Kinect.JointType.FootLeft].Position.X, body.Joints[Kinect.JointType.FootLeft].Position.Y, body.Joints[Kinect.JointType.FootLeft].Position.Z);
                Vector3 _ankleLeft = new Vector3(body.Joints[Kinect.JointType.AnkleLeft].Position.X, body.Joints[Kinect.JointType.AnkleLeft].Position.Y, body.Joints[Kinect.JointType.AnkleLeft].Position.Z);
                Vector3 _kneeLeft = new Vector3(body.Joints[Kinect.JointType.KneeLeft].Position.X, body.Joints[Kinect.JointType.KneeLeft].Position.Y, body.Joints[Kinect.JointType.KneeLeft].Position.Z);
                Vector3 _hipLeft = new Vector3(body.Joints[Kinect.JointType.HipLeft].Position.X, body.Joints[Kinect.JointType.HipLeft].Position.Y, body.Joints[Kinect.JointType.HipLeft].Position.Z);

                // Center
                Vector3 _spine = new Vector3(body.Joints[Kinect.JointType.SpineBase].Position.X, body.Joints[Kinect.JointType.SpineBase].Position.Y, body.Joints[Kinect.JointType.SpineBase].Position.Z);

                // Right
                Vector3 _footRight = new Vector3(body.Joints[Kinect.JointType.FootRight].Position.X, body.Joints[Kinect.JointType.FootRight].Position.Y, body.Joints[Kinect.JointType.FootRight].Position.Z);
                Vector3 _ankleRight = new Vector3(body.Joints[Kinect.JointType.AnkleRight].Position.X, body.Joints[Kinect.JointType.AnkleRight].Position.Y, body.Joints[Kinect.JointType.AnkleRight].Position.Z);
                Vector3 _kneeRight = new Vector3(body.Joints[Kinect.JointType.KneeRight].Position.X, body.Joints[Kinect.JointType.KneeRight].Position.Y, body.Joints[Kinect.JointType.KneeRight].Position.Z);
                Vector3 _hipRightt = new Vector3(body.Joints[Kinect.JointType.HipRight].Position.X, body.Joints[Kinect.JointType.HipRight].Position.Y, body.Joints[Kinect.JointType.HipRight].Position.Z);

                _anglesCalculation = gameObject.GetComponent<AnglesCalculation>(); // Initialize the AnglesCalculation class

                // Vector required for the SpineBase joint
                _spinBase = new Vector3(body.Joints[Kinect.JointType.SpineBase].Position.X, body.Joints[Kinect.JointType.SpineBase].Position.Y, body.Joints[Kinect.JointType.SpineBase].Position.Z);

                // Calculation of the spin Y delta
                if(_spinTracking == true){

                  SpinDeltaYTracking(_spinBase);

                }

                // Emergency stop
                EmergencyStop(body);

                // Calculation of the angles
                double _ankleLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_ankleLeft - _kneeLeft, _ankleLeft - _footLeft);
                double _kneeLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_kneeLeft - _hipLeft, _kneeLeft - _ankleLeft);
                double _hipLeftAngle = _anglesCalculation.AngleBetweenTwoVectors(_hipLeft - _spine, _hipLeft - _kneeLeft);
                double _ankleRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_ankleRight - _kneeRight, _ankleRight - _footRight);
                double _kneeRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_kneeRight - _hipRightt, _kneeRight - _ankleRight);
                double _hipRightAngle = _anglesCalculation.AngleBetweenTwoVectors(_hipRightt - _spine, _hipRightt - _kneeRight);

                LeftAnkleText.text = "The left ankle angle is: " + _ankleLeftAngle.ToString();
                LeftKneeText.text = "The left knee angle is: " + _kneeLeftAngle.ToString();
                LeftHipText.text = "The left hip angle is: " + _hipLeftAngle.ToString();
                RightAnkleText.text = "The right ankle angle is: " + _ankleRightAngle.ToString();
                RightKneeText.text = "The right knee is: " + _kneeRightAngle.ToString();
                RightHipText.text = "The right hip angle is: " + _hipRightAngle.ToString();


                //calibrate the Threshold
                CalibrateThreshold(_spine.y);

                //track knee lateral motion
                TrackKneeLateralMotion(_kneeLeft.x, _kneeRight.x);

                //detect if Stopped
                DetectStop(_spine.y);

                //update the exercise state
                UpdateExerciseState(_kneeLeftAngle, _kneeRightAngle, _hipLeftAngle, _hipRightAngle);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }

    #region Events

    private double calibrate()
    {
    //  private double currenttime =
        return 0.05; //default value, change this when you implement the code
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        return body;
    }

    //Update the exercise state depending on the current state and whether or not the subject has stopped or started moving.
    private void UpdateExerciseState(double leftkneeangle, double rightkneeangle, double lefthipangle, double righthipangle)
    {

        MotionTimer += (double)Time.deltaTime;

        //if subject starts moving down, change state to 1
        if(Calibrated && !Stopped && ExerciseState == 0)
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
        else if(Stopped && ExerciseState == 1)
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
        else if(!Stopped && ExerciseState == 2)
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
        else if(Stopped && ExerciseState == 3)
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
        }

        if(!Calibrated)
        {
            StatusLightColor = Color.red;
            colorActuator = 1001;
        }
    }

    //Experimental Function to Calibrate the threshold variable
    private void CalibrateThreshold(double CurrentSpineBaseVertical)
    {
        if(Calibrated)
            return;

        //get the size of the tracking window
        if(StopWindowSize == 0)
        {
            StopWindowSize = (int)(fps * TrackingWindow);
            return;
        }
        int trackingsize = StopWindowSize;

        //add the new positions to the positions lists
        SpineBaseVerticalPositions.Add(CurrentSpineBaseVertical);

        //still need more values
        if(SpineBaseVerticalPositions.Count < trackingsize)
            return;

        //calculate our first delta list
        for(int i = 0; i < trackingsize - 1; i++)
        {
            SpineBaseVerticalDeltas.Add(SpineBaseVerticalPositions[i + 1] - SpineBaseVerticalPositions[i]);
        }

        //get the absolute value delta maximum
        double AverageDelta = 0.0;
        for(int i = 0; i < SpineBaseVerticalDeltas.Count; i++)
        {
            AverageDelta += Math.Abs(SpineBaseVerticalDeltas[i]);
        }
        AverageDelta = AverageDelta / SpineBaseVerticalDeltas.Count;

        //set Threshold
        threshold = AverageDelta;

        //calibrated
        Calibrated = true;
        StatusLightColor = Color.green;


    }

    //Experimental function to potentially detect when a subject has stopped motion
    private void DetectStop(double CurrentSpineBaseVertical)
    {
        if(!Calibrated)
            return;

        //get the size of the tracking window
        if(StopWindowSize == 0)
        {
            StopWindowSize = (int)(fps * TrackingWindow);
            return;
        }
        int trackingsize = StopWindowSize;

        //add the new positions to the positions lists
        SpineBaseVerticalPositions.Add(CurrentSpineBaseVertical);

        //still need more values
        if(SpineBaseVerticalPositions.Count < trackingsize)
            return;
        else if(SpineBaseVerticalPositions.Count > trackingsize)
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
            for(int i = 0; i < SpineBaseVerticalDeltas.Count; i++)
            {
                AverageDelta += Math.Abs(SpineBaseVerticalDeltas[i]);
            }
            AverageDelta = AverageDelta / SpineBaseVerticalDeltas.Count;

            SpineBaseCurrentAverage = AverageDelta;

            //    SpineBaseAverageDelta = Math.Abs(SpineBaseCurrentAverage);

            //update stopped state
            if(Math.Abs(SpineBaseCurrentAverage) <= Math.Abs(threshold) && !Stopped)
            {
                Stopped = true;
            }
            else if(Math.Abs(SpineBaseCurrentAverage) > Math.Abs(threshold) && Stopped)
            {
                Stopped = false;
            }

        }
        else
        {
            //calculate our first delta list
            for(int i = 0; i < trackingsize - 1; i++)
            {
                SpineBaseVerticalDeltas.Add(SpineBaseVerticalPositions[i + 1] - SpineBaseVerticalPositions[i]);
            }
        }

    }

    private void TrackKneeLateralMotion(double CurrentLeftKneeLateral, double CurrentRightKneeLateral)
    {

        //get the size of the tracking window
        if(LateralsWindowSize == 0)
        {
            LateralsWindowSize = (int)(fps * TrackingWindow);
            return;
        }
        int trackingsize = LateralsWindowSize;

        //add the new positions to the positions lists
        LeftKneeLateralPositions.Add(CurrentLeftKneeLateral);
        RightKneeLateralPositions.Add(CurrentRightKneeLateral);

        //still need more values
        if(LeftKneeLateralPositions.Count < trackingsize)
            return;
        else if(LeftKneeLateralPositions.Count > trackingsize)
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
            if(Math.Abs(LeftKneeCurrentAverage) > LeftKneeAverageDelta)
                LeftKneeAverageDelta = Math.Abs(LeftKneeCurrentAverage);
            if(Math.Abs(RightKneeCurrentAverage) > RightKneeAverageDelta)
                RightKneeAverageDelta = Math.Abs(RightKneeCurrentAverage);
        }
        else
        {

            //calculate our first delta list for knees
            for(int i = 0; i < trackingsize - 1; i++)
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
        for(int i = 0; i < _spineList.Count - 1; i++)
        {
            _differencesSpinY.Add((_spineList[i+1] - _spineList[i]));
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
      if (_spinTrackingMin == true && Stopped) {

        _ySpinTimerDownStop = _ySpinTimerStart;
        _ySpinTimerStart = 0;
        _ySpinTimerStart += Time.deltaTime;
        // Add the y spin values in the list
        _spineList.Add(_spinBase.y);

      }

      // Calculate the delta of the min value
      for(int i = 0; i < _spineList.Count - 1; i++)
      {
          _differencesSpinY.Add((_spineList[i+1] - _spineList[i]));
      }

      // Calculate the average of the min delta
      _differencesSpinYMinAverage = _differencesSpinY.Average();
      _spinTrackingMin = false;

      // Calculate the time to achieve the normal posture again
      if (Stopped){

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

      if(_jointHandL.y == _jointHead.y)
      {
        Destroy(gameObject);
      }
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }

            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }

    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    #endregion
}
