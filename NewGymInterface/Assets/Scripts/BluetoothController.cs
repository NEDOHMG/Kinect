using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class BluetoothController : MonoBehaviour
{

    SerialPort myData = new SerialPort("COM6", 115200);

    int currentState = 5;
    int previousState = 5;

    // Use this for initialization
    void Start()
    {
        // This will just print the devices connected in the port 
        foreach (string str in SerialPort.GetPortNames())
        {
            Debug.Log(string.Format("port : {0}", str));
        }

        myData.Open();

    }


    private void OnApplicationQuit()
    {
        myData.Close();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (myData.IsOpen)
        {
            currentState = KinectAvatar.sharedInstance.ExerciseState;


            if (currentState == 0 && currentState != previousState)
            {
                if (KinectAvatar.sharedInstance.resistanceMode == false)
                {
                    myData.WriteLine("1");
                } else
                {
                    myData.WriteLine("0");
                }
            }


            if (currentState == 2 && currentState != previousState)
            {
                if (KinectAvatar.sharedInstance.resistanceMode == false) { 
                    myData.WriteLine("0");
                } else
                {
                    myData.WriteLine("1");
                }
                
            }

            previousState = currentState;

        }
    }
}
