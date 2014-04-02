using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace NUIResearchTools
{
    public class NUICursorLogger
    {
        // MEMBER DATA

        // Public
        public bool logTimestamps { get; set; }
        public bool writeOutOnClose { get; set; }

        // Private
        private List<Dictionary<string, object>> log;
        private StreamWriter logFile;
        private Stopwatch timer;

        // CONSTRUCTORS
        
        public NUICursorLogger()
        {
            init();
        }

        public NUICursorLogger(string logFileName)
        {
            init();

            OpenLogFile(logFileName);
        }


        // METHODS

        private void init()
        {
            log = new List<Dictionary<string, object>>();

            timer = new Stopwatch();
            timer.Start();

            logTimestamps = true;
            writeOutOnClose = false;
        }

        public void OpenLogFile(string logFileName)
        {
            logFile = new StreamWriter(logFileName, true);

            AddMark("Log opened.");
            if (Stopwatch.IsHighResolution)
                AddMark(String.Format("Using high-resolution timer ({0} ns).", (1000L * 1000L * 1000L) / Stopwatch.Frequency));
            else
                AddMark("High-resolution timer not available.");
        }

        public void AddPoint(PointF point)
        {
            Dictionary<string, object> logEntry = new Dictionary<string, object>();

            logEntry["timestamp"] = timer.Elapsed.TotalMilliseconds.ToString("0.0");
            logEntry["type"] = "point";
            logEntry["value"] = point;

            log.Add(logEntry);
        }

        public void AddPointPair(PointF pointA, PointF pointB)
        {
            Dictionary<string, object> logEntry = new Dictionary<string, object>();

            logEntry["timestamp"] = timer.Elapsed.TotalMilliseconds.ToString("0.0");
            logEntry["type"] = "pointPair";
            logEntry["value1"] = pointA;
            logEntry["value2"] = pointB;

            log.Add(logEntry);
        }

        public void AddMark(string markString)
        {
            Dictionary<string, object> logEntry = new Dictionary<string, object>();

            logEntry["timestamp"] = timer.Elapsed.TotalMilliseconds.ToString("0.0");
            logEntry["type"] = "mark";
            logEntry["markString"] = markString;

            log.Add(logEntry);
        }

        public void ClearLog()
        {
            log.Clear();
        }

        public void WriteOutLog()
        {
            PointF point;
            
            foreach (Dictionary<string, object> entry in log)
            {
                // If timestamp logging is enabled, log the timestamp.
                if (logTimestamps)
                {
                    logFile.Write("[{0}] ", entry["timestamp"]);
                }

                // Depending on the type of entry this is, log the value(s), in the appropriate format.
                if (entry["type"].Equals("mark"))
                {
                    logFile.Write("* {0}\n", entry["markString"]);
                }
                else if (entry["type"].Equals("point"))
                {
                    point = (PointF)entry["value"];
                    logFile.Write("{0} {1}\n", point.X, point.Y);
                }
                else if (entry["type"].Equals("pointPair"))
                {
                    point = (PointF)entry["value1"];
                    logFile.Write("{0} {1}\t", point.X, point.Y);

                    point = (PointF)entry["value2"];
                    logFile.Write("{0} {1}\n", point.X, point.Y);
                }
            }

            ClearLog();
        }

        public void CloseLogFile()
        {
            if (writeOutOnClose == false)
                ClearLog();

            AddMark("Log closed.");
            WriteOutLog();

            logFile.Close();
        }
    }
}
