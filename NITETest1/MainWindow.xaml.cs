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
using System.Threading;

namespace NITETest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // MEMBER VARIABLES
        
        // Tracking the cursor.
        private NUICursorTracker cursorTracker;

        // Logging the cursor.
        private NUICursorLogger logger;

        // Capturing the depth image.
        private NUIDepthGenerator depthGenerator;

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
        Timer flashTimer;

        public float updateFPS { get; set; }

        private const float DEFAULT_UPDATE_FPS = 60f;


        // METHODS

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateUI()
        {
            DrawCursorAtPosition(cursorTracker.cursorPosition);

            // Log the current cursor location.
            logger.AddPoint(cursorTracker.cursorPosition);

            DrawDebugImage();

            // Checking for clicks and setting background.
            checkForTargetHover();
            if (clickedLeft)
            {
                //background.Source = aquamarinePic;
                FlashTarget(leftTarget);

                // Log the click.
                logger.AddMark("Clicked LEFT target.");

                // Write out the log file.
                logger.WriteOutLog();
            }
            else if (clickedRight)
            {
                //background.Source = archipelagoPic;
                FlashTarget(rightTarget);

                // Log the click.
                logger.AddMark("Clicked RIGHT target.");

                // Write out the log file.
                logger.WriteOutLog();
            }
        }

        private void FlashTarget(Border target)
        {
            target.Background = System.Windows.Media.Brushes.Yellow;
            flashTimer = new Timer(obj => { deFlashTarget(target); }, null, 150, System.Threading.Timeout.Infinite);

        }

        private void deFlashTarget(Border target)
        {
            Dispatcher.Invoke( new Action(() => {
                target.Background = System.Windows.Media.Brushes.Crimson;
            }));
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
                // Log cursor entering target.
                if (framesHovering == 0)
                    logger.AddMark("Entered LEFT target.");
                
                framesHovering++;
                if (framesHovering > updateFPS * hoverTimeThreshold)
                {
                    clickedLeft = true;
                    framesHovering = 1;
                }
            }
            //else if (mainCanvas.InputHitTest(new System.Windows.Point(cursorPosition.X, cursorPosition.Y)) != null &&
            //    mainCanvas.InputHitTest(new System.Windows.Point(cursorPosition.X, cursorPosition.Y)).Equals(rightTarget))
            else if (cursorIsHoveringOnElement(rightTarget))
            {
                // Log cursor entering target.
                if (framesHovering == 0)
                    logger.AddMark("Entered RIGHT target.");

                framesHovering++;
                if (framesHovering > updateFPS * hoverTimeThreshold)
                {
                    clickedRight = true;
                    framesHovering = 1;
                }
            }
            else
            {
                // Log cursor exiting target.
                if (framesHovering > 0)
                    logger.AddMark("Exited target.");
                
                //Console.WriteLine("Not hitting any target!");
                framesHovering = 0;
            }
        }

        private void DrawCursorAtPosition(PointF position)
        {
            //this.cursor.SetValue(Canvas.LeftProperty, position.X - (cursor.Width / 2));
            //this.cursor.SetValue(Canvas.TopProperty, position.Y - (cursor.Height / 2));
            //this.cursorOutline.SetValue(Canvas.LeftProperty, position.X - (cursorOutline.Width / 2));
            //this.cursorOutline.SetValue(Canvas.TopProperty, position.Y - (cursorOutline.Height / 2));

            this.verticalCursor.SetValue(Canvas.LeftProperty, position.X - (verticalCursor.Width / 2));

            //Canvas.SetTop(test, position.Y);
            //Canvas.SetLeft(test, position.X);
        }

        private void DrawDebugImage()
        {
            // Update the depth image.
            depthImage.Source = BitmapImageFromBitmap(depthGenerator.depthImage);

            // Update the position of the debug cursor (displayed on the depth image).
            //depthImageCursor.SetValue(Canvas.LeftProperty, Canvas.GetLeft(depthImage) + cursorTracker.projectedHandPosition.X + 320 - (depthImageCursor.Width / 2));
            //depthImageCursor.SetValue(Canvas.TopProperty, Canvas.GetTop(depthImage) + (cursorTracker.projectedHandPosition.Y * -1) + 240 - (depthImageCursor.Height / 2));
            depthImageCursor.SetValue(Canvas.LeftProperty, Canvas.GetLeft(depthImage) + cursorTracker.projectedHandPosition.X - (depthImageCursor.Width / 2));
            depthImageCursor.SetValue(Canvas.TopProperty, Canvas.GetTop(depthImage) + (cursorTracker.projectedHandPosition.Y) - (depthImageCursor.Height / 2));
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

            // Position the debug image.
            Canvas.SetLeft(depthImage, mainCanvas.Width - depthImage.Width);
            Canvas.SetTop(depthImage, mainCanvas.Height - depthImage.Height);
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

                // Create the cursor logger and open a log file.
                logger = new NUICursorLogger();
                logger.OpenLogFile(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\logs\log1.txt"));

                // Create the depth generator and have it start capturing the depth image.
                depthGenerator = new NUIDepthGenerator();
                depthGenerator.StartGenerating();

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
