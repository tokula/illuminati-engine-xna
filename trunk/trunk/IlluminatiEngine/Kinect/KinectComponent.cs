#if WINDOWS && KINECT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Microsoft.Kinect;

namespace IlluminatiEngine.Kinect
{
    public class KinectComponent : GameComponent
    {
        // color divisors for tinting depth pixels
        private static readonly int[] IntensityShiftByPlayerR = { 1, 2, 0, 2, 0, 0, 2, 0 };
        private static readonly int[] IntensityShiftByPlayerG = { 1, 2, 2, 0, 2, 0, 0, 1 };
        private static readonly int[] IntensityShiftByPlayerB = { 1, 0, 2, 2, 0, 2, 0, 2 };

        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        public KinectSensor Sensor { get; set; }

        public Texture2D colorMap;
        public Texture2D depthMap;
        public List<Point> renderJoints = new List<Point>();

        public bool IsRequired { get; set; }

        bool sensorConflict { get; set; }

        public KinectComponent(Game game, bool isRequired = true)
            : base(game)
        {
            game.Services.AddService(this.GetType(), this);
            game.Components.Add(this);

            IsRequired = isRequired;
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    this.UpdateStatus(sensor.Status);
                    this.Sensor = sensor;
                    break;
                }
            }

            // If we didn't find a connected Sensor
            if (this.Sensor == null)
            {
                // NOTE: this doesn't handle the multiple Kinect sensor case very well.
                foreach (KinectSensor sensor in KinectSensor.KinectSensors)
                {
                    this.UpdateStatus(sensor.Status);
                }
            }

            if (Sensor != null)
            {
                Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                Sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                Sensor.SkeletonStream.Enable();


                Sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(Sensor_ColorFrameReady);
                Sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(Sensor_DepthFrameReady);

                Sensor.Start();
            }

        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame imageFrame = e.OpenDepthImageFrame())
            {
                if (imageFrame != null)
                {
                    depthMap = new Texture2D(Game.GraphicsDevice, imageFrame.Width, imageFrame.Height, false, SurfaceFormat.Color);

                    short[] data = new short[imageFrame.PixelDataLength];
                    Color[] depthData = new Color[imageFrame.Width * imageFrame.Height];

                    imageFrame.CopyPixelDataTo(data);

                    ConvertDepthFrame(data, Sensor.DepthStream, ref depthData);


                    depthMap.SetData<Color>(depthData);

                }
                else
                {
                    // imageFrame is null because the request did not arrive in time
                }
            }

        }

        private void UpdateStatus(KinectStatus status)
        {
            string message = null;
            string moreInfo = null;
            Uri moreInfoUri = null;
            bool showRetry = false;

            switch (status)
            {
                case KinectStatus.Connected:
                    // If there's a sensor conflict, we wish to display all of the normal 
                    // states and statuses, with the exception of Connected.
                    if (this.sensorConflict)
                    {
                        message = "This Kinect is being used by another application.";
                        moreInfo = "This application needs a Kinect for Windows sensor in order to function. However, another application is using the Kinect Sensor.";
                        moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239812");
                        showRetry = true;
                    }
                    else
                    {
                        message = "All set!";
                        moreInfo = null;
                        moreInfoUri = null;
                    }

                    break;
                case KinectStatus.DeviceNotGenuine:
                    message = "This sensor is not genuine!";
                    moreInfo = "This application needs a genuine Kinect for Windows sensor in order to function. Please plug one into the PC.";
                    moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239813");

                    break;
                case KinectStatus.DeviceNotSupported:
                    message = "Kinect for Xbox not supported.";
                    moreInfo = "This application needs a Kinect for Windows sensor in order to function. Please plug one into the PC.";
                    moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239814");

                    break;
                case KinectStatus.Disconnected:
                    if (this.IsRequired)
                    {
                        message = "Required";
                        moreInfo = "This application needs a Kinect for Windows sensor in order to function. Please plug one into the PC.";
                        moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239815");
                    }
                    else
                    {
                        message = "Get the full experience by plugging in a Kinect for Windows sensor.";
                        moreInfo = "This application will use a Kinect for Windows sensor if one is plugged into the PC.";
                        moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239816");
                    }

                    break;
                case KinectStatus.NotReady:
                case KinectStatus.Error:
                    message = "Oops, there is an error.";
                    moreInfo = "The Kinect Sensor is plugged in, however an error has occured. For steps to resolve, please click the \"Tell me more\" link.";
                    moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239817");
                    break;
                case KinectStatus.Initializing:
                    message = "Initializing...";
                    moreInfo = null;
                    moreInfoUri = null;
                    break;
                case KinectStatus.InsufficientBandwidth:
                    message = "Too many USB devices! Please unplug one or more.";
                    moreInfo = "The Kinect Sensor needs the majority of the USB Bandwidth of a USB Controller. If other devices are in contention for that bandwidth, the Kinect Sensor may not be able to function.";
                    moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239818");
                    break;
                case KinectStatus.NotPowered:
                    message = "Plug my power cord in!";
                    moreInfo = "The Kinect Sensor is plugged into the computer with its USB connection, but the power plug appears to be not powered.";
                    moreInfoUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=239819");
                    break;
            }

            //this.UpdateMessage(status, message, moreInfo, moreInfoUri, showRetry);
        }

        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    colorMap = new Texture2D(Game.GraphicsDevice, imageFrame.Width, imageFrame.Height);

                    byte[] data = new byte[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(data);

                    // Copy to Texture2D
                    //Flip the R with the B
                    for (int b = 0; b < data.Length; b += 4)
                    {
                        byte tmp = data[b];
                        data[b] = data[b + 2];
                        data[b + 2] = tmp;
                    }

                    colorMap.SetData<byte>(data);
                }
                else
                {
                    // imageFrame is null because the request did not arrive in time          }
                }

            }
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        private void ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream, ref Color[] depthFrame32)
        {
            for (int i16 = 0; i16 < depthFrame.Length; i16++)
            {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(~(realDepth >> 4));

                if (player == 0 && realDepth == depthStream.TooNearDepth)
                {
                    // white 
                    depthFrame32[i16] = new Color((int)255, (int)255, (int)255);

                }
                else if (player == 0 && realDepth == depthStream.TooFarDepth)
                {
                    // dark purple
                    depthFrame32[i16] = new Color((int)66, 0, (int)66);
                }
                else if (player == 0 && realDepth == depthStream.UnknownDepth)
                {
                    // dark brown
                    depthFrame32[i16] = new Color((int)66, (int)66, (int)33);
                }
                else
                {
                    // tint the intensity by dividing by per-player values
                    depthFrame32[i16] = new Color((int)(intensity >> IntensityShiftByPlayerR[player]), (int)(intensity >> IntensityShiftByPlayerG[player]), (int)(intensity >> IntensityShiftByPlayerB[player]));
                }
            }
        }
    }
}
#endif