// Source: https://docs.microsoft.com/en-us/dotnet/api/system.io.driveinfo.availablefreespace

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:freespace.exe freespace.cs


using System;
using System.IO;

class FreeSpace
{
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "/?")
            {
                Console.WriteLine(@"Lists logical drives, including total and available free space. Mapped drives will only be shown when freespace.exe is run within the same session or with the same credentials used to map the drive.
    
USAGE:
    freespace.exe [/?]");
                return;
            }

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                Console.WriteLine("\nDrive {0}", d.Name);
                Console.WriteLine("  Drive type: {0}", d.DriveType);
                if (d.IsReady == true)
                {
                    Console.WriteLine("  Volume label: {0}", d.VolumeLabel);
                    Console.WriteLine("  File system: {0}", d.DriveFormat);
                    Console.WriteLine(
                        "  Available space to current user:{0, 22} bytes",
                        String.Format("{0:n0}", d.AvailableFreeSpace));

                    Console.WriteLine(
                        "  Total available space:          {0, 22} bytes",
                        String.Format("{0:n0}", d.TotalFreeSpace));

                    Console.WriteLine(
                        "  Total size of drive:            {0, 22} bytes ",
                        String.Format("{0:n0}", d.TotalSize));
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("[-] ERROR: {0}", e.Message.Trim());
        }
        finally
        {
            Console.WriteLine("\nDONE");
        }
    }
}