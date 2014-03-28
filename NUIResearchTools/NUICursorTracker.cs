using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using NUICursorTools;

namespace NUIResearchTools
{
    public class NUICursorTracker
    {
        // Tracking the hand.
        private NUIHandTracker handTracker;

        // Generating the cursor position.
        private PointF projectedHandPosition;
        private NUICursorShaper shaper;
        private PointF _cursorPosition;
        public PointF cursorPosition { get { return _cursorPosition; } }
        public float updateFPS { get; set; }

        private const float DEFAULT_UPDATE_FPS = 60f;

        public NUICursorTracker()
        {
            // Create the hand tracker.
            handTracker = new NUIHandTracker();

            // Set up cursor shaping.
            shaper = new NUICursorShaper();
            NUICursorSpaceTransform space = new NUICursorSpaceTransform(new RectangleF(-320, -240, 640, 480), new RectangleF(0, 0, (float)System.Windows.SystemParameters.PrimaryScreenWidth, (float)System.Windows.SystemParameters.PrimaryScreenHeight), false, true, NUICursorSpaceTransform.NUI_CURSOR_SPACE_TRANSFORM_MODE.FILL);
            shaper.addTransform(space);
            NUICursorJitterTransform jitter = new NUICursorJitterTransform();
            shaper.addTransform(jitter);

            // Set update FPS to default.
            updateFPS = DEFAULT_UPDATE_FPS;
        }

        public void StartTracking()
        {
            handTracker.StartTracking();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int) (1000f / updateFPS));
            Console.WriteLine("Cursor tracker timer interval is {0}", dispatcherTimer.Interval);
            dispatcherTimer.Start();
        }

        private void UpdateCursor()
        {
            // Updating the cursor.
            projectedHandPosition = new PointF(handTracker.handPosition.X, handTracker.handPosition.Y);
            _cursorPosition = shaper.shape(projectedHandPosition);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateCursor();
        }
    }
}
