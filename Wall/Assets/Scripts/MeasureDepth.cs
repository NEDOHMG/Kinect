using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour
{

    public MultiSourceManager mMultiSource;
    public Texture2D mDepthTexture;

    private ushort[] mDepthData = null;
    // Save the 3D points that the camera is reading
    private CameraSpacePoint[] mCameraSpacePoints = null;
    // Transform the 3D point to 2D as pixels
    private ColorSpacePoint[] mColorSpacePoints = null;

    private KinectSensor mSensor = null;
    private CoordinateMapper mMapper = null;

    private readonly Vector2Int mDepthResolution = new Vector2Int(512, 424);

    private void Awake()
    {
        mSensor = KinectSensor.GetDefault();
        mMapper = mSensor.CoordinateMapper;

        int arraySize = mDepthResolution.x * mDepthResolution.y;

        mCameraSpacePoints = new CameraSpacePoint[arraySize];
        mColorSpacePoints = new ColorSpacePoint[arraySize];
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DepthToColor();

            mDepthTexture = CreateTexture();
        }
    }

    private void DepthToColor()
    {
        // Take the depth data
        mDepthData = mMultiSource.GetDepthData();

        // Map
        mMapper.MapDepthFrameToCameraSpace(mDepthData, mCameraSpacePoints);
        mMapper.MapDepthFrameToColorSpace(mDepthData, mColorSpacePoints);

        // Filter
    }

    private Texture2D CreateTexture()
    {
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false);

        for(int x = 0; x < 1980; x++)
        {
            for (int y = 0; y < 1080; y++)
            {
                // All the pixels in the texture will clear (transparent)
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        foreach(ColorSpacePoint point in mColorSpacePoints)
        {
            newTexture.SetPixel((int)point.X, (int)point.Y, Color.black);
        }

        newTexture.Apply();

        return newTexture;
    }

}
