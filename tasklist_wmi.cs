/*
 * USAGE: tasklist_wmi.exe
 *
 * List processes on local or remote system, optionally filter by ID, name, or WQL query
 *
 * Examples:
 *   tasklist_wmi.exe /v
 *   
 *   tasklist_wmi.exe /S 192.168.20.10 /FI "Name Like 'cmd%'"
 *   
 *   tasklist_wmi.exe /S 192.168.20.10 /FI "CommandLine Like '%svchost%'"
 *
 *   tasklist_wmi.exe /S 192.168.20.10 /U Desktop-624L8K3\Administrator /P password /FI "CommandLine Like '%svchost%'"
 */
// Source:
//   - https://stackoverflow.com/questions/777548/how-do-i-determine-the-owner-of-a-process-in-c

// C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\tasklist_wmi.exe tasklist_wmi.cs

using System;
using System.Collections.Generic;
using System.Management;

class TaskList {
    public static void PrintUsage() {
        Console.WriteLine(@"List processes on local or remote system, optionally filter by ID or name
        
USAGE:
    tasklist_wmi.exe [/S system [/U [domain\]username /P password]] [ [/PID processid | /IM imagename | /FI ""WQL where clause""] ] [/V] [/D ""<delimiter>""]
    
    *NOTE*: When using /FI, you must provide a Win32_Process-compatible WMI query language condition string rather than a standard tasklist filter. You may use '%' as a wildcard
    
    Examples:
        tasklist_wmi.exe /v
        
        tasklist_wmi.exe /S 192.168.20.10 /FI ""Name Like 'cmd%'""
        
        tasklist_wmi.exe /S 192.168.20.10 /FI ""CommandLine Like '%svchost%'""
        
        tasklist_wmi.exe /S 192.168.20.10 /U Desktop-624L8K3\Administrator /P password /FI ""CommandLine Like '%svchost%'""
    ");
        Console.WriteLine("DONE");
    }
    
    public static void Main(string[] args) {
        int max_key_length = 15;
        string system = ".";
        string username = "";
        string password = "";
        string delimiter = "";
        int pid = -1;
        string image = "";
        string condition = "";
        bool pid_set = false;
        bool image_set = false;
        bool condition_set = false;
        bool verbose_set = false;
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
        
            switch (arg.ToUpper()) {
                case "-D":
                case "/D":
                    i++;
                    delimiter = args[i];
                    break;
                case "-S":
                case "/S":
                    i++;
                    system = args[i].Trim(new Char[] {'\\', ' '});
                    break;
                case "-PID":
                case "/PID":
                    i++;
                    
                    // Catch error while attempting to parse the pid to prevent exception
                    bool test = int.TryParse(args[i], out pid);
                    if (test == false)
                    {
                        Console.WriteLine("Error: Invalid PID");
                        Console.WriteLine("\nDONE");
                        System.Environment.Exit(1);
                    }
                    pid_set = pid > -1;
                    break;
                case "-IM":
                case "/IM":
                    i++;
                    image = args[i];
                    image_set = true;
                    break;
                case "-FI":
                case "/FI":
                    i++;
                    condition = args[i];
                    condition_set = true;
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
                    verbose_set = true;
                    break;
                case "/?":
                default:
                    PrintUsage();
                    System.Environment.Exit(0);
                    break;
            }
        }
        
        // Error out if more than one of PID, image, and filter are specified
        if ((pid_set && image_set) || (pid_set && condition_set) || (image_set && condition_set)) {
            Console.WriteLine("ERROR: PID and image cannot both be set");
            Console.WriteLine("\nDONE");
            System.Environment.Exit(1);
        }
        
        var conn_opts = new ConnectionOptions();
        
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
        
        ManagementPath path = new ManagementPath() { NamespacePath = @"root\cimv2", Server = system };
        ManagementScope scope = new ManagementScope(path, conn_opts);
        
        if (pid_set) {
            condition = "PROCESSID = '" + pid + "'";
        } else if (image_set) {
            condition = "NAME = '" + image + "'";
        }
        
        List<string> selectedProperties = new List<string>(new string[] { "ProcessId", "ParentProcessId", "SessionId", "Name", "Handle", "ExecutablePath", "CommandLine"});
        SelectQuery query = new SelectQuery("Win32_Process", condition, selectedProperties.ToArray());
        
        Dictionary<string,string> proc_info;
        List<Dictionary<string,string>> processes = new List<Dictionary<string,string>>();
        
        try {
            // Execute query within scope and iterate through results
            using (var searcher = new ManagementObjectSearcher(scope, query)) {
                foreach (ManagementObject proc in searcher.Get()) {
                    proc_info = new Dictionary<string,string>();
                    
                    foreach (string prop in selectedProperties) {
                        if (proc != null) {
                            object val = proc.GetPropertyValue(prop);
                            
                            if (val != null) {
                                proc_info.Add(prop, val.ToString());
                            }
                        }
                    }
                    
                    // Try to get the process owner
                    if (verbose_set) {
                        try {
                            string[] argList = new string[] { string.Empty, string.Empty };
                            int returnVal = Convert.ToInt32(proc.InvokeMethod("GetOwner", argList));
                            if (returnVal == 0) {
                                // Store DOMAIN\user
                                proc_info.Add("User Name", argList[1] + "\\" + argList[0]);
                            }
                        } catch (Exception e) {
                            proc_info.Add("User Name", e.Message.ToString());
                        }
                    }
                    
                    processes.Add(proc_info);
                }
            }
            
            if (processes.Count > 0) {
                // Replace or remove "Handle" property (only used by GetOwner when running in verbose mode)
                if (verbose_set) {
                    selectedProperties[4] = "User Name";
                } else {
                    selectedProperties.RemoveAt(4);
                }
                
                foreach (Dictionary<string,string> proc in processes) {
                    // Loop through the properties in the order specified above
                    foreach (string prop in selectedProperties) {
                        if (proc.ContainsKey(prop)) {
                            Console.WriteLine("{0} : {1}", prop.PadRight(max_key_length), proc[prop]);
                        }
                    }
                    
                    // Separate each process entry with the specified delimiter
                    Console.WriteLine(delimiter);
                }
            } else {
                Console.WriteLine("No processes found matching the specified criteria\n");
            }
        } catch (Exception e) {
            Console.WriteLine("ERROR: {0}\n", e.Message);
        }
        
        Console.WriteLine("DONE");
    }
}