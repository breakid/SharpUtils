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
            try
            {
                if (args.Length == 0 || (args.Length > 0 && args[0] == "/?"))
                {
                    Console.WriteLine(@"Displays permissions, grouped by user/group, for each file or directory specified

USAGE:
    icacls.exe <file_or_directory> [...] [/?]

    Example:
        icacls.exe passwords.txt users.txt");
                }
                else
                {
                    int i = 0;

                    foreach (string path in args)
                    {
                        Console.WriteLine("Permissions for: " + path);

                        FileSecurity fSecurity = new FileSecurity(path, AccessControlSections.Access);

                        foreach (FileSystemAccessRule fsar in fSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                        {
                            Console.WriteLine("  {0}", fsar.IdentityReference.Value);
                            Console.WriteLine("    Type: {0}", fsar.AccessControlType);
                            Console.WriteLine("    Rights: {0}", fsar.FileSystemRights);
                            Console.WriteLine("    Source: {0}", fsar.IsInherited ? "Inherited" : "Explicit");
                            Console.WriteLine("    Propagation: {0}", fsar.PropagationFlags);
                            Console.WriteLine("    Inheritance: {0}", fsar.InheritanceFlags);
                        }

                        // Print extra space as a separator when multiple paths are specified
                        if (i++ < args.Length - 1)
                        {
                            Console.WriteLine("");
                        }
                    }
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
}