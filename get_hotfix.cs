// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:get_hotfix.exe get_hotfix.cs

using System;
using System.Management;

class GetHotFix {
    public static int PrintUsage() {
        Console.WriteLine(@"List all patches on a system
    
USAGE:
    get-hotfix [/S system] [/U [domain\]username /P password] [/V]
    
        /V    Print additional information (Description, InstalledBy, InstalledOn)
    ");
        return 1;
    }
    
    public static int Main(string[] args) {
        string system = ".";
        string username = "";
        string password = "";
        bool verbose_set = false;
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
        
            switch (arg.ToUpper()) {
                case "-S":
                case "/S":
                    i++;
                    system = args[i].Trim(new Char[] {'\\', ' '});
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
                case "-V":
                case "/V":
                    verbose_set = true;
                    break;
                case "/?":
                default:
                    return PrintUsage();
            }
        }
        
        ConnectionOptions conn_opts = new ConnectionOptions();
        
        // Apply username and password if specified
        if (username.Length > 0 && password.Length > 0) {
            conn_opts.Username = username;
            conn_opts.Password = password;
        } else if (username.Length > 0 || password.Length > 0) {
            // Error out if username or password were specified, but not both
            Console.WriteLine("ERROR: Please specify username and password");
            return 1;
        }
        
        ManagementScope scope = new ManagementScope("\\\\" + system + "\\root\\cimv2", conn_opts);
        
        string[] selectedProperties;
        
        if (verbose_set) {
            selectedProperties = new string[] {"HotFixID", "Description", "InstalledBy", "InstalledOn"};
        } else {
           selectedProperties = new string[] {"HotFixID"};
        }
        
        SelectQuery query = new SelectQuery("Win32_QuickFixEngineering", null, selectedProperties);
        
        try {
            // Execute query within scope and iterate through results
            using (var searcher = new ManagementObjectSearcher(scope, query)) {
                string line = "\n";
                
                // Print a column header for each property
                foreach (string prop in selectedProperties) {
                    line += prop.PadRight(22, ' ');
                }
                
                Console.WriteLine(line);
                
                line = "";
                
                // Dynamically generate underlines based on the list of properties
                foreach (string prop in selectedProperties) {
                    line += new String('-', prop.Length).PadRight(22, ' ');
                }
                
                Console.WriteLine(line);
                
                // Print the hotfix info
                foreach (ManagementObject proc in searcher.Get()) {
                    if (proc != null) {
                        line = "";
                        
                        foreach (string prop in selectedProperties) {
                            string val = proc.GetPropertyValue(prop).ToString();
                            
                            if (val != null) {
                                line += val.PadRight(22, ' ');
                            }
                        }
                        Console.WriteLine(line);
                    }
                }
            }
            
            Console.WriteLine("\nDONE");
            
            return 0;
        } catch (Exception e) {
            Console.WriteLine("ERROR: " + e.Message);
            return 1;
        }
    }
}