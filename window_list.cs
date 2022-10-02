// A C# implementation of:
//   Get-Process | Where-Object {$_.MainWindowTitle -ne ""} | Select-Object MainWindowTitle


// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:window_list.exe window_list.cs


using System;
using System.Diagnostics;
using System.Collections.Generic;

class WindowList
{
    public static void PrintUsage()
    {
        Console.WriteLine(@"Displays a list of visible windows and their associated process

USAGE:
    window_list.exe [/S <remote system>] [/?]");
    }

    // Source: https://stackoverflow.com/questions/2776673/how-do-i-truncate-a-net-string
    private static string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str.Length <= maxLength ? str : str.Substring(0, maxLength - 3) + "...";
    }

    private static void PrintDivider()
    {
        Console.WriteLine("\n" + new String('=', 78));
    }

    private static void PrintHeader()
    {
        Console.WriteLine("\nProc ID    Process Name                      Window Title");
        Console.WriteLine("-------    ------------                      ------------");
    }

    private static void PrintProcesses(List<Process> procs)
    {
        string line = "";

        foreach (Process proc in procs)
        {
            line = proc.Id.ToString().PadRight(11, ' ');
            try
            {
                // Display the executable name
                line += Truncate(proc.MainModule.ModuleName, 30).PadRight(34, ' ');
            }
            catch
            {
                // Sometimes MainModule fails with "Access Denied", fall back to ProcessName
                line += Truncate(proc.ProcessName, 30).PadRight(34, ' ');
            }
            line += proc.MainWindowTitle;

            Console.WriteLine(line);
        }
    }

    public static void Main(string[] args)
    {
        try
        {
            string system = ".";

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
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
                    case "/?":
                        PrintUsage();
                        return;
                }
            }

            if (system != ".")
            {
                Console.WriteLine("Listing windows on: " + system + "\n");
            }

            int session;
            IDictionary<int, List<Process>> process_map = new SortedDictionary<int, List<Process>>();

            // Group processes in a dictionary by session ID
            // TODO: Test against remote system
            foreach (Process proc in Process.GetProcesses(system))
            {
                if (proc.MainWindowTitle == "")
                {
                    continue;
                }

                session = proc.SessionId;

                if (!process_map.ContainsKey(session))
                {
                    process_map[session] = new List<Process>();
                }

                process_map[session].Add(proc);
            }

            PrintDivider();

            foreach (KeyValuePair<int, List<Process>> kvp in process_map)
            {
                Console.WriteLine("Session ID: " + kvp.Key);
                PrintHeader();
                PrintProcesses(kvp.Value);
                PrintDivider();
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