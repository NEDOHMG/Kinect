using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class BluetoothController : MonoBehaviour
{

    SerialPort myData = new SerialPort("COM4", 115200);

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
                //myData.Open(); //Open the serial stream
                //Debug.Log("Com open");
                myData.WriteLine("1");
                //Debug.Log("Data sent");
                //myData.Close();
                //Debug.Log("Com closed");
            }


            if (currentState == 2 && currentState != previousState)
            {
                //myData.Open(); //Open the serial stream
                //Debug.Log("Com open");
                myData.WriteLine("0");
                //Debug.Log("Data sent");
                //myData.Close();
                //Debug.Log("Com closed");
            }

            previousState = currentState;

        }
    }
}
