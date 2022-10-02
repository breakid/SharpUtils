// Sources:
//     - Kill Remote Process - https://stackoverflow.com/questions/25727323/kill-process-on-remote-machine

// TODO:
//     - Kill child processes: https://kv4s.files.wordpress.com/2015/08/killserviceprocesses.jpg

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:taskkill.exe taskkill.cs


using System;
using System.Management;

class TaskKill
{
    public static void PrintUsage()
    {
        Console.WriteLine(@"Kill process by ID or name
        
USAGE:
    taskkill [/S <system> [/U [domain\]<username> /P <password>]] { [/PID <processid>[,...] | /IM <imagename>] }");
    }

    // Returns an appropriate singular or plural string based on the number of processes
    private static string GetNumProcStr(int numProcesses)
    {
        if (numProcesses == 1)
        {
            return "1 process";
        }

        return numProcesses + " processes";
    }

    public static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string system = "127.0.0.1";
            string username = "";
            string password = "";
            string pid_str = "";
            string image = "";
            bool pid_set = false;
            bool image_set = false;

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
                    case "-PID":
                    case "/PID":
                        i++;

                        try
                        {
                            pid_str = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No PID(s) specified");
                        }

                        pid_set = true;
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
                    case "/?":
                    default:
                        PrintUsage();
                        return;
                }
            }

            // Error out if neither PID nor image are specified, or if both are specified
            if (!(pid_set || image_set))
            {
                throw new ArgumentException("No process specified");
            }
            else if (pid_set && image_set)
            {
                throw new ArgumentException("PID and image cannot both be set");
            }

            ConnectionOptions conn_opts = new ConnectionOptions();

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

            ManagementScope scope = new ManagementScope(@"\\" + system + @"\root\cimv2", conn_opts);

            // Initialize WMI query
            string queryStr = "select * from Win32_process where name = '" + image + "'";
            string procID = image;

            // Override the query if PID was specified
            if (pid_set)
            {
                queryStr = "select * from Win32_process where ProcessId = " + pid_str.Replace(",", " OR ProcessID = ");
                procID = "PID: " + pid_str.Replace(",", ", ");
            }

            int numProcesses = 0;
            SelectQuery query = new SelectQuery(queryStr);

            // Execute query within scope and iterate through results
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                ManagementObjectCollection collection = searcher.Get();

                Console.WriteLine("Attempting to terminate " + GetNumProcStr(collection.Count));

                foreach (ManagementObject process in collection)
                {
                    try
                    {
                        process.InvokeMethod("Terminate", null);
                        numProcesses++;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("PID {0} {1}", process.Properties["ProcessId"].Value, e.Message.Trim().ToLower()));
                    }
                }
            }

            Console.WriteLine("Terminated " + GetNumProcStr(numProcesses) + " (" + procID + ") on " + system);
        }
        catch (Exception e)
        {
            // Catch errors like connection timeouts
            Console.Error.WriteLine("[-] ERROR: {0}", e.Message.Trim());
        }
        finally
        {
            Console.WriteLine("\nDONE");
        }
    }
}