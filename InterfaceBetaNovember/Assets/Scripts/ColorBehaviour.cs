using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBehaviour : MonoBehaviour {

	// References to find the objects
	GameObject referenceKinect;
	GameObject referenceColorGreen;
	GameObject referenceColorYellow;
	GameObject referenceColorRed;
    
    // Object to access the kinect  
	BodySourceView kinect;

    // Objects to render differently in the light
    Renderer renderGreen;
    Renderer renderYellow;
	Renderer renderRed;

    // Use this for initialization
    void Start () {

		// find the references of for kinect
		referenceKinect = GameObject.FindGameObjectWithTag("Kinect");
		kinect = referenceKinect.GetComponent<BodySourceView>();
		
		// access to the child objects and obtenin the MeshRender references
		referenceColorGreen = GameObject.FindGameObjectWithTag("Green");
		renderGreen = referenceColorGreen.GetComponent<Renderer>();

		referenceColorYellow = GameObject.FindGameObjectWithTag("Yellow");
		renderYellow = referenceColorYellow.GetComponent<Renderer>();

		referenceColorRed = GameObject.FindGameObjectWithTag("Red");
		renderRed = referenceColorRed.GetComponent<Renderer>();

        renderGreen.material.shader = Shader.Find("_Color");
        renderGreen.material.SetColor("_Color", Color.black);
        renderGreen.material.shader = Shader.Find("Specular");
        renderGreen.material.SetColor("_SpecColor", Color.green);

        renderYellow.material.shader = Shader.Find("_Color");
        renderYellow.material.SetColor("_Color", Color.black);
        renderYellow.material.shader = Shader.Find("Specular");
        renderYellow.material.SetColor("_SpecColor", Color.yellow);

        renderRed.material.shader = Shader.Find("_Color");
        renderRed.material.SetColor("_Color", Color.black);
        renderRed.material.shader = Shader.Find("Specular");
        renderRed.material.SetColor("_SpecColor", Color.red);

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
