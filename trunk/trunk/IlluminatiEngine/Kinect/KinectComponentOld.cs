//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using Microsoft.Research.Kinect.Nui;
//using Microsoft.Research.Kinect.Audio;

//namespace IlluminatiEngine.Kinect
//{
//    public class KinectXNAComponent : GameComponent
//    {
//        public Runtime nui;
//        public Texture2D colorMap;
//        public Texture2D depthMap;
//        public List<Point> renderJoints = new List<Point>();

//        const int RED_IDX = 2;
//        const int GREEN_IDX = 1;
//        const int BLUE_IDX = 0;
//        byte[] depthFrame32 = new byte[320 * 240 * 4];
//        byte[] playerFrame32 = new byte[320 * 240 * 4];

//        public ImageResolution VideoStreamSize = ImageResolution.Resolution640x480;
//        public ImageResolution DepthStreamSize = ImageResolution.Resolution320x240;

//        public Point depthResolution = new Point();
//        public Point videoResolution = new Point();

//        public Texture2D playerMask;

//        public KinectXNAComponent(Game game)
//            : base(game)
//        {
//            game.Services.AddService(this.GetType(), this);
//            game.Components.Add(this);
//        }

//        public override void Initialize()
//        {
//            base.Initialize();

//            nui = new Runtime();
//            nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);

//            nui.VideoStream.Open(ImageStreamType.Video, 2, VideoStreamSize, ImageType.Color);
//            nui.DepthStream.Open(ImageStreamType.Depth, 2, DepthStreamSize, ImageType.DepthAndPlayerIndex);

//            if (depthResolution == new Point())
//            {
//                switch (nui.DepthStream.Resolution)
//                {
//                    case ImageResolution.Resolution1280x1024:
//                        depthResolution = new Point(1280, 1024);
//                        depthFrame32 = new byte[1280 * 1024 * 4];
//                        break;
//                    case ImageResolution.Resolution320x240:
//                        depthResolution = new Point(320, 240);
//                        depthFrame32 = new byte[320 * 240 * 4];
//                        break;
//                    case ImageResolution.Resolution640x480:
//                        depthResolution = new Point(640, 480);
//                        depthFrame32 = new byte[640 * 480 * 4];
//                        break;
//                    case ImageResolution.Resolution80x60:
//                        depthResolution = new Point(80, 60);
//                        depthFrame32 = new byte[80 * 60 * 4];
//                        break;
//                }
//            }

//            if (videoResolution == new Point())
//            {
//                switch (nui.VideoStream.Resolution)
//                {
//                    case ImageResolution.Resolution1280x1024:
//                        videoResolution = new Point(1280, 1024);
//                        break;
//                    case ImageResolution.Resolution320x240:
//                        videoResolution = new Point(320, 240);
//                        break;
//                    case ImageResolution.Resolution640x480:
//                        videoResolution = new Point(640, 480);
//                        break;
//                    case ImageResolution.Resolution80x60:
//                        videoResolution = new Point(80, 60);
//                        break;
//                }
//            }


//            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
//            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
//            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);
//        }

//        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
//        {
//            PlanarImage Image = e.ImageFrame.Image;

//            if (colorMap == null)
//                colorMap = new Texture2D(Game.GraphicsDevice, Image.Width, Image.Height, false, SurfaceFormat.Color);

//            byte[] data = Image.Bits;

//            // Flip the R with the B
//            for (int b = 0; b < data.Length; b += 4)
//            {
//                byte tmp = data[b];
//                data[b] = data[b + 2];
//                data[b + 2] = tmp;
//            }

//            colorMap.SetData<byte>(data);
//        }

//        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
//        {
//            PlanarImage Image = e.ImageFrame.Image;

//            if (depthMap == null)
//                depthMap = new Texture2D(Game.GraphicsDevice, Image.Width, Image.Height, false, SurfaceFormat.Color);

//            depthMap.SetData<byte>(convertDepthFrame(Image.Bits));

//            if(playerMask == null)
//                playerMask = new Texture2D(Game.GraphicsDevice, Image.Width, Image.Height, false, SurfaceFormat.Color);

//            playerMask.SetData<byte>(playerFrame32);
//        }

//        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
//        // that displays different players in different colors
//        byte[] convertDepthFrame(byte[] depthFrame16)
//        {
//            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
//            {
//                int player = depthFrame16[i16] & 0x07;
//                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
//                // transform 13-bit depth information into an 8-bit intensity appropriate
//                // for display (we disregard information in most significant bit)
//                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

//                depthFrame32[i32 + RED_IDX] = 0;
//                depthFrame32[i32 + GREEN_IDX] = 0;
//                depthFrame32[i32 + BLUE_IDX] = 0;

//                playerFrame32[i32 + RED_IDX] = 0;
//                playerFrame32[i32 + GREEN_IDX] = 0;
//                playerFrame32[i32 + BLUE_IDX] = 0;

//                // choose different display colors based on player
//                switch (player)
//                {
//                    case 0:
//                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
//                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
//                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 2);
//                        break;
//                    case 1:
//                        depthFrame32[i32 + RED_IDX] = intensity;
//                        playerFrame32[i32 + RED_IDX] = intensity; 
//                        break;
//                    case 2:
//                        depthFrame32[i32 + GREEN_IDX] = intensity;

//                        playerFrame32[i32 + GREEN_IDX] = intensity;
//                        break;
//                    case 3:
//                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 4);
//                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
//                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);

//                        playerFrame32[i32 + RED_IDX] = (byte)(intensity / 4);
//                        playerFrame32[i32 + GREEN_IDX] = (byte)(intensity);
//                        playerFrame32[i32 + BLUE_IDX] = (byte)(intensity);
//                        break;
//                    case 4:
//                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
//                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
//                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 4);

//                        playerFrame32[i32 + RED_IDX] = (byte)(intensity);
//                        playerFrame32[i32 + GREEN_IDX] = (byte)(intensity);
//                        playerFrame32[i32 + BLUE_IDX] = (byte)(intensity / 4);
//                        break;
//                    case 5:
//                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
//                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 4);
//                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);

//                        playerFrame32[i32 + RED_IDX] = (byte)(intensity);
//                        playerFrame32[i32 + GREEN_IDX] = (byte)(intensity / 4);
//                        playerFrame32[i32 + BLUE_IDX] = (byte)(intensity);
//                        break;
//                    case 6:
//                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
//                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
//                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);

//                        playerFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
//                        playerFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
//                        playerFrame32[i32 + BLUE_IDX] = (byte)(intensity);
//                        break;
//                    case 7:
//                        depthFrame32[i32 + RED_IDX] = (byte)(255 - intensity);
//                        depthFrame32[i32 + GREEN_IDX] = (byte)(255 - intensity);
//                        depthFrame32[i32 + BLUE_IDX] = (byte)(255 - intensity);

//                        playerFrame32[i32 + RED_IDX] = (byte)(255 - intensity);
//                        playerFrame32[i32 + GREEN_IDX] = (byte)(255 - intensity);
//                        playerFrame32[i32 + BLUE_IDX] = (byte)(255 - intensity);
//                        break;
//                }                
//            }
//            return depthFrame32;
//        }

//        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
//        {
//            SkeletonFrame skeletonFrame = e.SkeletonFrame;

//            renderJoints.Clear();
//            foreach (SkeletonData data in skeletonFrame.Skeletons)
//            {
//                if (SkeletonTrackingState.Tracked == data.TrackingState)
//                {
//                    foreach (Joint joint in data.Joints)
//                    {
//                        renderJoints.Add(getDisplayPosition(joint));
//                    }
//                }
//            }
//        }

//        private Point getDisplayPosition(Joint joint)
//        {
//            if (depthResolution != new Point() && videoResolution != new Point())
//            {
//                float depthX, depthY;
//                nui.SkeletonEngine.SkeletonToDepthImage(joint.Position,
//                    out depthX, out depthY);

//                depthX = Math.Max(0, Math.Min(depthX * depthResolution.X, depthResolution.X));
//                depthY = Math.Max(0, Math.Min(depthY * depthResolution.Y, depthResolution.Y));

//                int colorX, colorY;
//                ImageViewArea iv = new ImageViewArea();
//                // only ImageResolution.Resolution640x480 is supported at this point
//                nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(nui.VideoStream.Resolution, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

//                return new Point((int)(Game.GraphicsDevice.Viewport.Width * colorX / videoResolution.X), (int)(Game.GraphicsDevice.Viewport.Height * colorY / videoResolution.Y));
//            }
//            else
//                return new Point();
//        }
//    }
//}
