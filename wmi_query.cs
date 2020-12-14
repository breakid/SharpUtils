// Source: https://docs.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs?view=net-5.0
// Source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/9c28a7b0-9ee1-425e-8aa0-afeac329a983/list-of-installed-devices-and-drivers-using-cnet?forum=csharpgeneral


// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:wmi_query.exe wmi_query.cs



using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace WMIQuery
{
    public class Program
    {
        private static void WriteOutput(string outputFilepath, List<string> output)
        {
            if (outputFilepath != "")
            {
                Console.WriteLine("[*] Writing output to: {0}", outputFilepath);
                
                try
                {
                    System.IO.File.WriteAllLines(outputFilepath, output.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine("[-] ERROR: {0}", e.Message);
                }
            }
            else
            {
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
            }
        }
        
        private static void PrintUsage()
        {
            Console.WriteLine(@"Executes the specified WMI query; optionally writes output to a file
    
    USAGE:
        wmi_query.exe [/V] [/O <output_filepath>]
        
        /V    Verbose; print all properties, including empty ones");
            Console.WriteLine("\nDONE");
        }
        
        public static void Main()
        {
            string outputFilepath = "";
            string query = "";
            bool verbose = false;
            List<string> output = new List<string>();
            
            // Parse arguments
            string[] args = Environment.GetCommandLineArgs();
            
            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i];
            
                switch (arg.ToUpper()) {
                    case "-O":
                    case "/O":
                        i++;
                        outputFilepath = args[i];
                        
                        if (File.Exists(outputFilepath))
                        {
                            Console.WriteLine("[-] ERROR: Output file exists");
                            Console.WriteLine("\nDONE");
                            return;
                        }
                        break;
                    case "-V":
                    case "/V":
                        verbose = true;
                        break;
                    case "/?":
                        PrintUsage();
                        return;
                    default:
                        query = args[i];
                        break;
                }
            }
            
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                
                foreach (ManagementObject obj in searcher.Get())
                {
                    foreach (PropertyData prop in obj.Properties)
                    {
                        if (verbose)
                        {
                            output.Add(String.Format("{0}: {1}", prop.Name, prop.Value));
                        }
                        else if (prop.Value != null && !String.IsNullOrEmpty(prop.Value.ToString()))
                        {
                            output.Add(String.Format("{0}: {1}", prop.Name, prop.Value));
                        }
                    }
                    
                    // Add empty string to create a new line between property groups
                    output.Add("");
                }
                
                WriteOutput(outputFilepath, output);
            }
            catch (Exception e)
            {
                Console.WriteLine("[-] ERROR: {0}", e.Message);
            }
            
            Console.WriteLine("\nDONE");
        }
    }
}