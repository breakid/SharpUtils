// Source: https://docs.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs?view=net-5.0
// Source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/9c28a7b0-9ee1-425e-8aa0-afeac329a983/list-of-installed-devices-and-drivers-using-cnet?forum=csharpgeneral
// Source: https://docs.microsoft.com/en-us/dotnet/api/system.management.managementobjectsearcher.scope?view=dotnet-plat-ext-6.0#system-management-managementobjectsearcher-scope


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
    wmi_query.exe [/S <system> [/U [domain\]username /P password]] [-N <namespace>] [/O <output_filepath>] [/V]
    
    /V    Verbose; print all properties, including empty ones
    
    
    Examples:
        wmi_query.exe 'Select * from win32_process'
        
        wmi_query.exe -S DC01.MGMT.LOCAL -N root\standardcimv2 'Select * from MSFT_NetTCPConnection'
        
        wmi_query.exe -S DC01.MGMT.LOCAL -U MGMT\Administrator -P password -N root\standardcimv2 'Select * from MSFT_NetTCPConnection'");
            Console.WriteLine("\nDONE");
        }
        
        public static void Main()
        {
            string outputFilepath = "";
            string query = "";
            string namespace_path = @"root\cimv2";
            string system = ".";
            string username = "";
            string password = "";
            bool verbose = false;
            List<string> output = new List<string>();
            
            // Parse arguments
            string[] args = Environment.GetCommandLineArgs();
            
            try {
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                
                    switch (arg.ToUpper()) {
                        case "-N":
                        case "/N":
                            i++; 
                            namespace_path = args[i];
                            break;
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
                        case "-S":
                        case "/S":
                            i++;
                            system = args[i];
                            break;
                        case "-U":
                        case "/U":
                            i++;
                            username = args[i];
                            break;
                        case "-P":
                        case "/P":
                            i++;
                            password = args[i];
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
            } catch (IndexOutOfRangeException) {
                Console.WriteLine("ERROR: Invalid arguments");
                Console.WriteLine("\nDONE");
                return;
            }
            
            try
            {
                /*string scope = name_space;
                
                if (!String.IsNullOrEmpty(system)) {
                    scope = "\\\\" + system + "\\" + name_space;
                }
                */
                
                ConnectionOptions conn_opts = new ConnectionOptions();
                
                // Apply username and password if specified
                if (username.Length > 0 && password.Length > 0) {
                    conn_opts.Username = username;
                    conn_opts.Password = password;
                } else if (username.Length > 0 || password.Length > 0) {
                    // Error out if username or password were specified, but not both
                    Console.WriteLine("ERROR: Please specify username and password");
                    Console.WriteLine("\nDONE");
                    System.Environment.Exit(1);
                }
                
                ManagementPath path = new ManagementPath() { NamespacePath = namespace_path, Server = system };
                ManagementScope scope = new ManagementScope(path, conn_opts);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                searcher.Scope = scope;
                
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
                Console.WriteLine("[-] ERROR: {0}", e.Message.Trim());
            }
            
            Console.WriteLine("\nDONE");
        }
    }
}