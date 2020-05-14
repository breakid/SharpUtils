/*
 * Read the contents of a file; optionally limit to X number of lines from the beginning of the file or Y number of lines from the end
 * 
 * USAGE: read.exe [+X] [-Y] <path_to_file>
 */
 
// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:readfile.exe readfile.cs
 
using System;
using System.IO;

class ReadFile {
    private static int PrintUsage() {
        string[] exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Split('\\');
        string exeName = exePath[exePath.Length - 1];
        
        Console.WriteLine(@"Read the contents of a file; optionally limit to X number of lines from the beginning of the file or Y number of lines from the end

USAGE:
    {0} [+X] [-Y] <path_to_file>", exeName);
        return 1;
    }
    
    private static int ReadContents(string[] args) {
        if (args.Length == 0) {
            return PrintUsage();
        }
        
        int head = 0;
        int tail = 0;
        string filePath = "";
        int lineCount = 0;
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
            
            if (arg.StartsWith("+")) {
                arg = arg.Replace("+", "");
                
                bool test = int.TryParse(arg, out head);
                if (test == false) {
                    Console.WriteLine("Error: Invalid number of lines to read from head of file");
                    return 1;
                }
            } else if (arg.StartsWith("-")) {
                arg = arg.Replace("-", "");
                
                bool test = int.TryParse(arg, out tail);
                if (test == false) {
                    Console.WriteLine("Error: Invalid number of lines to read from tail of file");
                    return 1;
                }
            } else {
                filePath = arg;
            }
        }
        
        if (File.Exists(filePath)) {
            // If tail is specified, determine line count
            if (tail > 0) {
                using (StreamReader file = new StreamReader(filePath)) {
                    while (file.ReadLine() != null) {
                        lineCount++;
                    }
                }
            }
            
            // Use StreamReader so the entire files doesn't have to get read into memory at once
            using (StreamReader file = new StreamReader(filePath)) {
                int i = 0;
                string line;
                    
                while ((line = file.ReadLine()) != null) {
                    // Print the line if it's less than head, greater than tail, or neither head nor tail are specified
                    if ((head > 0 && i < head) || (tail > 0 && i >= lineCount - tail) || (head == 0 && tail == 0)) {
                        Console.WriteLine(line);
                    }
                    
                    // Stop when head is reached if tail is not also specified (for efficiency)
                    if (head > 0 && i == head && tail == 0) {
                        break;
                    }
                    
                    i++;
                }
            }
        } else {
            Console.WriteLine("[-] ERROR: File does not exist");
        }
        
        return 0;
    }
    
    public static void Main(string[] args) {
        ReadContents(args);
        
        Console.WriteLine("\nDONE");
    }
}