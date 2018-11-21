using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinect : MonoBehaviour {
	
	public int colorActuator = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.A)){
			// yellow
			colorActuator = 1010;
		} else if(Input.GetKey(KeyCode.S))
		{
			// rojo
			colorActuator = 1001;
		} else{
			// green
			colorActuator = 110;
		}
	}
}
