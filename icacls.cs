// Derived from: https://stackoverflow.com/questions/3507862/duplicate-getaccessrules-filesystemaccessrule-entries 

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:icacls.exe icacls.cs

using System;
using System.IO;
using System.Security.AccessControl;

namespace icacls
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine(@"Displays permissions, grouped by user/group, for each file or directory specified

USAGE:
    icacls.exe <file_or_directory>

    Example:
        icacls.exe passwords.txt" + "\n");
            } else {
                try
                {
                    foreach (string path in args) {
                        Console.WriteLine("Permissions for: " + path);
                        
                        FileSecurity fSecurity = new FileSecurity(path, AccessControlSections.Access);
                        
                        foreach (FileSystemAccessRule fsar in fSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                        {
                            string userName = fsar.IdentityReference.Value;
                            string userRights = fsar.FileSystemRights.ToString();
                            string userAccessType = fsar.AccessControlType.ToString();
                            string ruleSource = fsar.IsInherited ? "Inherited" : "Explicit";
                            string rulePropagation = fsar.PropagationFlags.ToString();
                            string ruleInheritance = fsar.InheritanceFlags.ToString();
                            Console.WriteLine("  " + userName + "\n    Type:\t\t" + userAccessType + "\n    Rights:\t\t" + userRights + "\n    Source:\t\t" + ruleSource + "\n    Propagation:\t" + rulePropagation + "\n    Inheritance:\t" + ruleInheritance);
                        }
        
                        // Print extra space in case multiple paths are specified
                        Console.WriteLine("\n");
                    }
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e);
                }
            }
            
            Console.WriteLine("DONE");
        }
    }
}