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

class TaskList
{
    public static void PrintUsage()
    {
        Console.WriteLine(@"List processes on local or remote system, optionally filter by ID or name
        
USAGE:
    tasklist_wmi.exe [/S system [/U [domain\]username /P password]] [ [/PID processid | /IM imagename | /FI ""WQL where clause""] ] [/V] [/D ""<delimiter>""] [/?]
    
    *NOTE*: When using /FI, you must provide a Win32_Process-compatible WMI query language condition string rather than a standard tasklist filter. You may use '%' as a wildcard
    
    Examples:
        tasklist_wmi.exe /v
        
        tasklist_wmi.exe /S 192.168.20.10 /FI ""Name Like 'cmd%'""
        
        tasklist_wmi.exe /S 192.168.20.10 /FI ""CommandLine Like '%svchost%'""
        
        tasklist_wmi.exe /S 192.168.20.10 /U Desktop-624L8K3\Administrator /P password /FI ""CommandLine Like '%svchost%'""");
    }

    public static void Main(string[] args)
    {
        try
        {
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
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "-D":
                    case "/D":
                        i++;
                        try
                        {
                            delimiter = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No delimiter specified");
                        }
                        break;
                    case "-S":
                    case "/S":
                        i++;
                        try
                        {
                            system = args[i].Trim(new Char[] { '\\', ' ' });
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No system specified");
                        }
                        break;
                    case "-PID":
                    case "/PID":
                        i++;

                        bool test = int.TryParse(args[i], out pid);
                        if (test == false)
                        {
                            throw new ArgumentException("Invalid PID");
                        }
                        pid_set = pid > -1;
                        break;
                    case "-IM":
                    case "/IM":
                        i++;
                        try
                        {
                            image = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No image specified");
                        }
                        image_set = true;
                        break;
                    case "-FI":
                    case "/FI":
                        i++;
                        try
                        {
                            condition = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No filter specified");
                        }
                        condition_set = true;
                        break;
                    case "-U":
                    case "/U":
                        i++;
                        try
                        {
                            username = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No username specified");
                        }

                        break;
                    case "-P":
                    case "/P":
                        i++;
                        try
                        {
                            password = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No password specified");
                        }
                        break;
                    case "-V":
                    case "/V":
                        verbose_set = true;
                        break;
                    case "/?":
                        PrintUsage();
                        return;
                }
            }

            // Error out if more than one of PID, image, and filter are specified
            if ((pid_set && image_set) || (pid_set && condition_set) || (image_set && condition_set))
            {
                throw new ArgumentException("PID and image cannot both be set");
            }

            var conn_opts = new ConnectionOptions();

            // Apply username and password if specified
            if (username.Length > 0 && password.Length > 0)
            {
                conn_opts.Username = username;
                conn_opts.Password = password;
            }
            else if (username.Length > 0 || password.Length > 0)
            {
                // Throw an exception if username or password were specified, but not both
                throw new ArgumentException("Please specify username and password");
            }

            ManagementPath path = new ManagementPath() { NamespacePath = @"root\cimv2", Server = system };
            ManagementScope scope = new ManagementScope(path, conn_opts);

            if (pid_set)
            {
                condition = "PROCESSID = '" + pid + "'";
            }
            else if (image_set)
            {
                condition = "NAME = '" + image + "'";
            }

            List<string> selectedProperties = new List<string>(new string[] { "ProcessId", "ParentProcessId", "SessionId", "Name", "Handle", "ExecutablePath", "CommandLine" });
            SelectQuery query = new SelectQuery("Win32_Process", condition, selectedProperties.ToArray());

            Dictionary<string, string> proc_info;
            List<Dictionary<string, string>> processes = new List<Dictionary<string, string>>();

            // Execute query within scope and iterate through results
            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                foreach (ManagementObject proc in searcher.Get())
                {
                    proc_info = new Dictionary<string, string>();

                    foreach (string prop in selectedProperties)
                    {
                        if (proc != null)
                        {
                            object val = proc.GetPropertyValue(prop);

                            if (val != null)
                            {
                                proc_info.Add(prop, val.ToString());
                            }
                        }
                    }

                    // Try to get the process owner
                    if (verbose_set)
                    {
                        try
                        {
                            string[] argList = new string[] { string.Empty, string.Empty };
                            int returnVal = Convert.ToInt32(proc.InvokeMethod("GetOwner", argList));
                            if (returnVal == 0)
                            {
                                // Store DOMAIN\user
                                proc_info.Add("User Name", argList[1] + "\\" + argList[0]);
                            }
                        }
                        catch (Exception e)
                        {
                            proc_info.Add("User Name", e.Message.ToString());
                        }
                    }

                    processes.Add(proc_info);
                }
            }

            if (processes.Count > 0)
            {
                // Replace or remove "Handle" property (only used by GetOwner when running in verbose mode)
                if (verbose_set)
                {
                    selectedProperties[4] = "User Name";
                }
                else
                {
                    selectedProperties.RemoveAt(4);
                }

                foreach (Dictionary<string, string> proc in processes)
                {
                    // Loop through the properties in the order specified above
                    foreach (string prop in selectedProperties)
                    {
                        if (proc.ContainsKey(prop))
                        {
                            Console.WriteLine("{0} : {1}", prop.PadRight(max_key_length), proc[prop]);
                        }
                    }

                    // Separate each process entry with the specified delimiter
                    Console.WriteLine(delimiter);
                }
            }
            else
            {
                Console.WriteLine("No processes found matching the specified criteria\n");
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