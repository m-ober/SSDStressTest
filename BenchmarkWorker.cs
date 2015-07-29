using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SSDStressTest
{
    class BenchmarkWorker
    {
        private volatile bool stopWork = false;

        private long blockswritten = 0;
        private long inst_blockswritten = 0;
        private double prev_measurement = 0;
        private double last_measurement = 0;
        private double inst_measurement = -1;

        private double performance = 0;
        private double inst_performance = 0;

        private const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        private const uint FILE_WRITE_THROUGH = 0x80000000;
        private const uint flags = FILE_FLAG_NO_BUFFERING | FILE_WRITE_THROUGH;

        private string filename;
        private long blocksize = 16 * 1048576; // MByte * Bytes
        private long numblocks = 32; // numblocks * blocksize = max. size of testfile

        private object _lock = new object();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr SecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        public void DoWork()
        {
            byte[] randomdata = new byte[blocksize];
            new Random().NextBytes(randomdata);

            SafeFileHandle filehandle = CreateFile(filename, (uint)FileAccess.Write, (uint)FileShare.None,
                            IntPtr.Zero, (uint)FileMode.OpenOrCreate, flags, IntPtr.Zero);

            if (!filehandle.IsInvalid)
            {
                Console.WriteLine("Benchmark worker thread started.");
                if (Stopwatch.IsHighResolution)
                    Console.WriteLine("Using high resolution timer.");
                Console.WriteLine();

                var filestream = new FileStream(filehandle, FileAccess.ReadWrite,
                    Convert.ToInt32(blocksize), false);
                var timer = new Stopwatch();
                timer.Start();

                float freq = Stopwatch.Frequency;

                int blockindex = 1;
                while (!this.stopWork)
                {
                    prev_measurement = timer.ElapsedTicks / freq;
                    filestream.Write(randomdata, 0, Convert.ToInt32(blocksize));
                    filestream.Flush();
                    last_measurement = timer.ElapsedTicks / freq;

                    lock (_lock)
                    {
                        blockswritten++;
                        inst_blockswritten++;

                        if (inst_measurement == -1)
                        {
                            inst_measurement = prev_measurement;
                            inst_blockswritten = 1;
                        }

                        performance = (blocksize * blockswritten / 1048576.0) / last_measurement;
                        inst_performance = (blocksize * inst_blockswritten / 1048576.0) / (last_measurement - inst_measurement);
                    }

                    if (blockindex++ == numblocks)
                    {
                        blockindex = 1;
                        filestream.Seek(0, SeekOrigin.Begin);
                    }
                }
                timer.Stop();
                filestream.Flush();
                filestream.Close();
                File.Delete(filename);
            }
            else
            {
                Console.WriteLine("Could not create testfile");
            }

        }

        public void setDrive(string driveLetter)
        {
            this.filename = driveLetter + @":\evndnj9e19t7ef9mexd3.dat";
        }
        public void RequestStop()
        {
            Console.WriteLine("Shutting down worker thread...");
            this.stopWork = true;
        }

        public double GetPerformance()
        {
            lock (_lock)
            {
                return Math.Round(this.performance, 2);
            }
        }
        public double GetInstPerformance()
        {
            lock (_lock)
            {
                this.inst_measurement = -1;
                return Math.Round(this.inst_performance, 2);
            }
        }

        public double GetWrittenMBytes()
        {
            lock (_lock)
            {
                return blocksize * blockswritten / 1048576.0;
            }
        }
    }
}
