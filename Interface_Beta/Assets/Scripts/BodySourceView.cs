using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System;
using UnityEngine.UI;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public Text LeftAnkleText;
    public Text LeftKneeText;
    public Text LeftHipText;
    public Text RightAnkleText;
    public Text RightKneeText;
    public Text RightHipText;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private AnglesCalculation _anglesCalculation;

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

    void Update () 
    {
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

    //private double AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
    //{
    //    double dotProduct = 0.0;
    //    vectorA.Normalize();
    //    vectorB.Normalize();
    //    dotProduct = Vector3.Dot(vectorA, vectorB);

    //    return (double)Math.Acos(dotProduct) / Math.PI * 180;
    //}

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    #endregion
}
