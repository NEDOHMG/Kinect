using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class HandBehaviour : MonoBehaviour {

    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;

    private Transform _hand;

    // Use this for initialization
    void Start()
    {
        _hand = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
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

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                //Debug.Log("Is tracking");
                var pos = body.Joints[Kinect.JointType.HandRight].Position;
                var orientation = body.JointOrientations[Kinect.JointType.HandRight].Orientation;
                _hand.position = new Vector3(pos.X * 2, pos.Y * 2, pos.Z * 2);
                _hand.rotation = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W) * Quaternion.AngleAxis(90, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));

                //break;
            }
        }

    }
}
