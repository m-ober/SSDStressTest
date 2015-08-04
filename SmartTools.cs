// (c) Microsoft Corporation
// Author: Clemens Vasters (clemensv@microsoft.com)
// Code subject to MS-PL: http://opensource.org/licenses/ms-pl.html 
// SMART Attributes and Background: http://en.wikipedia.org/wiki/S.M.A.R.T.
// SMART Attributes Overview: http://www.t13.org/Documents/UploadedDocuments/docs2005/e05171r0-ACS-SMARTAttributes_Overview.pdf

using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

namespace SSDStressTest
{

    public enum SmartAttributeType : byte
    {
        ReadErrorRate = 0x01,
        ThroughputPerformance = 0x02,
        SpinUpTime = 0x03,
        StartStopCount = 0x04,
        ReallocatedSectorsCount = 0x05,
        ReadChannelMargin = 0x06,
        SeekErrorRate = 0x07,
        SeekTimePerformance = 0x08,
        PowerOnHoursPOH = 0x09,
        SpinRetryCount = 0x0A,
        CalibrationRetryCount = 0x0B,
        PowerCycleCount = 0x0C,
        SoftReadErrorRate = 0x0D,
        SATADownshiftErrorCount = 0xB7,
        EndtoEnderror = 0xB8,
        HeadStability = 0xB9,
        InducedOpVibrationDetection = 0xBA,
        ReportedUncorrectableErrors = 0xBB,
        CommandTimeout = 0xBC,
        HighFlyWrites = 0xBD,
        AirflowTemperatureWDC = 0xBE,
        TemperatureDifferencefrom100 = 0xBE,
        GSenseErrorRate = 0xBF,
        PoweroffRetractCount = 0xC0,
        LoadCycleCount = 0xC1,
        Temperature = 0xC2,
        HardwareECCRecovered = 0xC3,
        ReallocationEventCount = 0xC4,
        CurrentPendingSectorCount = 0xC5,
        UncorrectableSectorCount = 0xC6,
        UltraDMACRCErrorCount = 0xC7,
        MultiZoneErrorRate = 0xC8,
        WriteErrorRateFujitsu = 0xC8,
        OffTrackSoftReadErrorRate = 0xC9,
        DataAddressMarkerrors = 0xCA,
        RunOutCancel = 0xCB,
        SoftECCCorrection = 0xCC,
        ThermalAsperityRateTAR = 0xCD,
        FlyingHeight = 0xCE,
        SpinHighCurrent = 0xCF,
        SpinBuzz = 0xD0,
        OfflineSeekPerformance = 0xD1,
        VibrationDuringWrite = 0xD3,
        ShockDuringWrite = 0xD4,
        DiskShift = 0xDC,
        GSenseErrorRateAlt = 0xDD,
        LoadedHours = 0xDE,
        LoadUnloadRetryCount = 0xDF,
        LoadFriction = 0xE0,
        LoadUnloadCycleCount = 0xE1,
        LoadInTime = 0xE2,
        TorqueAmplificationCount = 0xE3,
        PowerOffRetractCycle = 0xE4,
        GMRHeadAmplitude = 0xE6,
        DriveTemperature = 0xE7,
        HeadFlyingHours = 0xF0,
        TransferErrorRateFujitsu = 0xF0,
        TotalLBAsWritten = 0xF1,
        TotalLBAsRead = 0xF2,
        ReadErrorRetryRate = 0xFA,
        FreeFallProtection = 0xFE,
    }

    public class SmartData
    {
        readonly Dictionary<SmartAttributeType, SmartAttribute> attributes;
        readonly ushort structureVersion;

        public SmartData(byte[] arrVendorSpecific)
        {
            attributes = new Dictionary<SmartAttributeType, SmartAttribute>();
            for (int offset = 2; offset < arrVendorSpecific.Length; )
            {
                var a = FromBytes<SmartAttribute>(arrVendorSpecific, ref offset, 12);
                // Attribute values 0x00, 0xfe, 0xff are invalid
                if (a.AttributeType != 0x00 && (byte)a.AttributeType != 0xfe &&
                    (byte)a.AttributeType != 0xff)
                {
                    attributes[a.AttributeType] = a;
                }
            }
            structureVersion = (ushort)(arrVendorSpecific[0] * 256 + arrVendorSpecific[1]);
        }

        public ushort StructureVersion
        {
            get
            {
                return this.structureVersion;
            }
        }

        public SmartAttribute this[SmartAttributeType v]
        {
            get
            {
                return this.attributes[v];
            }
        }

        public IEnumerable<SmartAttribute> Attributes
        {
            get
            {
                return this.attributes.Values;
            }
        }

        static T FromBytes<T>(byte[] bytearray, ref int offset, int count)
        {
            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.AllocHGlobal(count);
                Marshal.Copy(bytearray, offset, ptr, count);
                offset += count;
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SmartAttribute
    {
        public SmartAttributeType AttributeType;
        public ushort Flags;
        public byte Value;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] VendorData;

        public bool Advisory
        {
            get
            {
                return (Flags & 0x1) == 0x0; // Bit 0 unset?
            }
        }
        public bool FailureImminent
        {
            get
            {
                return (Flags & 0x1) == 0x1; // Bit 0 set?
            }
        }
        public bool OnlineDataCollection
        {
            get
            {
                return (Flags & 0x2) == 0x2; // Bit 0 set?
            }
        }

    }

    public class SmartTools
    {
        public static void loadSmartData(Disk disk, bool twoByteValues)
        {
            var result = new Dictionary<string, int>();

            var searcher = new ManagementObjectSearcher(
                "root\\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData  WHERE InstanceName='" +
                disk.pnpId.Replace("\\", "\\\\") + "_0" + "'");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                var arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");

                // Create SMART data from 'vendor specific' array
                var d = new SmartData(arrVendorSpecific);
                foreach (var b in d.Attributes)
                {
                    if (twoByteValues)
                    {
                        byte[] data = { b.VendorData[0], b.VendorData[1] };
                        var decVal = BitConverter.ToInt16(data, 0);
                        result.Add(b.AttributeType.ToString(), decVal);
                    }
                    else
                    {
                        var decVal = BitConverter.ToInt32(b.VendorData, 0);
                        result.Add(b.AttributeType.ToString(), decVal);
                    }
                }

            }

            disk.smartData = result;
        }
    }
}