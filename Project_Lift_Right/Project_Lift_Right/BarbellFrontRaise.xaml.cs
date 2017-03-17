using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Kinect;
using System.ComponentModel;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Project_Lift_Right
{

    // display type for reference
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BarbellFrontRaise : Page
    {

            private const DisplayFrameType DEFAULT_DISPLAYFRAMETYPE = DisplayFrameType.Infrared;
            //private const DisplayFrameType DEFAULT_DISPLAYFRAMETYPE = DisplayFrameType.BodyJoints;
            /// <summary>
            /// The highest value that can be returned in the InfraredFrame.
            /// It is cast to a float for readability in the visualization code.
            /// </summary>
            private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;


            /// <summary>
            /// Used to set the lower limit, post processing, of the
            /// infrared data that we will render.
            /// Increasing or decreasing this value sets a brightness 
            /// "wall" either closer or further away.
            /// </summary>
            private const float InfraredOutputValueMinimum = 0.01f;

            /// <summary>
            /// The upper limit, post processing, of the
            /// infrared data that will render.
            /// </summary>
            private const float InfraredOutputValueMaximum = 1.0f;

            /// <summary>
            /// The InfraredSceneValueAverage value specifies the average infrared 
            /// value of the scene. This value was selected by analyzing the average 
            /// pixel intensity for a given scene. 
            /// This could be calculated at runtime to handle different IR conditions
            /// of a scene (outside vs inside).
            /// </summary>
            private const float InfraredSceneValueAverage = 0.08f;

            /// <summary>
            /// The InfraredSceneStandardDeviations value specifies the number of 
            /// standard deviations to apply to InfraredSceneValueAverage. 
            /// This value was selected by analyzing data from a given scene.
            /// This could be calculated at runtime to handle different IR conditions
            /// of a scene (outside vs inside).
            /// </summary>
            private const float InfraredSceneStandardDeviations = 3.0f;

            // Size of the RGB pixel in the bitmap
            private const int BytesPerPixel = 4;

            private KinectSensor kinectSensor = null;
            private string statusText = null;
            private WriteableBitmap bitmap = null;
            private FrameDescription currentFrameDescription;
            private DisplayFrameType currentDisplayFrameType;
            private MultiSourceFrameReader multiSourceFrameReader = null;
            private CoordinateMapper coordinateMapper = null;
            private BodiesManager bodiesManager = null;


            // team 583 constants
            public int stopwatch_start;
            public Stopwatch stopwatch = new Stopwatch();
            public string current_state = "NOT_START";
            public const double START_MIN = 0;
            public const double START_MAX = 35;
            public const double END_MAX = 180;
            public const double END_MIN = 100;
            public const double HALF_ANGLE = 60;
            public double BEND_MAX = 180;
            public const double BEND_MIN = 140;
            public bool timer_started = false;
            public int rep_count = 0;
            public int right_rep_count = 0;
            public int failed_rep_count = 0;
            public int right_failed_rep_count = 0;
            public int start_hold_time = 3000;
            public int end_hold_time = 1500;



            //Infrared Frame 
            private InfraredFrameReader infraredFrameReader = null;
            private ushort[] infraredFrameData = null;
            private byte[] infraredPixels = null;

            //BodyMask Frames
            private DepthSpacePoint[] colorMappedToDepthPoints = null;

            //Body Joints are drawn here
            private Canvas drawingCanvas;

            public event PropertyChangedEventHandler PropertyChanged;

            public string StatusText
            {
                get { return this.statusText; }
                set
                {
                    if (this.statusText != value)
                    {
                        this.statusText = value;
                        if (this.PropertyChanged != null)
                        {
                            this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                        }
                    }
                }
            }

            public FrameDescription CurrentFrameDescription
            {
                get { return this.currentFrameDescription; }
                set
                {
                    if (this.currentFrameDescription != value)
                    {
                        this.currentFrameDescription = value;
                        if (this.PropertyChanged != null)
                        {
                            this.PropertyChanged(this, new PropertyChangedEventArgs("CurrentFrameDescription"));
                        }
                    }
                }
            }


            public BarbellFrontRaise()
            {

                // one sensor is currently supported
                this.kinectSensor = KinectSensor.GetDefault();

                SetupCurrentDisplay(DEFAULT_DISPLAYFRAMETYPE);
                //SetupCurrentDisplay(DisplayFrameType.BodyJoints);
                this.coordinateMapper = this.kinectSensor.CoordinateMapper;

                this.multiSourceFrameReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Infrared | FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);

                this.multiSourceFrameReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

                // get the infraredFrameDescription from the InfraredFrameSource
                /* FrameDescription infraredFrameDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;

                 // open the reader for the infrared frames
                 this.infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();

                 // wire handler for frame arrival
                 this.infraredFrameReader.FrameArrived += this.Reader_InfraredFrameArrived;

                 // allocate space to put the pixels being received and converted
                 this.infraredFrameData = new ushort[infraredFrameDescription.Width * infraredFrameDescription.Height];
                 this.infraredPixels = new byte[infraredFrameDescription.Width * infraredFrameDescription.Height * BytesPerPixel];

                 // create the bitmap to display
                 this.bitmap = new WriteableBitmap(infraredFrameDescription.Width, infraredFrameDescription.Height);

                 this.CurrentFrameDescription = infraredFrameDescription;
                 */

                // set IsAvailableChanged event notifier
                this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

                // use the window object as the view model in this simple example
                this.DataContext = this;

                // open the sensor
                this.kinectSensor.Open();


                this.InitializeComponent();

                // bigAssCounter.Text = "";

                //Setup the display on Screen
                Debug.WriteLine("Starting up the Display\n");
                SetupCurrentDisplay(DisplayFrameType.BodyJoints);
            }

            // This is called whenever the current display changes.
            private void SetupCurrentDisplay(DisplayFrameType newDisplayFrameType)
            {
                currentDisplayFrameType = newDisplayFrameType;
                // Frames used by more than one type are declared outside the switch
                FrameDescription colorFrameDescription = null;

                // reset the display methods
                if (this.BodyJointsGrid != null)
                {
                    this.BodyJointsGrid.Visibility = Visibility.Collapsed;
                }
                if (this.FrameDisplayImage != null)
                {
                    this.FrameDisplayImage.Source = null;
                }
                switch (currentDisplayFrameType)
                {
                    case DisplayFrameType.Infrared:
                        FrameDescription infraredFrameDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;
                        this.CurrentFrameDescription = infraredFrameDescription;
                        // allocate space to put the pixels being received and converted
                        this.infraredFrameData = new ushort[infraredFrameDescription.Width * infraredFrameDescription.Height];
                        this.infraredPixels = new byte[infraredFrameDescription.Width * infraredFrameDescription.Height * BytesPerPixel];
                        this.bitmap = new WriteableBitmap(infraredFrameDescription.Width, infraredFrameDescription.Height);
                        break;

                    case DisplayFrameType.BodyJoints:
                        // instantiate a new Canvas
                        this.drawingCanvas = new Canvas();

                        // set the clip rectangle to prevent rendering outside the canvas
                        this.drawingCanvas.Clip = new RectangleGeometry();
                        this.drawingCanvas.Clip.Rect = new Rect(0.0, 0.0, this.BodyJointsGrid.Width, this.BodyJointsGrid.Height);
                        this.drawingCanvas.Width = this.BodyJointsGrid.Width;
                        this.drawingCanvas.Height = this.BodyJointsGrid.Height;

                        // reset the body joints grid
                        this.BodyJointsGrid.Visibility = Visibility.Visible;
                        this.BodyJointsGrid.Children.Clear();

                        // add canvas to DisplayGrid
                        this.BodyJointsGrid.Children.Add(this.drawingCanvas);
                        bodiesManager = new BodiesManager(this.coordinateMapper, this.drawingCanvas, this.kinectSensor.BodyFrameSource.BodyCount);


                        colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;
                        this.CurrentFrameDescription = colorFrameDescription;

                        // create the bitmap to display
                        this.bitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height);



                        break;
                    default:
                        break;
                }
            }
            // MARK -- Weightlift Tracker Engine
            private void ShowBodyJoints(BodyFrame bodyFrame)
            {
                Body[] bodies = new Body[this.kinectSensor.BodyFrameSource.BodyCount];
                bool dataReceived = false;


                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                    // analyze the body
                    foreach (var body in bodies)
                    {

                        if (body != null)
                        {

                            // Do something with the body...
                            if (body.IsTracked)
                            {

                                /*Joint head = body.Joints[JointType.Head];

                                float x = head.Position.X;
                                float y = head.Position.Y;
                                float z = head.Position.Z;

                                feedback_textBlock.Text = x.ToString("F");
                                 */
                                //left arm

                                // Weightlifter Tracker Engine
                                Vector3 left_Wrist = new Vector3(body.Joints[JointType.WristLeft].Position.X, body.Joints[JointType.WristLeft].Position.Y, body.Joints[JointType.WristLeft].Position.Z);
                                Vector3 left_Elbow = new Vector3(body.Joints[JointType.ElbowLeft].Position.X, body.Joints[JointType.ElbowLeft].Position.Y, body.Joints[JointType.ElbowLeft].Position.Z);
                                Vector3 left_Shoulder = new Vector3(body.Joints[JointType.ShoulderLeft].Position.X, body.Joints[JointType.ShoulderLeft].Position.Y, body.Joints[JointType.ShoulderLeft].Position.Z);
                                Vector3 spine_mid = new Vector3(body.Joints[JointType.SpineMid].Position.X, body.Joints[JointType.SpineMid].Position.Y, body.Joints[JointType.SpineMid].Position.Z);
                                Vector3 neck = new Vector3(body.Joints[JointType.SpineShoulder].Position.X, body.Joints[JointType.SpineShoulder].Position.Y, body.Joints[JointType.SpineShoulder].Position.Z);


                                double left_arm = Vector3.Angle(Vector3.Subtract(left_Elbow, left_Shoulder), Vector3.Subtract(left_Elbow, left_Wrist));
                                double raised_arm = Vector3.Angle(Vector3.Subtract(neck, left_Elbow), Vector3.Subtract(neck, spine_mid));

                                if (raised_arm > 180)
                                {
                                    raised_arm = 360 - raised_arm;
                                }
                                if (left_arm > 180)
                                {
                                    left_arm = 360 - left_arm;
                                }
                                left_value_textBlock.Text = left_arm.ToString("F");

                                //right arm
                                Vector3 right_Wrist = new Vector3(body.Joints[JointType.WristRight].Position.X, body.Joints[JointType.WristRight].Position.Y, body.Joints[JointType.WristRight].Position.Z);
                                Vector3 right_Elbow = new Vector3(body.Joints[JointType.ElbowRight].Position.X, body.Joints[JointType.ElbowRight].Position.Y, body.Joints[JointType.ElbowRight].Position.Z);
                                Vector3 right_Shoulder = new Vector3(body.Joints[JointType.ShoulderRight].Position.X, body.Joints[JointType.ShoulderRight].Position.Y, body.Joints[JointType.ShoulderRight].Position.Z);

                                double right_arm = Vector3.Angle(Vector3.Subtract(right_Elbow, right_Shoulder), Vector3.Subtract(right_Elbow, right_Wrist));
                                double right_raised_arm = Vector3.Angle(Vector3.Subtract(neck, right_Elbow), Vector3.Subtract(neck, spine_mid));

                                if (right_arm > 180)
                                {
                                    right_arm = 360 - right_arm;
                                }
                                if (right_raised_arm > 180)
                                {
                                    right_raised_arm = 360 - right_raised_arm;
                                }
                                right_value_textBlock.Text = right_arm.ToString("F");
                                elbow_angle_value.Text = raised_arm.ToString("F");
                                right_elbow_angle_value.Text = right_raised_arm.ToString("F");
                                feedback_textBlock.Text = current_state;
                                Debug.WriteLine(raised_arm);


                                if (current_state == "NOT_START")
                                {
                                    arm_value.Text = "LEFT Arm";
                                    if (!In_Range(BEND_MIN, BEND_MAX, left_arm))
                                    {
                                        message.Text = "Please ensure your arm is straight.";
                                        bigAssCounter.Text = "";
                                        if (timer_started)
                                        {
                                            stopwatch.Stop();
                                            stopwatch.Reset();
                                            timer_started = false;
                                        }
                                    }
                                    else
                                    {

                                        //Debug.WriteLine("NOT_START\nbigAssCounter.Text = "Ready!";


                                        // if successful, we are in the start range
                                        if (In_Range(START_MIN, START_MAX, raised_arm))
                                        {
                                            //Debug.WriteLine("TIMER SET");
                                            // check if timer is set, if then set it.
                                            if (timer_started)
                                            {
                                                //Debug.WriteLine("CALCULATING TIMER");
                                                //calculate the time, then if past two seconds change stare to START_PULLUP
                                                //stopwatch.Stop();
                                                long current_time = stopwatch.ElapsedMilliseconds;

                                                if (current_time >= start_hold_time)
                                                {
                                                    bigAssCounter.Text = "";
                                                    current_state = "START_PULLUP";
                                                    message.Text = "";
                                                    stopwatch.Stop();
                                                    stopwatch.Reset();
                                                    timer_started = false;
                                                }
                                                else
                                                {
                                                    // if less than the time, show on gui timer.
                                                    long countdown = (start_hold_time - current_time) / 1000;
                                                    long temp = (start_hold_time - current_time) % 1000;
                                                    message.Text = "Hold right there!";
                                                    bigAssCounter.Text = countdown.ToString() + "." + temp.ToString();

                                                }

                                            }
                                            else if (!timer_started)
                                            {
                                                //Debug.WriteLine("Starting Timer");
                                                // timer hasn't started so set the time
                                                timer_started = true;
                                                stopwatch.Start();

                                            }
                                        }
                                        else
                                        {
                                            message.Text = "Straightened your arm. And point to the floor!";
                                        }
                                    }

                                }
                                else if (current_state == "START_PULLUP")
                                {
                                        if (!In_Range(BEND_MIN, BEND_MAX, left_arm))
                                        {
                                            bigAssCounter.Text = "Wrong Form";
                                            message.Text = "Your arm is not straight.";
                                        }
                                        else
                                        {
                                            message.Text = "";
                                            bigAssCounter.Text = "UP";
                                            //is the arm in hold position?
                                            if (In_Range(END_MIN, END_MAX, raised_arm))
                                            {
                                                // yes!
                                                current_state = "START_HOLD";
                                            }
                                        }

                                }
                                else if (current_state == "START_HOLD")
                                {
                                    if (!In_Range(BEND_MIN, BEND_MAX, left_arm))
                                    {
                                        bigAssCounter.Text = "Wrong Form!";
                                        message.Text = "Straigthen your arm.";
                                        stopwatch.Stop();
                                        stopwatch.Reset();
                                        timer_started = false;
                                    }
                                    else
                                    {

                                        message.Text = "";
                                        // is the arm still in hold postion?
                                        if (In_Range(END_MIN, END_MAX, raised_arm))
                                        {

                                            // yes!

                                            if (timer_started)
                                            {
                                                long current_time = stopwatch.ElapsedMilliseconds;
                                                if (current_time >= end_hold_time)
                                                {
                                                    current_state = "START_PULLDOWN";
                                                    rep_count++;
                                                    bigAssCounter.Text = "";
                                                    stopwatch.Stop();
                                                    stopwatch.Reset();
                                                    timer_started = false;
                                                }
                                                else
                                                {
                                                    long countdown = (end_hold_time - current_time) / 1000;
                                                    long temp = (end_hold_time - current_time) % 1000;

                                                    bigAssCounter.Text = countdown.ToString() + "." + temp.ToString();

                                                }
                                            }
                                            else
                                            {
                                                timer_started = true;
                                                stopwatch.Start();
                                            }
                                        }
                                        else
                                        {
                                            // no! user pull down too early
                                            stopwatch.Stop();
                                            stopwatch.Reset();
                                            timer_started = false;
                                            bigAssCounter.Text = ":(";
                                            message.Text = "You pulled down too early! Hold your arm until the timer has completed.";
                                            // is the arm angle below half?
                                            if (raised_arm > HALF_ANGLE)
                                            {
                                                failed_rep_count++;
                                                // yes, he is giving up the rap
                                                current_state = "NOT_START";
                                            }
                                            else
                                            {
                                                // no, he is still trying to go back
                                                // do nothing
                                            }
                                        }
                                    }

                                }
                                else if (current_state == "START_PULLDOWN")
                                {
                                    if (!In_Range(BEND_MIN, BEND_MAX, left_arm))
                                    {

                                        bigAssCounter.Text = "Wrong Form!";
                                        message.Text = "Move your elbow closer to your body!";

                                    }
                                    else
                                    {

                                        bigAssCounter.Text = "DOWN";
                                        message.Text = "";
                                        if (In_Range(START_MIN, START_MAX, raised_arm))
                                        {
                                            current_state = "LEFT_DONE";
                                        }

                                    }

                                }
                                else if (current_state == "LEFT_DONE")
                                {
                                    bigAssCounter.Text = "";
                                    start_hold_time = 1000;
                                    current_state = "RIGHT_NOT_START";
                                }
                                else if (current_state == "RIGHT_NOT_START")
                                {
                                    arm_value.Text = "RIGHT Arm";
                                    if (!In_Range(BEND_MIN, BEND_MAX, right_arm))
                                    {
                                        message.Text = "Please ensure your right arm is straight.";
                                        bigAssCounter.Text = "";
                                        if (timer_started)
                                        {
                                            stopwatch.Stop();
                                            stopwatch.Reset();
                                            timer_started = false;
                                        }
                                    }
                                    else
                                    {

                                        // if successful, we are in the start range
                                        if (In_Range(START_MIN, START_MAX, right_raised_arm))
                                        {
                                            //Debug.WriteLine("TIMER SET");
                                            // check if timer is set, if then set it.
                                            if (timer_started)
                                            {
                                                //Debug.WriteLine("CALCULATING TIMER");
                                                //calculate the time, then if past two seconds change stare to START_PULLUP
                                                //stopwatch.Stop();
                                                long current_time = stopwatch.ElapsedMilliseconds;

                                                if (current_time >= start_hold_time)
                                                {
                                                    bigAssCounter.Text = "";
                                                    current_state = "RIGHT_START_PULLUP";
                                                    message.Text = "";
                                                    stopwatch.Stop();
                                                    stopwatch.Reset();
                                                    timer_started = false;
                                                }
                                                else
                                                {
                                                    // if less than the time, show on gui timer.
                                                    long countdown = (start_hold_time - current_time) / 1000;
                                                    long temp = (start_hold_time - current_time) % 1000;
                                                    message.Text = "Hold right there!";
                                                    bigAssCounter.Text = countdown.ToString() + "." + temp.ToString();

                                                }

                                            }
                                            else if (!timer_started)
                                            {
                                                //Debug.WriteLine("Starting Timer");
                                                // timer hasn't started so set the time
                                                timer_started = true;
                                                stopwatch.Start();

                                            }
                                        }
                                        else
                                        {
                                            message.Text = "Straightened your arm. And point to the floor!";
                                        }
                                    }

                                }
                                else if (current_state == "RIGHT_START_PULLUP")
                                {
                                    if (!In_Range(BEND_MIN, BEND_MAX, right_arm))
                                    {
                                        bigAssCounter.Text = "Wrong Form";
                                        message.Text = "Your arm is not straight.";
                                    }
                                    else
                                    {
                                        message.Text = "";
                                        bigAssCounter.Text = "UP";
                                        //is the arm in hold position?
                                        if (In_Range(END_MIN, END_MAX, right_raised_arm))
                                        {
                                            // yes!
                                            current_state = "RIGHT_START_HOLD";
                                        }
                                    }

                                }
                                else if (current_state == "RIGHT_START_HOLD")
                                {
                                    if (!In_Range(BEND_MIN, BEND_MAX, right_arm))
                                    {
                                        bigAssCounter.Text = "Wrong Form!";
                                        message.Text = "Straigthen your arm.";
                                        stopwatch.Stop();
                                        stopwatch.Reset();
                                        timer_started = false;
                                    }
                                    else
                                    {

                                        message.Text = "";
                                        // is the arm still in hold postion?
                                        if (In_Range(END_MIN, END_MAX, right_raised_arm))
                                        {

                                            // yes!

                                            if (timer_started)
                                            {
                                                long current_time = stopwatch.ElapsedMilliseconds;
                                                if (current_time >= end_hold_time)
                                                {
                                                    current_state = "RIGHT_START_PULLDOWN";
                                                    right_rep_count++;
                                                    bigAssCounter.Text = "";
                                                    stopwatch.Stop();
                                                    stopwatch.Reset();
                                                    timer_started = false;
                                                }
                                                else
                                                {
                                                    long countdown = (end_hold_time - current_time) / 1000;
                                                    long temp = (end_hold_time - current_time) % 1000;

                                                    bigAssCounter.Text = countdown.ToString() + "." + temp.ToString();

                                                }
                                            }
                                            else
                                            {
                                                timer_started = true;
                                                stopwatch.Start();
                                            }
                                        }
                                        else
                                        {
                                            // no! user pull down too early
                                            stopwatch.Stop();
                                            stopwatch.Reset();
                                            timer_started = false;
                                            bigAssCounter.Text = ":(";
                                            message.Text = "You pulled down too early! Hold your arm until the timer has completed.";
                                            // is the arm angle below half?
                                            if (right_raised_arm > HALF_ANGLE)
                                            {
                                                right_failed_rep_count++;
                                                // yes, he is giving up the rap
                                                current_state = "NOT_START";
                                            }
                                            else
                                            {
                                                // no, he is still trying to go back
                                                // do nothing
                                            }
                                        }
                                    }
                                }
                                else if (current_state == "RIGHT_START_PULLDOWN")
                                {
                                    if (!In_Range(BEND_MIN, BEND_MAX, right_arm))
                                    {

                                        bigAssCounter.Text = "Wrong Form!";
                                        message.Text = "Straighten your arm!";

                                    }
                                    else
                                    {

                                        bigAssCounter.Text = "DOWN";
                                        message.Text = "";
                                        if (In_Range(START_MIN, START_MAX, right_raised_arm))
                                        {
                                            current_state = "DONE";
                                        }

                                    }
                                }
                                else if (current_state == "DONE")
                                    {
                                    start_hold_time = 1000;
                                    current_state = "NOT_START";
                                }
                            }
                        }

                    }
                }
                if (dataReceived)
                {
                    this.bodiesManager.UpdateBodiesAndEdges(bodies);
                    rep_counter_value.Text = rep_count.ToString();
                    failed_rep_counter_value.Text = failed_rep_count.ToString();
                    right_rep_counter_value.Text = right_rep_count.ToString();
                    right_failed_rep_counter_value.Text = right_failed_rep_count.ToString();
                }
            }

            private void ShowInfraredFrame(InfraredFrame infraredFrame)
            {
                bool infraredFrameProcessed = false;

                if (infraredFrame != null)
                {
                    FrameDescription infraredFrameDescription = infraredFrame.FrameDescription;

                    // verify data and write the new infrared frame data to the display bitmap
                    if (((infraredFrameDescription.Width * infraredFrameDescription.Height)
                        == this.infraredFrameData.Length) &&
                        (infraredFrameDescription.Width == this.bitmap.PixelWidth) &&
                        (infraredFrameDescription.Height == this.bitmap.PixelHeight))
                    {
                        // Copy the pixel data from the image to a temporary array
                        infraredFrame.CopyFrameDataToArray(this.infraredFrameData);

                        infraredFrameProcessed = true;
                    }
                }

                // we got a frame, convert and render
                if (infraredFrameProcessed)
                {
                    this.ConvertInfraredDataToPixels();
                    this.RenderPixelArray(this.infraredPixels);
                }
            }

            private void Reader_MultiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs e)
            {

                MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

                // If the Frame has expired by the time we process this event, return.
                if (multiSourceFrame == null)
                {
                    return;
                }
                DepthFrame depthFrame = null;
                ColorFrame colorFrame = null;
                InfraredFrame infraredFrame = null;
                BodyFrame bodyFrame = null;
                BodyIndexFrame bodyIndexFrame = null;
                IBuffer depthFrameData = null;
                IBuffer bodyIndexFrameData = null;
                // Com interface for unsafe byte manipulation
                // IBufferByteAccess bodyIndexByteAccess = null;

                switch (currentDisplayFrameType)
                {
                    case DisplayFrameType.Infrared:
                        using (infraredFrame = multiSourceFrame.InfraredFrameReference.AcquireFrame())
                        {
                            ShowInfraredFrame(infraredFrame);
                        }
                        break;
                    /* case DisplayFrameType.Color:
                         using (colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
                         {
                             ShowColorFrame(colorFrame);
                         }
                         break;
                     case DisplayFrameType.Depth:
                         using (depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame())
                         {
                             ShowDepthFrame(depthFrame);
                         }
                         break;*/
                    case DisplayFrameType.BodyJoints:

                        using (colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
                        {
                            ShowColorFrame(colorFrame);
                        }


                        using (bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame())
                        {
                            ShowBodyJoints(bodyFrame);
                        }
                        break;
                    default:
                        break;
                }
            }

            private void Sensor_IsAvailableChanged(KinectSensor sender, IsAvailableChangedEventArgs args)
            {
                this.StatusText = this.kinectSensor.IsAvailable ? "Running" : "Not Available";
            }


            private void ConvertInfraredDataToPixels()
            {
                // Convert the infrared to RGB
                int colorPixelIndex = 0;
                for (int i = 0; i < this.infraredFrameData.Length; ++i)
                {
                    // normalize the incoming infrared data (ushort) to a float ranging from 
                    // [InfraredOutputValueMinimum, InfraredOutputValueMaximum] by
                    // 1. dividing the incoming value by the source maximum value
                    float intensityRatio = (float)this.infraredFrameData[i] / InfraredSourceValueMaximum;

                    // 2. dividing by the (average scene value * standard deviations)
                    intensityRatio /= InfraredSceneValueAverage * InfraredSceneStandardDeviations;

                    // 3. limiting the value to InfraredOutputValueMaximum
                    intensityRatio = Math.Min(InfraredOutputValueMaximum, intensityRatio);

                    // 4. limiting the lower value InfraredOutputValueMinimum
                    intensityRatio = Math.Max(InfraredOutputValueMinimum, intensityRatio);

                    // 5. converting the normalized value to a byte and using the result
                    // as the RGB components required by the image
                    byte intensity = (byte)(intensityRatio * 255.0f);
                    this.infraredPixels[colorPixelIndex++] = intensity; //Blue
                    this.infraredPixels[colorPixelIndex++] = intensity; //Green
                    this.infraredPixels[colorPixelIndex++] = intensity; //Red
                    this.infraredPixels[colorPixelIndex++] = 255;       //Alpha
                }
            }

            private void ShowColorFrame(ColorFrame colorFrame)
            {
                bool colorFrameProcessed = false;

                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    // verify data and write the new color frame data to the Writeable bitmap
                    if ((colorFrameDescription.Width == this.bitmap.PixelWidth) && (colorFrameDescription.Height == this.bitmap.PixelHeight))
                    {
                        if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                        {
                            colorFrame.CopyRawFrameDataToBuffer(this.bitmap.PixelBuffer);
                        }
                        else
                        {
                            colorFrame.CopyConvertedFrameDataToBuffer(this.bitmap.PixelBuffer, ColorImageFormat.Bgra);
                        }

                        colorFrameProcessed = true;
                    }
                }

                if (colorFrameProcessed)
                {
                    this.bitmap.Invalidate();
                    FrameDisplayImage.Source = this.bitmap;
                }
            }

            private void RenderPixelArray(byte[] pixels)
            {
                pixels.CopyTo(this.bitmap.PixelBuffer);
                this.bitmap.Invalidate();
                FrameDisplayImage.Source = this.bitmap;
            }

            private void InfraredButton_Click(object sender, RoutedEventArgs e)
            {
                SetupCurrentDisplay(DisplayFrameType.Infrared);
            }

            private void BodyJointsButton_Click(object sender, RoutedEventArgs e)
            {
                SetupCurrentDisplay(DisplayFrameType.BodyJoints);
                feedback_textBlock.Text = "Starting";
            }

            private void done_btn_Click(object sender, RoutedEventArgs e)
            {
                this.Frame.Navigate(typeof(Summary), null);
            }

            // MARK - HELPER FUNCTIONS
            private bool CompareAngles(double current_angle, double expected_angle, double error_factor)
            {
                return (current_angle >= expected_angle - error_factor && current_angle <= expected_angle + error_factor);
            }

            private bool In_Range(double start_range, double end_range, double angle)
            {
                if (angle < end_range && angle > start_range)
                {
                    return true;
                }
                return false;
            }

            private void finish_btn_Click(object sender, RoutedEventArgs e)
            {
                this.Frame.Navigate(typeof(MainPage), null);
            }
        }
    }
