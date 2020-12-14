// Source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/9c28a7b0-9ee1-425e-8aa0-afeac329a983/list-of-installed-devices-and-drivers-using-cnet?forum=csharpgeneral

// TODO - find a better way to approximate driveryquery /v (esp location of driver)

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:driver_list.exe driver_list.cs


using System;
using System.Collections.Generic;
using System.Management;

namespace DriverList
{
    public class Driver
    {
        public string ID;
        public string Name;
        public string Version;
        public string Signer;
        public Driver(string id, string name, string version, string signer)
        {
            ID = id;
            Name = name;
            Version = version;
            Signer = signer;
            
            if (Name == "" && ID != "")
            {
                Name = ID;
            }
        }
    }

    public class Program
    {
        private static void PrintUsage() {
            Console.WriteLine(@"Lists data about installed drivers
    
    USAGE:
        driver_list.exe [/V | /VV]
        
        /V    Verbose; lists driver ID, name, version, and signer
        /VV   Very verbose; dumps all driver data in list format");
            Console.WriteLine("\nDONE");
        }
        
        public static void Main(string[] args)
        {
            string id;
            string name;
            string version;
            string signer;
            int name_len = 0;
            int signer_len = 0;
            bool verbose = false;
            bool very_verbose = false;
            Driver d;
            List<Driver> driver_list = new List<Driver>();
            
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
            
                switch (arg.ToUpper()) {
                    case "-V":
                    case "/V":
                        verbose = true;
                        break;
                    case "-VV":
                    case "/VV":
                        very_verbose = true;
                        break;
                    case "/?":
                        PrintUsage();
                        return;
                }
            }
            
            string fields = (very_verbose) ? "*" : "DeviceID,DeviceName,DriverVersion,Signer";
            string query = String.Format("SELECT {0} FROM Win32_PnPSignedDriver", fields);
            
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            
            foreach (ManagementObject obj in searcher.Get())
            {
                if (very_verbose)
                {
                    foreach (PropertyData prop in obj.Properties)
                    {
                        Console.WriteLine("{0,-24}: {1}", prop.Name, prop.Value);
                    }
                    
                    // Add a new line to separate property groups
                    Console.WriteLine("");
                }
                else
                {
                    id = String.IsNullOrEmpty((string)obj.GetPropertyValue("DeviceID")) ? "" : obj.GetPropertyValue("DeviceID").ToString();
                    name = String.IsNullOrEmpty((string)obj.GetPropertyValue("DeviceName")) ? "" : obj.GetPropertyValue("DeviceName").ToString();
                    version = String.IsNullOrEmpty((string)obj.GetPropertyValue("DriverVersion")) ? "" : obj.GetPropertyValue("DriverVersion").ToString();
                    signer = String.IsNullOrEmpty((string)obj.GetPropertyValue("Signer")) ? "" : obj.GetPropertyValue("Signer").ToString();
                    
                    d = new Driver(id, name, version, signer);
                    driver_list.Add(d);
                    
                    name_len = (d.Name.Length > name_len) ? d.Name.Length : name_len;
                    signer_len = (d.Signer.Length > signer_len) ? d.Signer.Length : signer_len;
                }
            }
            
            if (very_verbose)
            {
                Console.WriteLine("\nDONE");
                System.Environment.Exit(0);
            }
            
            if (verbose)
            {
                foreach (Driver driver in driver_list)
                {
                    Console.WriteLine("{0,-8}: {1}", "ID", driver.ID);
                    Console.WriteLine("{0,-8}: {1}", "Name", driver.Name);
                    Console.WriteLine("{0,-8}: {1}", "Version", driver.Version);
                    Console.WriteLine("{0,-8}: {1}\n", "Signer", driver.Signer);
                }
            }
            else
            {
                // Use the max length of the driver name to dynamically size the table
                name_len += 4;
                signer_len += 4;
                string format_str = "{0,-" + name_len + "}{1}";
                Console.WriteLine(format_str, "Name", "Signer");
                Console.WriteLine(new String('-', name_len + signer_len));
                
                foreach (Driver driver in driver_list)
                {
                    Console.WriteLine(format_str, driver.Name, driver.Signer);
                }
            }
            
            Console.WriteLine("\nDONE");
        }
    }
}