// Sources:
//     - Kill Remote Process - https://stackoverflow.com/questions/25727323/kill-process-on-remote-machine

// TODO:
//     - Kill child processes: https://kv4s.files.wordpress.com/2015/08/killserviceprocesses.jpg

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:taskkill.exe taskkill.cs

using System;
using System.Management;

class TaskKill {
    public static void PrintUsage() {
        Console.WriteLine(@"Kill process by ID or name
        
USAGE:
    taskkill [/S <system> [/U [domain\]<username> /P <password>]] { [/PID <processid> | /IM <imagename>] }");
        Console.WriteLine("\nDONE");
    }
    
    // Returns an appropriate singular or plural string based on the number of processes
    private static string GetNumProcStr(int numProcesses) {
        if (numProcesses == 1) {
            return "1 process";
        }
        
        return numProcesses + " processes";
    }
    
    public static void Main(string[] args) {
        if (args.Length == 0) {
            PrintUsage();
            return;
        }
        
        string system = "127.0.0.1";
        string username = "";
        string password = "";
        int pid = -1;
        string image = "";
        bool pid_set = false;
        bool image_set = false;
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
        
            switch (arg.ToUpper()) {
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
                        return;
                    }
                    pid_set = pid > -1;
                    break;
                case "-IM":
                case "/IM":
                    i++;
                    image = args[i];
                    image_set = true;
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
                case "/?":
                default:
                    PrintUsage();
                    return;
            }
        }
        
        // Error out if neither PID nor image are specified, or if both are specified
        if (!(pid_set || image_set)) {
            Console.WriteLine("ERROR: No process specified");
            Console.WriteLine("\nDONE");
            return;
        } else if (pid_set && image_set) {
            Console.WriteLine("ERROR: PID and image cannot both be set");
            Console.WriteLine("\nDONE");
            return;
        }

        ConnectionOptions conn_opts = new ConnectionOptions();
        
        // Apply username and password if specified
        if (username.Length > 0 && password.Length > 0) {
            conn_opts.Username = username;
            conn_opts.Password = password;
        } else if (username.Length > 0 || password.Length > 0) {
            // Error out if username or password were specified, but not both
            Console.WriteLine("ERROR: Please specify username and password");
            Console.WriteLine("\nDONE");
            return;
        }
        
        ManagementScope scope = new ManagementScope(@"\\" + system + @"\root\cimv2", conn_opts);

        // Initialize WMI query
        string queryStr = "select * from Win32_process where name = '" + image + "'";
        string procID = image;
        
        // Override the query if PID was specified
        if (pid_set) {
            queryStr = "select * from Win32_process where ProcessId = " + pid;
            procID = "PID: " + pid;
        }
        
        try {
            int numProcesses = 0;
            SelectQuery query = new SelectQuery(queryStr);
            
            // Execute query within scope and iterate through results
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                ManagementObjectCollection collection = searcher.Get();
                
                Console.WriteLine("Attempting to terminate " + GetNumProcStr(collection.Count));
                
                foreach (ManagementObject process in collection)
                {
                    try {
                        process.InvokeMethod("Terminate", null);
                        numProcesses++;
                    } catch (Exception e) {
                        //string name = process.Properties["Name"].Value;
                        
                        Console.WriteLine("ERROR: PID " + process.Properties["ProcessId"].Value 
                            + " " + e.Message.ToLower());
                    }
                }
            }
            
            Console.WriteLine("Terminated " + GetNumProcStr(numProcesses) + " (" + procID + ") on " + system);
        } catch (Exception e) {
            // Catch errors like connection timeouts
            Console.WriteLine("ERROR: " + e.Message);
        }
        
        Console.WriteLine("\nDONE");
    }
}