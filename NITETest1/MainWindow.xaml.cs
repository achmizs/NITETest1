﻿using System;
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
using OpenNI;
using NITE;

namespace NITETest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Context context;
        private DepthGenerator depth;
        private Bitmap bitmap;
        private HandsGenerator handGen;
        private GestureGenerator gestureGen;
        private Point3D handPosition;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private unsafe void UpdateDepth()
        {
            //Console.WriteLine("Starting to update depth...");
            
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

            pDest = (byte*)data.Scan0.ToPointer() + ((int)handPosition.Y * -1 + 240) * data.Stride; // 0 should be y
            pDest += 3 * ((int)handPosition.X + 320);
            pDest[0] = 255;
            pDest[1] = 255;
            pDest[2] = 255;

            this.bitmap.UnlockBits(data);

            image1.Source = getBitmapImage(bitmap);

            //Console.WriteLine("Finished updating depth.");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.context = new Context(@"..\..\openniconfig.xml");
                //this.context = Context.CreateFromXmlFile(@"openniconfig.xml");

                this.depth = context.FindExistingNode(NodeType.Depth) as DepthGenerator;
                if (this.depth == null)
                    throw new Exception(@"Error in openniconfig.xml. No depth node found.");
                MapOutputMode mapMode = this.depth.MapOutputMode;
                this.bitmap = new Bitmap((int)mapMode.XRes, (int)mapMode.YRes, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Console.WriteLine("{0}, {1}", bitmap.Width, bitmap.Height);

                gestureGen = new GestureGenerator(this.context);
                //gestureGen.GestureRecognized += new EventHandler<GestureRecognizedEventArgs>(gestureGen_GestureRecognized);
                gestureGen.GestureRecognized += gestureGen_GestureRecognized;
                Console.Write("{0}: ", gestureGen.NumberOfEnumeratedGestures);
                for (int i = 0; i < gestureGen.NumberOfEnumeratedGestures; i++)
                {
                    Console.Write("{0} ", gestureGen.EnumerateAllGestures()[i]);
                }
                Console.WriteLine("");

                Console.WriteLine("{0}", gestureGen.IsGestureAvailable("Wave"));

                Console.Write("{0}: ", gestureGen.GetAllActiveGestures().Length);
                for (int i = 0; i < gestureGen.GetAllActiveGestures().Length; i++)
                {
                    Console.Write("{0} ", gestureGen.GetAllActiveGestures()[i]);
                }
                Console.WriteLine("");
                gestureGen.AddGesture("Wave");
                Console.Write("{0}: ", gestureGen.GetAllActiveGestures().Length);
                for (int i = 0; i < gestureGen.GetAllActiveGestures().Length; i++)
                {
                    Console.Write("{0} ", gestureGen.GetAllActiveGestures()[i]);
                }
                Console.WriteLine("");
                //Console.WriteLine("{0}", gestureGen.IsGenerating);
                //gestureGen.StartGenerating();
                //Console.WriteLine("{0}", gestureGen.IsGenerating);

                // Hand tracking code
                handGen = new HandsGenerator(this.context);
                //handGen.HandCreate += new EventHandler<HandCreateEventArgs>(handGen_HandCreate);
                handGen.HandCreate += handGen_HandCreate;
                handGen.HandUpdate += handGen_HandUpdate;
                Console.WriteLine("Is handGen generating? {0}", handGen.IsGenerating);
                handGen.StartGenerating();
                Console.WriteLine("Is handGen generating? {0}", handGen.IsGenerating);
          }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing OpenNI.\n" + ex.Message);
                //MessageBox.Show(ex.Message);
                this.Close();
            }

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();
            Console.WriteLine("Finished loading.");
        }

        void gestureGen_GestureRecognized(object sender, GestureRecognizedEventArgs e)
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
            Console.WriteLine("{0}, {1}, {2}", e.Position.X, e.Position.Y, e.Position.Z);
            handPosition = e.Position;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this.context.WaitAnyUpdateAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            UpdateDepth();

            //Console.WriteLine("{0} {1}", gestureGen.IsGenerating, gestureGen.IsNewDataAvailable);
            //Console.WriteLine("{0}", handGen.IsGenerating);
 
            //Console.Write("{0}: ", gestureGen.GetAllActiveGestures().Length);
            //for (int i = 0; i < gestureGen.GetAllActiveGestures().Length; i++)
            //{
            //    Console.Write("{0} ", gestureGen.GetAllActiveGestures()[i]);
            //}
            //Console.WriteLine("");

            
        }

        public static BitmapImage getBitmapImage(Bitmap bitmap)
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
