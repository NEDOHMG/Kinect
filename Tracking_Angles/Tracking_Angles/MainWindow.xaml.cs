using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
// Kinect dll
using Microsoft.Kinect;
using System;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Threading;

namespace Tracking_Angles
{
    public enum Mode
    {
        Color,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members 

        // Default mode 
        public Mode _mode = Mode.Color;

        // Create and object of the Kinect class
        KinectSensor _sensor;
        // Create and object to read the incoming frames
        MultiSourceFrameReader _reader;
        // Create a list to save the ID of the tracked bodys
        // IList<Body> _bodies;
        Body[] _bodies;

        bool _displayBody = false;
        private int activeBodyIndex;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        // This window will be always updating in the .xaml 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();
                // Read the streams
                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Text block variables
            string angleLeftAnkle = "Null";
            string angleLeftKnee = "Null";
            string angleLeftHip = "Null";
            string angleRightAnkle = "Null";
            string angleRightKnee = "Null";
            string angleRightHip = "Null";


            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {

                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    // Check activeBodyIndex is still active
                    if (activeBodyIndex != -1)
                    {
                        Body body_Index = _bodies[activeBodyIndex];
                        if (!body_Index.IsTracked)
                        {
                            activeBodyIndex = -1;
                        }
                    }

                    // Get new activeBodyIndex if it's not currently tracked
                    if (activeBodyIndex == -1)
                    {
                        for (int i = 0; i < _bodies.Length; i++)
                        {
                            Body body_index = _bodies[i];
                            if (body_index.IsTracked)
                            {
                                activeBodyIndex = i;
                                // No need to continue loop
                                break;
                            }
                        }
                    }

                    // If active body is still active after checking and 
                    // updating, use it
                    if (activeBodyIndex != -1)
                    {
                        Body body_index = _bodies[activeBodyIndex];
                        // Do stuff with known active body.
                        foreach (var body in _bodies)
                        {
                            if (body != null)
                            {
                                if (body.IsTracked)
                                {
                                    // Left vectors side
                                    Vector3D FootLeft = new Vector3D(body.Joints[JointType.FootLeft].Position.X, body.Joints[JointType.FootLeft].Position.Y, body.Joints[JointType.FootLeft].Position.Z);
                                    Vector3D AnkleLeft = new Vector3D(body.Joints[JointType.AnkleLeft].Position.X, body.Joints[JointType.AnkleLeft].Position.Y, body.Joints[JointType.AnkleLeft].Position.Z);
                                    Vector3D KneeLeft = new Vector3D(body.Joints[JointType.KneeLeft].Position.X, body.Joints[JointType.KneeLeft].Position.Y, body.Joints[JointType.KneeLeft].Position.Z);
                                    Vector3D HipLeft = new Vector3D(body.Joints[JointType.HipLeft].Position.X, body.Joints[JointType.HipLeft].Position.Y, body.Joints[JointType.HipLeft].Position.Z);
                                    // Base
                                    Vector3D SpineBase = new Vector3D(body.Joints[JointType.SpineBase].Position.X, body.Joints[JointType.SpineBase].Position.Y, body.Joints[JointType.SpineBase].Position.Z);
                                    // Right side
                                    Vector3D FootRight = new Vector3D(body.Joints[JointType.FootRight].Position.X, body.Joints[JointType.FootRight].Position.Y, body.Joints[JointType.FootRight].Position.Z);
                                    Vector3D AnkleRight = new Vector3D(body.Joints[JointType.AnkleRight].Position.X, body.Joints[JointType.AnkleRight].Position.Y, body.Joints[JointType.AnkleRight].Position.Z);
                                    Vector3D KneeRight = new Vector3D(body.Joints[JointType.KneeRight].Position.X, body.Joints[JointType.KneeRight].Position.Y, body.Joints[JointType.KneeRight].Position.Z);
                                    Vector3D HipRight = new Vector3D(body.Joints[JointType.HipRight].Position.X, body.Joints[JointType.HipRight].Position.Y, body.Joints[JointType.HipRight].Position.Z);

                                    // Left angle side
                                    double LeftAnkleAngle = AngleBetweenTwoVectors(AnkleLeft - KneeLeft, AnkleLeft - FootLeft);
                                    double LeftKneeAngle = AngleBetweenTwoVectors(KneeLeft - HipLeft, KneeLeft - AnkleLeft);
                                    double LeftHipAngle = AngleBetweenTwoVectors(HipLeft - SpineBase, HipLeft - KneeLeft);
                                    // Right angle side
                                    double RightAnkleAngle = AngleBetweenTwoVectors(AnkleRight - KneeRight, AnkleRight - FootRight);
                                    double RightKneeAngle = AngleBetweenTwoVectors(KneeRight - HipRight, KneeRight - AnkleRight);
                                    double RightHipAngle = AngleBetweenTwoVectors(HipRight - SpineBase, HipRight - KneeRight);

                                    // Send to box
                                    angleLeftAnkle = System.Convert.ToString(LeftAnkleAngle);
                                    angleLeftKnee = System.Convert.ToString(LeftKneeAngle);
                                    angleLeftHip = System.Convert.ToString(LeftKneeAngle);
                                    angleRightAnkle = System.Convert.ToString(RightAnkleAngle);
                                    angleRightKnee = System.Convert.ToString(RightKneeAngle);
                                    angleRightHip = System.Convert.ToString(RightHipAngle);

                                    // Draw skeleton.
                                    if (_displayBody)
                                    {
                                        //canvas.DrawSkeleton(body);

                                        // COORDINATE MAPPING
                                        foreach (Joint joint in body.Joints.Values)
                                        {
                                            if (joint.TrackingState == TrackingState.Tracked)
                                            {
                                                // 3D space point
                                                // pack the X Y Z values
                                                CameraSpacePoint jointPosition = joint.Position;

                                                // 2D space point 
                                                Point point = new Point();

                                                // We are always using the color frames
                                                ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);

                                                point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                                                point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;

                                                // Draw
                                                Ellipse ellipse = new Ellipse
                                                {
                                                    Fill = Brushes.Red,
                                                    Width = 30,
                                                    Height = 30
                                                };

                                                Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                                                Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                                                canvas.Children.Add(ellipse);
                                            }
                                        }

                                        // Send to text blocks
                                        VectorALA.Text = angleLeftAnkle;
                                        VectorALK.Text = angleLeftKnee;
                                        VectorALH.Text = angleLeftHip;
                                        VectorARA.Text = angleRightAnkle;
                                        VectorARK.Text = angleRightKnee;
                                        VectorARH.Text = angleRightHip;
                                        int milliseconds = 20;
                                        Thread.Sleep(milliseconds);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Angle calculation method
        public double AngleBetweenTwoVectors(Vector3D vectorA, Vector3D vectorB)
        {
            double dotProduct = 0.0;
            vectorA.Normalize();
            vectorB.Normalize();
            dotProduct = Vector3D.DotProduct(vectorA, vectorB);

            return (double)Math.Acos(dotProduct) / Math.PI * 180;
        }

        private void Tracking_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;
        }

        #endregion
    }
}
