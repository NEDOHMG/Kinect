using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class LedDebug : MonoBehaviour {

    // Change depending of the behaviour of the COM 
    SerialPort myData = new SerialPort("COM5", 115200);

    int inputUser = 5;

	// Use this for initialization
	void Start () {
		
        foreach(string str in SerialPort.GetPortNames())
        {
            Debug.Log(string.Format("port : {0}", str));
        }

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.A))
        {
            myData.Open(); //Open the serial stream
            Debug.Log("Com open");
            myData.WriteLine("1");
            Debug.Log("Data sent");
            myData.Close();
            Debug.Log("Com closed");
            inputUser = 5; // clean the variable

        } else if (Input.GetKeyDown(KeyCode.B))
        {
            myData.Open(); //Open the serial stream
            Debug.Log("Com open");
            myData.WriteLine("0");
            Debug.Log("Data sent");
            myData.Close();
            Debug.Log("Com closed");
            inputUser = 5;
        }

    }

}
