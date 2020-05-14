// Source: https://stackoverflow.com/questions/14442960/getting-drive-info-from-a-remote-computer

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\lld.exe

using System;
using System.Management;

class ListLogicalDrives {
    public static int PrintUsage() {
        Console.WriteLine(@"List logical drives on local or remote host

USAGE:
    lld [system]");
        Console.WriteLine("\nDONE");
        return 1;
    }
    
    public static int Main(string[] args) {
        string[] DRIVE_TYPES = {"Unknown", "No Root Directory", "Removable Disk", "Local Disk", "Network Drive", "Compact Disc", "RAM Disk"};
        
        string system = ".";
        
        foreach (string arg in args) {
            switch (arg.ToUpper()) {
                case "/?":
                    return PrintUsage();
                default:
                    system = args[0].Trim(new Char[] {'\\', ' '});
                    break;
            }
        }
        
        ManagementPath path = new ManagementPath() {
                NamespacePath = @"root\cimv2",
                Server = system
        };
        ManagementScope scope = new ManagementScope(path);
        
        string[] selectedProperties = new string[] { "DeviceID", "DriveType", "ProviderName", "FreeSpace", "Size", "VolumeName"};
        SelectQuery query = new SelectQuery("Win32_LogicalDisk", "", selectedProperties);

        try {
            // Execute query within scope and iterate through results
            using (var searcher = new ManagementObjectSearcher(scope, query)) {
                foreach (ManagementObject volume in searcher.Get()) {
                    Console.WriteLine("\nDrive " + volume.GetPropertyValue("DeviceID") + "\\");
                    
                    if (String.Format("{0}", volume.GetPropertyValue("VolumeName")) != "") {
                        Console.WriteLine("  Name:       {0}", volume.GetPropertyValue("VolumeName"));
                    }
                    
                    Console.WriteLine("  Type:       {0}", DRIVE_TYPES[Convert.ToInt32(volume.GetPropertyValue("DriveType"))]);
                    
                    if (Convert.ToInt32(volume.GetPropertyValue("DriveType")) == 4) {
                        Console.WriteLine("  Provider:   {0}", volume.GetPropertyValue("ProviderName"));
                    }
                    
                    if (Convert.ToInt64(volume.GetPropertyValue("Size")) > 0) {
                        Console.WriteLine("  Free Space: {0}", String.Format("{0:n0}", Convert.ToInt64(volume.GetPropertyValue("FreeSpace"))) + " bytes");
                        Console.WriteLine("  Size:       {0}", String.Format("{0:n0}", Convert.ToInt64(volume.GetPropertyValue("Size"))) + " bytes");
                    }
                }
            }
            
            Console.WriteLine("\nDONE");
            return 0;
        } catch (Exception e) {
            Console.WriteLine("ERROR: " + e.Message);
            Console.WriteLine("\nDONE");
            return 1;
        }
    }
}