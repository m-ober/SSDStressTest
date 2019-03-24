using Mono.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Xml;

namespace SSDStressTest
{
    class Program
    {
        private static BenchmarkWorker worker;
        private static DateTime start_time;
        private static Disk disk;

        private static System.Timers.Timer queryTimer;
        private static Thread workerThread;

        private static string driveLetter = null;
        private static int timeOut = 0;
        private static string outputFile = null;
        private static int howLong = 1;
        private static bool fourBytes = false;
        private static long blocksize_b = 0;
        private static long testsize_b = 0;
        private static List<String> smartParams = new List<String>();
        private static string hdSentinelDriveId = "";
        private static bool disableWmi = false;

        private static StreamWriter logfile;
        private static CultureInfo us = new CultureInfo("en-US");

        private static void queryData(object source, ElapsedEventArgs e)
        {
            if (smartParams.Count > 0 && !disableWmi)
                SmartTools.loadSmartData(disk, !fourBytes);

            StringBuilder logstring = new StringBuilder();
            logstring.Append((DateTime.Now - start_time).TotalSeconds.ToString(us)).Append(",");
            logstring.Append(worker.GetWrittenMBytes().ToString(us)).Append(",");
            logstring.Append(worker.GetPerformance().ToString(us)).Append(",");
            logstring.Append(worker.GetInstPerformance().ToString(us)).Append(",");

            if (smartParams.Count > 0 && !disableWmi)
            {
                for (int i = 0; i < smartParams.Count; i++)
                {
                    logstring.Append(disk.smartData[smartParams[i]]);
                    if (i < smartParams.Count - 1)
                        logstring.Append(",");
                }
            }
            else if (hdSentinelDriveId.Length > 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("HDSentinel.xml");
                XmlNode node = doc.DocumentElement.SelectSingleNode(
                    "/Hard_Disk_Sentinel/Physical_Disk_Information_Disk_" + hdSentinelDriveId +
                    "/Hard_Disk_Summary/Current_Temperature");
                logstring.Append(node.InnerText.Substring(0, node.InnerText.Length - 3));
            } else
            {
                logstring.Append("NO_DATA");
            }
            logstring.Append(Environment.NewLine);

            String s = logstring.ToString();
            logfile.Write(s);
            Console.Write(s);
        }
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine(String.Format("Example: {0} --drive=c --smart=Temperature",
                System.AppDomain.CurrentDomain.FriendlyName));
            Console.WriteLine();
            Console.WriteLine("Help:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static string FormatSize(double bytes)
        {
            string[] suffix = {"Byte", "KByte", "MByte", "GByte"};
            int i = 0;
            for (; i < suffix.Length; i++)
            {
                if (bytes < 1024)
                    break;
                bytes /= 1024;
            }
            return String.Format("{0} {1}", Math.Round(bytes,3).ToString(us), suffix[i]);
        }

        static long ParseSize(string str)
        {
            string suffix = str.Substring(str.Length - 1).ToUpperInvariant();
            long mult = 1;
            long bytes = 0;

            switch (suffix)
            {
                case "G": mult = 1073741824; break;
                case "M": mult = 1048576; break;
                case "K": mult = 1024; break;
            }
            try
            {
                if (mult > 1)
                    bytes = Convert.ToInt64(str.Substring(0, str.Length - 1));
                else
                    bytes = Convert.ToInt64(str);
            }
            catch (Exception e)
            {
                return -1;
            }

            return bytes * mult;
        }

        static bool parseParameters(string[] args)
        {
            bool show_help = false;
            int timeout_default = 2000;
            int limit_default = 1;
            long blocksize_default = ParseSize("16M");
            long testsize_default = ParseSize("512M");
            string blocksize_str = null;
            string testsize_str = null;

            var p = new OptionSet() 
            {
                { "d|drive=", "Drive letter to test (always required)",
                  v => driveLetter = v.ToUpper() },

                { "s|smart:", "SMART value to log / list available values",
                  v => smartParams.Add(v) },

                { "x|xml=", "Use HDSentinel.xml, specify drive Id to use",
                  v => hdSentinelDriveId = v },

                { "w|disablewmi", "Do not query WMI",
                  v => disableWmi = v != null},

                { "4|four", "Interpret SMART values as 4 (not 2) bytes",
                  v => fourBytes = v != null},

                { "i|interval=",
                    String.Format("Timeout between measurements (in ms)\nMust be >= 500 (default {0})",
                    timeout_default),
                  (int v) => timeOut = v },

                { "l|limit=",
                    String.Format("Time in minutes to run the test\n(0 = run until stopped, default {0})",
                    limit_default),
                  (int v) => howLong = v },

                { "b|blocksize=",
                    String.Format("Blocksize in K/M/G Byte (default {0})",
                    FormatSize(blocksize_default)),
                  v => blocksize_str = v },

                { "t|testsize=",
                    String.Format("Testsize in K/M/G Byte (default {0})",
                    FormatSize(testsize_default)),
                  v => testsize_str = v },

                { "o|output=", "Output CSV file name",
                  v =>  outputFile = v},

                { "h|help",  "Show help", 
                  v => show_help = v != null },
            };

            try
            {
                p.Parse(args);
                if (driveLetter == null || driveLetter.Length != 1)
                    show_help = true;
                if (timeOut < 500)
                    timeOut = timeout_default;
                if (howLong < 0)
                    show_help = true;

                if (blocksize_str == null)
                {
                    blocksize_b = blocksize_default;
                }
                else
                {
                    blocksize_b = ParseSize(blocksize_str);
                    if (blocksize_b <= 0)
                    {
                        Console.WriteLine("Invalid blocksize");
                        show_help = true;
                    }
                }

                if (testsize_str == null)
                {
                    testsize_b = testsize_default;
                }
                else
                {
                    testsize_b = ParseSize(testsize_str);
                    if (testsize_b <= 0)
                    {
                        Console.WriteLine("Invalid testsize");
                        show_help = true;
                    }
                }
            }
            catch (OptionException e)
            {
                show_help = true;
            }

            if (disableWmi && smartParams.Count > 0)
            {
                Console.WriteLine("WMI is disabled, cannot read SMART values.");
                Console.WriteLine("Please remove all SMART parameters and/or use the XML option.");
                show_help = true;
            }

            if (!show_help)
            {
                try
                {
                    disk = WmiTools.getDisk(driveLetter);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not open drive " + driveLetter + ":");
                    Console.WriteLine("Error Message: " + e.Message);
                    WmiTools.ListAllDisks();
                    show_help = true;
                }
            }

            if (!show_help && !disableWmi)
            {
                try
                {
                    SmartTools.loadSmartData(disk, !fourBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not query WMI data (run as administrator?)");
                    Console.WriteLine("Error Message: " + e.Message);
                    show_help = true;
                }
            }

            if (!show_help)
            {
                long free;
                long total;
                disk.QuerySpace(out free, out total);
                if (testsize_b > free)
                {
                    Console.WriteLine(
                        String.Format("ERROR: Testsize ({0}) larger than free space on drive ({1})",
                        FormatSize(testsize_b), FormatSize(free)));
                    show_help = true;
                }
            }

            if (!show_help && smartParams.Count > 0)
            {
                foreach (var par in smartParams)
                if (!disk.smartData.ContainsKey(par))
                {
                    Console.WriteLine("ERROR: Smart parameter not found: " + par);
                    Console.WriteLine();
                    smartParams.Clear();
                    break;
                }
            }

            if (!show_help && smartParams.Count == 0 && hdSentinelDriveId.Length == 0 && !disableWmi)
            {
                Console.WriteLine("Listing available SMART parameters");
                Console.WriteLine();
                Console.WriteLine(disk);
                show_help = true;
            }

            if (!show_help)
            {
                if (outputFile == null)
                {
                    int i = 0;
                    do
                    {
                        outputFile = Regex.Replace(disk.productName, @"[^A-Za-z0-9]+", "_")
                            + "_" + i + ".csv";
                        i++;
                    } while (File.Exists(outputFile));
                }
                try
                {
                    logfile = new StreamWriter(outputFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not open output file for writing");
                    Console.WriteLine("Error Message: " + e.Message);
                    show_help = true;
                }
            }

            if (show_help)
            {
                ShowHelp(p);
                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
            }

            return !show_help;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("SSD Stress Tester - https://github.com/m-ober/SSDStressTest");
            Console.WriteLine("USE AT YOUR OWN RISK! THIS TOOL MAY DAMAGE YOUR DRIVE!");
            Console.WriteLine("");

            if (parseParameters(args))
            {
                long free;
                long total;
                disk.QuerySpace(out free, out total);

                StringBuilder header = new StringBuilder();
                header.AppendLine("'Log file name: " + outputFile)
                    .AppendLine("'Logging started on " + DateTime.Now)
                    .AppendLine("'Product name: " + disk.productName)
                    .AppendLine("'Disk PNP ID: " + disk.pnpId)
                    .AppendLine("'Blocksize: " + FormatSize(blocksize_b))
                    .AppendLine("'Testsize: " + FormatSize(testsize_b))
                    .AppendLine("'Free disk space: " + FormatSize(free))
                    .AppendLine("'Total disk space: " + FormatSize(total));

                if (howLong > 0)
                    header.AppendLine("'Running test for " + howLong + " minute(s)");
                else
                    header.AppendLine("'Running test until stopped (press ESC).");

                if (smartParams.Count > 0)
                    header.AppendLine("Time,MBytesWritten,Performance,InstPerformance,"
                        + String.Join(",", smartParams));
                else
                    header.AppendLine("Time,MBytesWritten,Performance,InstPerformance,Temperature");

                string s = header.ToString();
                logfile.Write(s);
                Console.Write(s);
                Console.WriteLine();

                start_time = DateTime.Now;
                queryTimer = new System.Timers.Timer(timeOut);
                queryTimer.Elapsed += new ElapsedEventHandler(queryData);
                queryTimer.Enabled = true;

                worker = new BenchmarkWorker();
                worker.SetDrive(driveLetter);
                worker.SetBlocksize(blocksize_b);
                worker.SetTestSize(testsize_b);
                workerThread = new Thread(worker.DoWork);
                workerThread.Start();
                Console.WriteLine("Starting benchmark...");
                while (!workerThread.IsAlive) ;

                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                        break;

                    if (howLong > 0 && Math.Floor((DateTime.Now - start_time).TotalSeconds) > howLong * 60)
                        break;

                    Thread.Sleep(100);
                }

                worker.RequestStop();
                queryTimer.Enabled = false;
                workerThread.Join();
                Console.WriteLine("Test stopped.");

                logfile.Close();

                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
