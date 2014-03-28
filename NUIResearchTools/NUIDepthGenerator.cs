using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.IO;
using OpenNI;

namespace NUIResearchTools
{
    public class NUIDepthGenerator
    {
        private Context context;
        private DepthGenerator depth;
        private Bitmap bitmap;

        public Bitmap depthImage { get { return bitmap; } }
        public float updateFPS { get; set; }

        private static const float DEFAULT_UPDATE_FPS = 30;

        public NUIDepthGenerator()
        {
            // Create an OpenNI context.
            this.context = new Context(@"..\..\openniconfig.xml");
            //this.context = Context.CreateFromXmlFile(@"openniconfig.xml");

            // Create the depth generator.
            this.depth = context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            if (this.depth == null)
                throw new Exception(@"Error in openniconfig.xml. No depth node found.");
            MapOutputMode mapMode = this.depth.MapOutputMode;
            
            // Create a bitmap image.
            this.bitmap = new Bitmap((int)mapMode.XRes, (int)mapMode.YRes, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Set update FPS to default.
            updateFPS = DEFAULT_UPDATE_FPS;
        }

        public void StartGenerating()
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this.context.WaitAnyUpdateAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred while attempting to get update from OpenNI depth generator:");
                Console.WriteLine(ex.Message);
            }

            UpdateDepth();
        }

        private unsafe void UpdateDepth()
        {
            DepthMetaData depthMD = new DepthMetaData();

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, this.bitmap.Width, this.bitmap.Height);
            BitmapData data = this.bitmap.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            depth.GetMetaData(depthMD);

            //Console.WriteLine("{0}, {1}", depthMD.XRes, depthMD.YRes);

            ushort* pDepth = (ushort*)this.depth.DepthMapPtr.ToPointer();

            byte* pDest;

            for (int y = 0; y < depthMD.YRes; y++)
            {
                pDest = (byte*)data.Scan0.ToPointer() + y * data.Stride;
                for (int x = 0; x < depthMD.XRes; x++, pDepth++, pDest += 3)
                {
                    pDest[0] = (byte)(*pDepth >> 2);
                    pDest[1] = (byte)(*pDepth >> 3);
                    pDest[2] = (byte)(*pDepth >> 4);
                }
            }

            //pDest = (byte*)data.Scan0.ToPointer() + ((int)handPosition.Y * -1 + 240) * data.Stride; // 0 should be y
            //pDest += 3 * ((int)handPosition.X + 320);
            //pDest[0] = 255;
            //pDest[1] = 255;
            //pDest[2] = 255;

            this.bitmap.UnlockBits(data);

            //image1.Source = getBitmapImage(bitmap);

        }
    }
}
