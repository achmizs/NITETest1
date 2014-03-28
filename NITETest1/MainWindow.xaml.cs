using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.IO;
using NUIResearchTools;

namespace NITETest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Tracking the cursor.
        private NUICursorTracker cursorTracker;

        // Background images.
        private BitmapImage windowsBackground;
        private BitmapImage aquamarinePic;
        private BitmapImage archipelagoPic;
        private BitmapImage deepspacePic;
        private BitmapImage mountainsPic;
        private BitmapImage spacePic;
        private BitmapImage starwarsPic;

        private List<BitmapImage> backgroundPics;

        // Stuff for the cursor hitting the target.
        int framesHovering;
        static float hoverTimeThreshold = 0.5f;
        bool clickedLeft;
        bool clickedRight;

        public float updateFPS { get; set; }

        private const float DEFAULT_UPDATE_FPS = 60f;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateUI()
        {
            DrawCursorAtPosition(cursorTracker.cursorPosition);

            // Checking for clicks and setting background.
            checkForTargetHover();
            if (clickedLeft)
            {
                background.Source = aquamarinePic;
            }
            else if (clickedRight)
            {
                background.Source = archipelagoPic;
            }
        }

        private bool cursorIsHoveringOnElement(FrameworkElement element)
        {
            // Get coordinates of element
            System.Windows.Point elementOrigin = element.TransformToAncestor(Application.Current.MainWindow).Transform(new System.Windows.Point(0, 0));
            // Get rectangle circumscribing element
            RectangleF elementRect = new RectangleF((float)elementOrigin.X, (float)elementOrigin.Y, (float)element.Width, (float)element.Height);

            return elementRect.Contains(cursorTracker.cursorPosition);
        }

        private void checkForTargetHover()
        {
            clickedLeft = false;
            clickedRight = false;

            //if (mainCanvas.InputHitTest(new System.Windows.Point(cursorPosition.X, cursorPosition.Y)) != null &&
            //    mainCanvas.InputHitTest(new System.Windows.Point(cursorPosition.X, cursorPosition.Y)).Equals(leftTarget))
            if(cursorIsHoveringOnElement(leftTarget))
            {
                //Console.WriteLine("Hitting left target!");
                framesHovering++;
                if (framesHovering > updateFPS * hoverTimeThreshold)
                    clickedLeft = true;
            }
            //else if (mainCanvas.InputHitTest(new System.Windows.Point(cursorPosition.X, cursorPosition.Y)) != null &&
            //    mainCanvas.InputHitTest(new System.Windows.Point(cursorPosition.X, cursorPosition.Y)).Equals(rightTarget))
            else if (cursorIsHoveringOnElement(rightTarget))
            {
                //Console.WriteLine("Hitting right target!");
                framesHovering++;
                if (framesHovering > updateFPS * hoverTimeThreshold)
                    clickedRight = true;
            }
            else
            {
                //Console.WriteLine("Not hitting any target!");
                framesHovering = 0;
            }
        }

        private void DrawCursorAtPosition(PointF position)
        {
            this.cursor.SetValue(Canvas.LeftProperty, position.X - (cursor.Width / 2));
            this.cursor.SetValue(Canvas.TopProperty, position.Y - (cursor.Height / 2));
            this.cursorOutline.SetValue(Canvas.LeftProperty, position.X - (cursorOutline.Width / 2));
            this.cursorOutline.SetValue(Canvas.TopProperty, position.Y - (cursorOutline.Height / 2));

            //this.verticalCursor.SetValue(Canvas.LeftProperty, position.X - (verticalCursor.Width / 2));

            //Canvas.SetTop(test, position.Y);
            //Canvas.SetLeft(test, position.X);
        }

        private void SetupUI()
        {
            Console.WriteLine("Screen dimensions: {0} x {1}", System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight);

            // Setting up the UI.
            mainCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            mainCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            background.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            background.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            windowsBackground = new BitmapImage(new Uri("images/windows_background_1920_1080.jpg", UriKind.Relative));
            aquamarinePic = new BitmapImage(new Uri("images/aquamarine2x.jpg", UriKind.Relative));
            archipelagoPic = new BitmapImage(new Uri("images/archipelago2x.jpg", UriKind.Relative));
            deepspacePic = new BitmapImage(new Uri("images/deepspace.jpg", UriKind.Relative));
            mountainsPic = new BitmapImage(new Uri("images/mountains.jpg", UriKind.Relative));
            spacePic = new BitmapImage(new Uri("images/space.jpg", UriKind.Relative));
            starwarsPic = new BitmapImage(new Uri("images/starwars.jpg", UriKind.Relative));

            backgroundPics = new List<BitmapImage>();
            backgroundPics.Add(windowsBackground);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set up the UI.
                SetupUI();

                // Set update FPS to default.
                updateFPS = DEFAULT_UPDATE_FPS;

                // Create the cursor tracker and have it start tracking.
                cursorTracker = new NUICursorTracker();
                cursorTracker.StartTracking();

                // Hover detection stuff
                framesHovering = 0;
                clickedLeft = false;
                clickedRight = false;

                // Start the screen redraw run loop.
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(1000f / updateFPS));
                dispatcherTimer.Start();

                Console.WriteLine("Finished loading.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred!\n" + ex.Message);
                //MessageBox.Show(ex.Message);
                this.Close();
            }

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        public static BitmapImage BitmapImageFromBitmap(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }
   }
}
