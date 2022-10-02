// Source
//   - Slice - https://www.dotnetperls.com/array-slice

// TODO
//   - Add username/password option like taskkill.cs
//   - Populate other fields to blend in better - https://docs.microsoft.com/en-us/dotnet/api/system.management.managementobject.invokemethod?view=netframework-4.7.2

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:create_process.exe create_process.cs


using System;
using System.Management;

public static class Extensions
{
    /// <summary>
    /// Get the array slice between the two indexes.
    /// ... Inclusive for start index, exclusive for end index.
    /// </summary>
    public static T[] Slice<T>(this T[] source, int start, int end)
    {
        // Handles negative ends.
        if (end < 0)
        {
            end = source.Length + end;
        }
        int len = end - start;

        // Return new array.
        T[] res = new T[len];
        for (int i = 0; i < len; i++)
        {
            res[i] = source[i + start];
        }
        return res;
    }
}

public class CreateProcess
{
    private static void PrintUsage()
    {
        Console.WriteLine(@"Executes the given command on the specified system

USAGE:
    CreateProcess.exe <system> <full_path_to_command_on_system> <executable args...>

    Example:
        CreateProcess.exe 192.168.20.10 C:\Windows\System32\program.exe -Run");
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

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            else if (args.Length > 1)
            {
                // Parse target system from first arg; strip \\ just in case
                string system = args[0].Trim(new Char[] { ' ', '\\' });

                // Catenate remaining args into a command string
                string command = String.Join(" ", args.Slice(1, args.Length));

                Console.WriteLine("[*] Running '" + command + "' on " + system);

                ManagementClass processClass = new ManagementClass(@"\\" + system + @"\root\cimv2:Win32_Process");

                // Execute the method
                ManagementBaseObject inParams = processClass.GetMethodParameters("Create");
                inParams["CommandLine"] = command;
                ManagementBaseObject result = processClass.InvokeMethod("Create", inParams, null);

                // Display results
                if (result["returnValue"].ToString() == "0")
                {
                    Console.WriteLine("Process ID: " + result["processId"]);
                }
                else
                {
                    throw new Exception(String.Format("Failed to start process; exit code: {0}", result["returnValue"]));
                }
            }
            else
            {
                throw new ArgumentException("No command specified");
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