using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour {

    // Provide the color and depth data from Kinect
    public MultiSourceManager mMultiSource;

    // To put the output of the kinect
    public RawImage mRawImage;

	void Update ()
    {
        // Get the color data and display it in the canvas
        mRawImage.texture = mMultiSource.GetColorTexture();	
	}
}
