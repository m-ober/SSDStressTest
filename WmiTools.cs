using System;
using System.Management;

namespace SSDStressTest
{
    class WmiTools
    {   
        private static String GetPartName(String inp)
        {
            String Dependent = "", ret = "";
            ManagementObjectSearcher LogicalDisk = new ManagementObjectSearcher("Select * from Win32_LogicalDiskToPartition");
            foreach (ManagementObject drive in LogicalDisk.Get())
            {
                if (drive["Antecedent"].ToString().Contains(inp))
                {
                    Dependent = drive["Dependent"].ToString();
                    // \\PC\root\cimv2:Win32_LogicalDisk.DeviceID="T:"
                    ret = Dependent.Substring(Dependent.Length - 3, 2);
                    break;
                }

            }
            return ret;
        }

        public static void ListAllDisks()
        {
            String DiskName = "";
            String PartState = "";
            String PartName = "";
            String DriveLetter = "";

            ManagementObjectSearcher hdd = new ManagementObjectSearcher("Select * from Win32_DiskDrive");

            foreach (ManagementObject objhdd in hdd.Get())
            {
                PartState = "";
                DiskName = "Disk " + objhdd["Index"].ToString() + ": " + objhdd["Caption"].ToString().Replace(" ATA Device", "") +
                    " (" + Math.Round(Convert.ToDouble(objhdd["Size"]) / 1073741824, 1) + " GB)";

                Console.WriteLine(DiskName);

                var ObjCount = Convert.ToInt16(objhdd["Partitions"]);

                ManagementObjectSearcher partitions = new ManagementObjectSearcher(
                    "Select * From Win32_DiskPartition Where DiskIndex='" + objhdd["Index"].ToString() + "'");
                foreach (ManagementObject part in partitions.Get())
                {
                    PartName = part["DeviceID"].ToString();
                    DriveLetter = GetPartName(PartName);

                    if (DriveLetter == "")
                        PartState = "No drive letter";
                    else PartState = "Local disk " + DriveLetter;

                    Console.WriteLine("    Partition " + part["Index"].ToString() + " " + PartState);
                }
                Console.WriteLine();
            }

        }

        public static Disk getDisk(string driveLetter)
        {
            var disks = new ManagementObject("Win32_LogicalDisk.DeviceID='" + driveLetter + ":'");
            foreach (ManagementObject diskPart in disks.GetRelated("Win32_DiskPartition"))
            {
                foreach (ManagementObject diskDrive in diskPart.GetRelated("Win32_DiskDrive"))
                {
                    Disk disk = new Disk();
                    disk.driveLetter = driveLetter;
                    disk.productName = diskDrive["Caption"].ToString().Replace(" ATA Device", "");
                    disk.pnpId = diskDrive["PnPDeviceID"].ToString();
                    return disk;
                }
            }
            return null;
        }

    }
}
