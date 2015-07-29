using Mono.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

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
        private static List<String> smartParams = new List<String>();

        private static StreamWriter logfile;
        private static CultureInfo us = new CultureInfo("en-US");

        private static void queryData(object source, ElapsedEventArgs e)
        {
            SmartTools.loadSmartData(disk, !fourBytes);
            StringBuilder logstring = new StringBuilder();

            logstring.Append((e.SignalTime - start_time).TotalSeconds.ToString(us)).Append(",");
            logstring.Append(worker.GetWrittenMBytes().ToString(us)).Append(",");
            logstring.Append(worker.GetPerformance().ToString(us)).Append(",");
            logstring.Append(worker.GetInstPerformance().ToString(us)).Append(",");
            for (int i = 0; i < smartParams.Count; i++)
            {
                logstring.Append(disk.smartData[smartParams[i]]);
                if (i < smartParams.Count - 1)
                    logstring.Append(",");
            }
            logstring.Append(Environment.NewLine);

            String s = logstring.ToString();
            logfile.Write(s);
            Console.Write(s);
        }
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Help:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static bool parseParameters(string[] args)
        {
            bool show_help = false;

            var p = new OptionSet() 
            {
                { "d|drive=", "Drive letter to test (REQUIRED)",
                  v => driveLetter = v.ToUpper() },
                { "s|smart:", "SMART value to log (REQUIRED)\nWithout value, available values are listed",
                  v => smartParams.Add(v) },
                { "4|four", "Interpret SMART values as 4 bytes\n(otherwise 2, default)",
                  v => fourBytes = v != null},
                { "t|timeout=", "Timeout between measurements (in ms)\nMust be >= 500 (default 2000)",
                  (int v) => timeOut = v },
                { "l|limit=", "Time in minutes to run the test\nZero means indefinite (default 1)",
                  (int v) => howLong = v },
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
                    timeOut = 2000;
                if (howLong < 0)
                    show_help = true;
            }
            catch (OptionException e)
            {
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
                    //WmiTools.ListAllDisks();
                    show_help = true;
                }
            }

            if (!show_help)
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

            if (!show_help && smartParams.Count > 0)
            {
                foreach (var par in smartParams)
                if (!disk.smartData.ContainsKey(par))
                {
                    Console.WriteLine("Error: Smart parameter not found: " + par);
                    Console.WriteLine();
                    smartParams.Clear();
                    break;
                }
            }

            if (!show_help && smartParams.Count == 0)
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
                        outputFile = Regex.Replace(disk.productName, @"[^A-Za-z0-9]+", "_") + "_" + i + ".csv";
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
                StringBuilder header = new StringBuilder();
                header.AppendLine("'Log file name: " + outputFile);
                header.AppendLine("'Logging started on " + DateTime.Now);
                header.AppendLine("'Product name: " + disk.productName);
                header.AppendLine("'Disk PNP ID: " + disk.pnpId);
                if (howLong > 0)
                    header.AppendLine("'Running test for " + howLong + " minute(s)");
                else
                    header.AppendLine("'Running test until stopped (press ESC).");
                header.AppendLine("Time,MBytesWritten,Performance,InstPerformance,"
                    + String.Join(",", smartParams));

                string s = header.ToString();
                logfile.Write(s);
                Console.Write(s);
                Console.WriteLine();

                start_time = DateTime.Now;
                queryTimer = new System.Timers.Timer(timeOut);
                queryTimer.Elapsed += new ElapsedEventHandler(queryData);
                queryTimer.Enabled = true;

                worker = new BenchmarkWorker();
                worker.setDrive(driveLetter);
                workerThread = new Thread(worker.DoWork);
                workerThread.Start();
                Console.WriteLine("Main thread: Starting worker thread...");
                while (!workerThread.IsAlive) ;

                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                        break;

                    if (howLong > 0 && (DateTime.Now - start_time).TotalMinutes > howLong)
                        break;

                    Thread.Sleep(100);
                }

                worker.RequestStop();
                queryTimer.Enabled = false;
                workerThread.Join();
                Console.WriteLine("Main thread: Worker thread has terminated.");

                logfile.Close();

                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
