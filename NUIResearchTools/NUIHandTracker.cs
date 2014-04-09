using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.IO;
using OpenNI;
using NITE;

namespace NUIResearchTools
{
    public class NUIHandTracker
    {
        // OpenNI stuff.
        private Context context;
        private HandsGenerator handGen;
        private GestureGenerator gestureGen;
        private DepthGenerator depthGen;
        private Point3D _handPosition;

        // NITE stuff.
        //SessionManager sessionManager;
        //FlowRouter flowRouter;
        //PointControl pointControl;

        public Point3D handPosition { get { return _handPosition; } }

        public float updateFPS { get; set; }
        private const float DEFAULT_UPDATE_FPS = 60f;

        public NUIHandTracker()
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

            // Create the depth generator (used for coordinate conversion).
            this.depthGen = context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            if (this.depthGen == null)
                throw new Exception(@"Error in openniconfig.xml. No depth node found.");
            MapOutputMode mapMode = this.depthGen.MapOutputMode;

            // Set up NITE stuff.
            //sessionManager = new SessionManager(context, "Wave", "Wave,RaiseHand");
            //flowRouter = new FlowRouter();
            //pointControl = new PointControl();
            //denoiser = new PointDenoiser();

            //sessionManager.SessionStart += sessionManager_SessionStart;
            ////flowRouter.ActiveListener = pointControl;
            //flowRouter.ActiveListener = denoiser;
            //sessionManager.AddListener(flowRouter);
            //pointControl.PrimaryPointCreate += pointControl_PrimaryPointCreate;
            //pointControl.PrimaryPointUpdate += pointControl_PrimaryPointUpdate;
            //pointControl.PrimaryPointDestroy += pointControl_PrimaryPointDestroy;
            //denoiser.PrimaryPointCreate += pointControl_PrimaryPointCreate;
            //denoiser.PrimaryPointUpdate += pointControl_PrimaryPointUpdate;
            //denoiser.PrimaryPointDestroy += pointControl_PrimaryPointDestroy;

            // Set update FPS to default.
            updateFPS = DEFAULT_UPDATE_FPS;
        }

        //private void sessionManager_SessionStart(object sender, NITE.PositionEventArgs e)
        //{

        //}

        //private void pointControl_PrimaryPointCreate(object sender, NITE.HandFocusEventArgs e)
        //{
        //    Console.WriteLine("Primary Point Created.");
        //    //first time populate the hand co-ordinates
        //    //justBeforeClicked = new Point3D(e.Hand.Position.X, e.Hand.Position.Y, e.Hand.Position.Z);
        //    //pointUpdating = true;
        //}

        //private void pointControl_PrimaryPointUpdate(object sender, NITE.HandEventArgs e)
        //{
        //    _handPosition = depthGen.ConvertRealWorldToProjective(e.Hand.Position);
        //}

        //private void pointControl_PrimaryPointDestroy(object sender, NITE.IdEventArgs e)
        //{
        //    Console.WriteLine("Primary point destroyed.");
        //    //ClassLibrary1.MyGlobals.primaryPoint = false;
        //}

        public void StartTracking()
        {
            gestureGen.StartGenerating();
            handGen.StartGenerating();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(1000f / updateFPS));
            Console.WriteLine("Hand tracker timer interval is {0}", dispatcherTimer.Interval);
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
            _handPosition = depthGen.ConvertRealWorldToProjective(e.Position);
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
                context.WaitAnyUpdateAll();
                //sessionManager.Update(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred while attempting to get update from OpenNI hand generator:");
                Console.WriteLine(ex.Message);
            }
        }

        //private void OnPointCreate(HandPointContext context)
        //{
        //    Console.WriteLine("Created point!");
        //}

        //private void OnPointUpdate(HandPointContext context)
        //{
        //    Console.WriteLine("Updated point!");
        //}

        //private void OnPointDestroy(long nID)
        //{
        //    Console.WriteLine("Destroyed point!");
        //}
    }
}
