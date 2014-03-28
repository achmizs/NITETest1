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
    public class NUIHandGenerator
    {
        private Context context;
        private HandsGenerator handGen;
        private GestureGenerator gestureGen;
        private Point3D _handPosition;

        public Point3D handPosition { get { return _handPosition; } }
        public float updateFPS { get; set; }

        private const float DEFAULT_UPDATE_FPS = 30;

        public NUIHandGenerator()
        {
            // Create a new OpenNI context.
            this.context = new Context(@"..\..\openniconfig.xml");
            //this.context = Context.CreateFromXmlFile(@"openniconfig.xml");

            // Gesture recognition code.
            gestureGen = new GestureGenerator(this.context);
            gestureGen.GestureRecognized += gestureGen_GestureRecognized;
            gestureGen.AddGesture("Wave");

            // Hand tracking code
            handGen = new HandsGenerator(this.context);
            handGen.HandCreate += handGen_HandCreate;
            handGen.HandUpdate += handGen_HandUpdate;
            handGen.HandDestroy += handGen_HandDestroy;

            // Set update FPS to default.
            updateFPS = DEFAULT_UPDATE_FPS;
        }

        public void StartGenerating()
        {
            gestureGen.StartGenerating();
            handGen.StartGenerating();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(1000 / updateFPS));
            dispatcherTimer.Start();
        }

        private void gestureGen_GestureRecognized(object sender, GestureRecognizedEventArgs e)
        {
            Console.WriteLine("Recognized a gesture!");
            handGen.StartTracking(e.EndPosition);
        }

        private void handGen_HandCreate(object sender, HandCreateEventArgs e)
        {
            Console.WriteLine("Created a hand!");
        }

        private void handGen_HandUpdate(object sender, HandUpdateEventArgs e)
        {
            _handPosition = e.Position;
        }

        private void handGen_HandDestroy(object sender, HandDestroyEventArgs e)
        {
            // Nothing here.
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this.context.WaitAnyUpdateAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred while attempting to get update from OpenNI hand generator:");
                Console.WriteLine(ex.Message);
            }

            //Console.WriteLine("{0} {1}", gestureGen.IsGenerating, gestureGen.IsNewDataAvailable);
            //Console.WriteLine("{0}", handGen.IsGenerating);

            //Console.Write("{0}: ", gestureGen.GetAllActiveGestures().Length);
            //for (int i = 0; i < gestureGen.GetAllActiveGestures().Length; i++)
            //{
            //    Console.Write("{0} ", gestureGen.GetAllActiveGestures()[i]);
            //}
            //Console.WriteLine("");


        }

    }
}
