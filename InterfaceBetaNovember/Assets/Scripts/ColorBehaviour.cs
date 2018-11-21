using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBehaviour : MonoBehaviour {

	//GameObject referenceKinect;
	GameObject referenceKinect;
	GameObject referenceColorGreen;
	GameObject referenceColorYellow;
	GameObject referenceColorRed;
	BodySourceView kinect;
    MeshRenderer renderGreen;
	MeshRenderer renderYellow;
	MeshRenderer renderRed;

	// Use this for initialization
	void Start () {

		// find the references of for kinect
		referenceKinect = GameObject.FindGameObjectWithTag("Kinect");
		kinect = referenceKinect.GetComponent<BodySourceView>();
		
		// access to the child objects and obtenin the MeshRender references
		referenceColorGreen = GameObject.FindGameObjectWithTag("Green");
		renderGreen = referenceColorGreen.GetComponent<MeshRenderer>();

		referenceColorYellow = GameObject.FindGameObjectWithTag("Yellow");
		renderYellow = referenceColorYellow.GetComponent<MeshRenderer>();

		referenceColorRed = GameObject.FindGameObjectWithTag("Red");
		renderRed = referenceColorRed.GetComponent<MeshRenderer>();

	}
	
	// Update is called once per frame
	// This will update green, yellow, red
	void Update () {
		if(kinect.colorActuator == 100){ 

			renderGreen.material.color = Color.green;
			renderYellow.material.color = Color.black;
			renderRed.material.color = Color.black;

		} else if(kinect.colorActuator == 1010){

			renderGreen.material.color = Color.black;
			renderYellow.material.color = Color.yellow;
			renderRed.material.color = Color.black;

		} else if(kinect.colorActuator == 1001){
			renderGreen.material.color = Color.black;
			renderYellow.material.color = Color.black;
			renderRed.material.color = Color.red;
		}
	}
}
