using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LampColor : MonoBehaviour {

	// References to find the objects
	GameObject referenceKinect;
    
    // Object to access the kinect  
	BodySourceView kinect;

    // Objects to render differently in the light
    Renderer _color;

    // Use this for initialization
    void Start () {

		// find the references of for kinect
		referenceKinect = GameObject.FindGameObjectWithTag("Kinect");
		kinect = referenceKinect.GetComponent<BodySourceView>();

        // access to the child objects and obtenin the MeshRender references
        _color = GetComponent<Renderer>();

        //_color.material.SetColor("_SpecColor", Color.red);

        //Set the main Color of the Material to green
        _color.material.shader = Shader.Find("_Color");
        _color.material.SetColor("_Color", Color.green);

        //Find the Specular shader and change its Color to red
        _color.material.shader = Shader.Find("Specular");
        _color.material.SetColor("_SpecColor", Color.green);

    }
	
	// Update is called once per frame
	// This will update green, yellow, red
	void Update () {

		if(kinect.colorActuator == 100){

            //_color.material.color = Color.green;
            _color.material.SetColor("_Color", Color.green);
            _color.material.SetColor("_SpecColor", Color.green);

        } else if(kinect.colorActuator == 1010){

            //_color.material.color = Color.yellow;
            _color.material.SetColor("_Color", Color.yellow);
            _color.material.SetColor("_SpecColor", Color.yellow);

        } else if(kinect.colorActuator == 1001){

            //_color.material.color = Color.red;
            _color.material.SetColor("_Color", Color.red);
            _color.material.SetColor("_SpecColor", Color.red);

        }
	}
}
