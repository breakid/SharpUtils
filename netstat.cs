// Source: https://docs.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs?view=net-5.0
// Source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/9c28a7b0-9ee1-425e-8aa0-afeac329a983/list-of-installed-devices-and-drivers-using-cnet?forum=csharpgeneral
// Source: https://docs.microsoft.com/en-us/dotnet/api/system.management.managementobjectsearcher.scope?view=dotnet-plat-ext-6.0#system-management-managementobjectsearcher-scope


// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:wmi_query.exe wmi_query.cs



using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace Netstat
{
    public class Program
    {
        private static void WriteOutput(string outputFilepath, List<string> output)
        {
            if (outputFilepath != "")
            {
                Console.WriteLine("[*] Writing output to: {0}", outputFilepath);
                
                try
                {
                    System.IO.File.WriteAllLines(outputFilepath, output.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine("[-] ERROR: {0}", e.Message);
                }
            }
            else
            {
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
            }
        }
        
        private static void PrintUsage()
        {
            Console.WriteLine(@"Lists listening TCP and UDP ports, and active TCP connection (equivalent to 'netstat -ano'); optionally writes output to a file
    
USAGE:
    netstat.exe [/S <system> [/U [domain\]username /P password]] [/O <output_filepath>] [TCP | UDP]
    
    Examples:
        netstat.exe
        
        netstat.exe tcp
        
        netstat.exe -S DC01.MGMT.LOCAL
        
        netstat.exe -S DC01.MGMT.LOCAL -U MGMT\Administrator -P password");
            Console.WriteLine("\nDONE");
        }
        
        public static void Main()
        {
            string outputFilepath = "";
            string system = ".";
            string username = "";
            string password = "";
            string protocol = "";
            
            // Parse arguments
            string[] args = Environment.GetCommandLineArgs();
            
            try {
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                
                    switch (arg.ToUpper()) {
                        case "-O":
                        case "/O":
                            i++;
                            outputFilepath = args[i];
                            
                            if (File.Exists(outputFilepath))
                            {
                                Console.WriteLine("[-] ERROR: Output file exists");
                                Console.WriteLine("\nDONE");
                                return;
                            }
                            break;
                        case "-S":
                        case "/S":
                            i++;
                            system = args[i];
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
                        case "/?":
                            PrintUsage();
                            return;
                        default:
                            protocol = args[i].ToUpper();
                            
                            if (!(protocol.Equals("TCP") || protocol.Equals("UDP"))) {
                                Console.WriteLine("[-] ERROR: Invalid protocol specified");
                                Console.WriteLine("\nDONE");
                                return;
                            }
                            
                            break;
                    }
                }
            } catch (IndexOutOfRangeException) {
                Console.WriteLine("ERROR: Invalid arguments");
                Console.WriteLine("\nDONE");
                return;
            }
            
            //try
            //{
                ConnectionOptions conn_opts = new ConnectionOptions();
                
                // Apply username and password if specified
                if (username.Length > 0 && password.Length > 0) {
                    conn_opts.Username = username;
                    conn_opts.Password = password;
                } else if (username.Length > 0 || password.Length > 0) {
                    // Error out if username or password were specified, but not both
                    Console.WriteLine("ERROR: Please specify username and password");
                    Console.WriteLine("\nDONE");
                    System.Environment.Exit(1);
                }
                
                // Keep track of the max length of each entry in order to dynamically space the columns
                int localAddrMaxSize = 13;       // Length of "Local Address"
                int remoteAddrMaxSize = 15;      // Length of "Foreign Address"
                int stateMaxSize = 5;             // Length of "State"
                int colPadding = 4;
                Dictionary<string,string> entry;
                List<Dictionary<string,string>> entries = new List<Dictionary<string,string>>();
                
                // Lookup table for TCP connection states
                Dictionary<string,string> tcpStates = new Dictionary<string,string>();
                tcpStates.Add("1", "Closed");
                tcpStates.Add("2", "LISTENING");
                tcpStates.Add("3", "SYN_SENT");
                tcpStates.Add("4", "SYN_RECEIVED");
                tcpStates.Add("5", "ESTABLISHED");
                tcpStates.Add("6", "FIN_WAIT1");
                tcpStates.Add("7", "FIN_WAIT2");
                tcpStates.Add("8", "CLOSE_WAIT");
                tcpStates.Add("9", "CLOSING");
                tcpStates.Add("10", "LAST_ACK");
                tcpStates.Add("11", "TIME_WAIT");
                tcpStates.Add("12", "DELETE_TCB");
                tcpStates.Add("100", "BOUND");
                
                ManagementPath path = new ManagementPath() { NamespacePath = @"root\standardcimv2", Server = system };
                ManagementScope scope = new ManagementScope(path, conn_opts);
                SelectQuery query;
                
                // Display TCP if it's specified or no protocol was specified
                if (protocol.Equals("TCP") || protocol.Equals("")) {
                    // Query for TCP ports and connections; return specified attributes
                    query = new SelectQuery("MSFT_NetTCPConnection", null, new string[] { "LocalAddress", "LocalPort", "RemoteAddress", "RemotePort", "State", "OwningProcess"});
                    
                    using (var searcher = new ManagementObjectSearcher(scope, query)) {
                        foreach (ManagementObject obj in searcher.Get()) {
                            if (obj != null) {
                                entry = new Dictionary<string,string>();
                                
                                entry.Add("protocol", "TCP");
                                // Use a lookup to convert the state code into a human-readable string
                                entry.Add("state", tcpStates[obj.GetPropertyValue("State").ToString()]);
                                entry.Add("pid", obj.GetPropertyValue("OwningProcess").ToString());
                                
                                if (obj.GetPropertyValue("LocalAddress").ToString().Contains(":")) {
                                    // IPv6 address
                                    entry.Add("local_address", "[" + obj.GetPropertyValue("LocalAddress").ToString() + "]:" + obj.GetPropertyValue("LocalPort").ToString());
                                    entry.Add("remote_address", "[" + obj.GetPropertyValue("RemoteAddress").ToString() + "]:" + obj.GetPropertyValue("RemotePort").ToString());
                                } else {
                                    // IPv4 address
                                    entry.Add("local_address", obj.GetPropertyValue("LocalAddress").ToString() + ":" + obj.GetPropertyValue("LocalPort").ToString());
                                    entry.Add("remote_address", obj.GetPropertyValue("RemoteAddress").ToString() + ":" + obj.GetPropertyValue("RemotePort").ToString());
                                }
                                
                                entries.Add(entry);
                                
                                // Calculate the max length of each column (for dynamic spacing)
                                if (entry["local_address"].Length > localAddrMaxSize) {
                                    localAddrMaxSize = entry["local_address"].Length;
                                }
                                
                                if (entry["remote_address"].Length > remoteAddrMaxSize) {
                                    remoteAddrMaxSize = entry["remote_address"].Length;
                                }
                                
                                if (entry["state"].Length > stateMaxSize) {
                                    stateMaxSize = entry["state"].Length;
                                }
                            }
                        }
                    }
                }
                
                // Display TCP if it's specified or no protocol was specified
                if (protocol.Equals("UDP") || protocol.Equals("")) {
                    // Query for UDP ports; return specified attributes
                    query = new SelectQuery("MSFT_NetUDPEndpoint", null, new string[] { "LocalAddress", "LocalPort", "OwningProcess"});
                    
                    using (var searcher = new ManagementObjectSearcher(scope, query)) {
                        foreach (ManagementObject obj in searcher.Get()) {
                            if (obj != null) {
                                entry = new Dictionary<string,string>();
                                
                                entry.Add("protocol", "UDP");
                                entry.Add("state", "");
                                entry.Add("remote_address", "*:*");
                                entry.Add("pid", obj.GetPropertyValue("OwningProcess").ToString());
                                
                                if (obj.GetPropertyValue("LocalAddress").ToString().Contains(":")) {
                                    // IPv6 address
                                    entry.Add("local_address", "[" + obj.GetPropertyValue("LocalAddress").ToString() + "]:" + obj.GetPropertyValue("LocalPort").ToString());
                                } else {
                                    // IPv4 address
                                    entry.Add("local_address", obj.GetPropertyValue("LocalAddress").ToString() + ":" + obj.GetPropertyValue("LocalPort").ToString());
                                }
                                
                                entries.Add(entry);
                                
                                // Calculate the max length of the local address column (remote address and state and not populated)
                                if (entry["local_address"].Length > localAddrMaxSize) {
                                    localAddrMaxSize = entry["local_address"].Length;
                                }
                            }
                        }
                    }
                }
                
                
                // Add extra padding to separate the columns
                localAddrMaxSize += colPadding;
                remoteAddrMaxSize += colPadding;
                stateMaxSize += colPadding;
                
                List<string> output = new List<string>();
                string line;
                
                // Convert dictionary of network entries to a string with each column dynamically padded
                foreach (Dictionary<string,string> row in entries) {
                    line = "  " + row["protocol"] + "    ";
                    line += row["local_address"].PadRight(localAddrMaxSize);
                    line += row["remote_address"].PadRight(remoteAddrMaxSize);
                    line += row["state"].PadRight(stateMaxSize);
                    line += row["pid"];
                    
                    output.Add(line);
                }
                
                // Sort the output to make it look cleaner
                output.Sort();
                
                // Prepend table header after sorting
                line = "\nActive Connections\n\n  Proto  ";
                line += "Local Address".PadRight(localAddrMaxSize);
                line += "Foreign Address".PadRight(remoteAddrMaxSize);
                line += "State".PadRight(stateMaxSize);
                line += "PID";
                
                output.Insert(0, line);
                
                WriteOutput(outputFilepath, output);
            /*}
            catch (Exception e)
            {
                Console.WriteLine("[-] ERROR: {0}", e.Message.Trim());
            }
            */
            
            Console.WriteLine("\nDONE");
        }
    }
}