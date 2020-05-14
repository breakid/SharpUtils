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

public class CreateProcess {
    public static void Main(string[] args) {
        if (args.Length == 0) {
            Console.WriteLine(@"Executes the given command on the specified system

USAGE:
    CreateProcess.exe <system> <full_path_to_command_on_system> <executable args...>

    Example:
        CreateProcess.exe 192.168.20.10 C:\Windows\System32\program.exe -Run");
        } else {
            // Parse target system from first arg; strip \\ just in case
            string system = args[0].Trim(new Char[] {' ', '\\'});
            
            // Catenate remaining args into a command string
            string command = String.Join(" ", args.Slice(1, args.Length));
            
            Console.WriteLine("Running '" + command + "' on " + system);

            ManagementClass processClass = new ManagementClass(@"\\" + system + @"\root\cimv2:Win32_Process");

            // Create an array containing arguments for InvokeMethod
            object[] methodArgs =    {command, null, null, 0};

            try {
                // Execute the method
                int result;
                bool test = int.TryParse(processClass.InvokeMethod("Create", methodArgs).ToString(), out result);
                
                // Display results
                if (test && result == 0) {
                    Console.WriteLine("PID: " + methodArgs[3]);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message.Trim());
            }
        }
        
        Console.WriteLine("\nDONE");
    }
}