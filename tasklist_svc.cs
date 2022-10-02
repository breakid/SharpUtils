/*
 * USAGE: tasklist_svc.exe
 *
 * Lists processes, their PIDs, and their associated services (if applicable)
 */
// C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:test/tasklist_svc.exe tasklist_svc.cs


using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;

public class TaskListSVC
{
    private static void PrintUsage()
    {
        Console.WriteLine(@"Lists processes, their PIDs, and their associated services (if applicable)
    
USAGE:
    tasklist_svc.exe [/?]");
    }

    public static void Main(string[] args)
    {
        try
        {
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "/?":
                        PrintUsage();
                        return;
                }
            }

            Dictionary<int, string> pid_proc_map = new Dictionary<int, string>();
            int max_proc_name = 0;
            int proc_id;
            string proc_name;

            // Get a process list; map PIDs to process names
            foreach (ManagementObject proc in (new ManagementClass("Win32_Process")).GetInstances())
            {
                proc_id = Convert.ToInt32(proc["ProcessID"]);
                proc_name = proc["Name"].ToString();

                pid_proc_map.Add(proc_id, proc_name);

                // Find the length of the longest process name, so the output can be formatted nicely
                if (proc_name.Length > max_proc_name)
                {
                    max_proc_name = proc_name.Length;
                }
            }

            int max_pid = 8;
            int max_svc_name = 45;
            List<string> svc_names;
            string svc_str;
            string query;

            // Print the table header, pad each column so they line up nicely
            Console.WriteLine("{0} {1} {2}", "Image Name".PadRight(max_proc_name), "PID".PadLeft(max_pid), "Services".PadRight(max_svc_name));
            Console.WriteLine("{0} {1} {2}", new String('=', max_proc_name), new String('=', max_pid), new String('=', max_svc_name));

            // Sort the PIDs for convenience (because why not be better than the native command...)
            List<int> pids = pid_proc_map.Keys.ToList();
            pids.Sort();

            // Loop through each PID, search for any associated processes, and print
            foreach (int pid in pids)
            {
                svc_str = "N/A";
                svc_names = new List<string>();

                if (pid != 0)
                {
                    query = "SELECT NAME FROM WIN32_SERVICE WHERE PROCESSID = '" + pid.ToString() + "'";
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                    foreach (ManagementObject svc in searcher.Get())
                    {
                        svc_names.Add(svc["NAME"].ToString());
                    }
                }

                // Comma-separate the service names if there are multiples
                if (svc_names.Count > 0)
                {
                    svc_str = String.Join(", ", svc_names.ToArray());
                }

                Console.WriteLine("{0} {1} {2}", pid_proc_map[pid].PadRight(max_proc_name), pid.ToString().PadLeft(max_pid), svc_str);
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
