// Source: https://github.com/dev-2null/ADIDNSRecords
// Source: https://stackoverflow.com/questions/1315758/specify-which-dns-servers-to-use-to-resolve-hostnames-in-net

/*
 * Original code based on https://github.com/dev-2null/ADIDNSRecords
 *   - Modified to allow the DNS server to be manually specified (such as if 
 *     running from a non-domain joined system or a host in a different DNS domain)
 *   - Original auto-detection code maintained as default if no server is specified
 *   - Modified to allow queries against specific domains or forests (each can be specified independently)
 *   - Improved the fallback DNS resolution to resolve against the explicitly set DNS server
 *     - Since there is no native .Net function to resolve hostnames against a custom DNS server, used code from
 *       https://stackoverflow.com/questions/1315758/specify-which-dns-servers-to-use-to-resolve-hostnames-in-net 
 *       to manually create a DNS request and send via a raw socket
 *   - Added an option to write output to a file (writes to STDOUT by default)
 */

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:dump_dns.exe dump_dns.cs


using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DumpDNS
{
    public class DumpDNS
    {
        public static List<string> output = new List<string>();
        public static IPAddress dnsAddr;
        
        public static void PrintUsage() {
            Console.WriteLine(@"Dumps DNS zone data
            
    USAGE:
        dump_dns.exe [/S <server>] [/D <domain_name>] [/F <forest_name>] [/T] [/O <output_filepath>]
        
        /T    Optionally include Tombstoned records");
            Console.WriteLine("\nDONE");
        }
        
        public static void Main(string[] args)
        {
            //domain dns Dn
            string dDnsDn = "DC=DomainDnsZones,";

            //forest dns Dn
            string fDnsDn = "DC=ForestDnsZones,";
            
            // DNS server to query
            string server = "";
            
            // X.500 Distinguished Name of domain and forest
            string dDomain = "";
            string dForest = "";
            
            // Fully-qualified domain and forest DNS names
            string domainName = "";
            string forestName = "";
            
            // Whether or not Tombstoned records should be included
            bool includeTombstoned = false;
            
            // File path where output should be written (optional)
            string outputFilepath = "";
            
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                
                switch (arg.ToUpper()) {
                    case "-S":
                    case "/S":
                        i++;
                        server = args[i];
                        VerifyDNSAddr(server);
                        break;
                    case "-D":
                    case "/D":
                        i++;
                        domainName = args[i];
                        dDomain = "DC=" + domainName.Replace(".", ",DC=");
                        break;
                    case "-F":
                    case "/F":
                        i++;
                        forestName = args[i];
                        dForest = "DC=" + forestName.Replace(".", ",DC=");
                        break;
                    case "-T":
                    case "/T":
                        includeTombstoned = true;
                        break;
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
                    case "/?":
                        PrintUsage();
                        return;
                }
            }
            
            // Auto-detect domain and forest name if not provided
            if (domainName == "" && forestName == "" )
            {
                Console.WriteLine("[*] Auto-detecting domain and forest");
                
                try {
                    DirectoryEntry rootEntry = null;
                    
                    if (server != "")
                    {
                        rootEntry = new DirectoryEntry("LDAP://" + server);
                        rootEntry.AuthenticationType = AuthenticationTypes.None;
                    }
                    else
                    {
                        rootEntry = new DirectoryEntry("LDAP://rootDSE");
                    }
                    
                    if (rootEntry.Properties.Contains("defaultNamingContext") && rootEntry.Properties.Contains("rootDomainNamingContext"))
                    {
                        // Current domain DN
                        dDomain = (string)rootEntry.Properties["defaultNamingContext"].Value;
                        
                        // Current forest DN
                        dForest = (string)rootEntry.Properties["rootDomainNamingContext"].Value;
                    }
                    else
                    {
                        // Current domain DN
                        dDomain = (string)rootEntry.Properties["distinguishedName"].Value;
                        
                        // Current forest DN
                        dForest = (string)rootEntry.Properties["distinguishedName"].Value;
                    }
                    
                    // Convert Distinguished Name to DNS name
                    domainName = dDomain.Replace("DC=", "").Replace(",", ".");
                    forestName = dForest.Replace("DC=", "").Replace(",", ".");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[-] ERROR: {0}", e.Message);
                }
            }
            
            if (domainName == "" && forestName == "" )
            {
                Console.WriteLine("[-] ERROR: Auto-detection failed; please specify a domain and/or forest name");
                System.Environment.Exit(1);
            }
            
            string dDnsRoot = dDnsDn + dDomain;
            string fDnsRoot = fDnsDn + dForest;
            
            // Allow Domain and Forest zones to be queries independently
            if (domainName != "")
            {
                output.Add(String.Format("\n[*] Domain: {0}", domainName));
                try
                {
                    GetDNS(server, domainName, dDnsDn, dDnsRoot, includeTombstoned);
                }
                catch (Exception e)
                {
                    output.Add(String.Format("[-] ERROR: {0}", e.Message));
                }
            }
            
            if (forestName != "")
            {
                output.Add(String.Format("\n[*] Forest: {0}", forestName));
                try
                {
                    GetDNS(server, forestName, fDnsDn, fDnsRoot, includeTombstoned);
                }
                catch (Exception e)
                {
                    output.Add(String.Format("[-] ERROR: {0}", e.Message));
                }
            }
            
            WriteOutput(outputFilepath);
            
            Console.WriteLine("\nDONE");
        }
        
        
        public static void VerifyDNSAddr(string server)
        {
            // Resolve DNS server if it's not already an IP address
            if (!IPAddress.TryParse(server, out dnsAddr))
            {
                try
                {
                    server = GetIP(server);
                }
                catch
                {
                    // Suppress error message
                }
                
                if (!IPAddress.TryParse(server, out dnsAddr))
                {
                    Console.WriteLine("[-] ERROR: DNS server ({0}) could not be resolved; please specify an IP address", server);
                    System.Environment.Exit(1);
                }
            }
        }
        
        
        // FQN       :   domain.local
        // dnsDn     :   DC=ForestDnsZones,
        // dnsRoot   :   DC=ForestDnsZones,DC=domain,DC=local
        // bool      :   true (include tombstoned records or not)
        public static void GetDNS(string server, string FQN, string dnsDn, string dnsRoot, bool includeTombstoned)
        {
            // If no server was explicitly specified, default to the domain or forest being queried
            // Resolve IP so you know specifically which server was queried
            if (server == "") {
                server = GetIP(FQN);
            }
            
            //Console.WriteLine("GetDNS('{0}', '{1}', '{2}', '{3}', '{4}')", server, FQN, dnsDn, dnsRoot, includeTombstoned); // DEBUG
            
            Dictionary<string, byte[]> hostList = new Dictionary<string, byte[]>();
            List<string> privhostList = new List<string>();
            string hostname = null;
            
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + server + "/" + dnsRoot);
                
                // Find DNS Zones
                String queryZones = @"(&(objectClass=dnsZone)(!(DC=*arpa))(!(DC=RootDNSServers)))";
                
                DirectorySearcher searchZones = new DirectorySearcher(entry, queryZones);
                
                searchZones.SearchScope = SearchScope.Subtree;
                
                foreach (SearchResult zone in searchZones.FindAll())
                {
                    output.Add("----------------------------------------------------------");
                    output.Add(String.Format(" *  Querying Server: {0}", server));
                    output.Add(String.Format(" *  DNS Zone: {0}", zone.Properties["Name"][0]));
                    output.Add("----------------------------------------------------------");
                    
                    DirectoryEntry zoneEntry = new DirectoryEntry(zone.Path);
                    
                    // Exclude objects that have been removed
                    String queryRecord = @"(&(objectClass=*)(!(DC=@))(!(DC=*DnsZones))(!(DC=*arpa))(!(DC=_*))(!dNSTombstoned=TRUE))";
                    
                    DirectorySearcher searchRecord = new DirectorySearcher(zoneEntry, queryRecord);
                    
                    searchRecord.SearchScope = SearchScope.OneLevel;
                    
                    foreach (SearchResult record in searchRecord.FindAll())
                    {
                        if (record.Properties.Contains("dnsRecord"))
                        {
                            if (record.Properties["dnsRecord"][0] is byte[])
                            {
                                var dnsByte = ((byte[])record.Properties["dnsRecord"][0]);
                                var key = record.Properties["DC"][0] + "." + FQN;
                                
                                // Resolve every record in case there are duplicate mappings
                                ResolveDNSRecord(key, server, dnsByte);
                                
                                //if (!hostList.ContainsKey(key))
                                //{
                                //    hostList.Add(key, dnsByte);
                                //}
                            }
                        }
                        // No permission to view records
                        else
                        {
                            string DN = ",CN=MicrosoftDNS," + dnsDn;
                            int end = record.Path.IndexOf(DN);
                            string ldapheader = "LDAP://" + server + "/";
                            
                            hostname = record.Path.Substring(0, end).Replace(ldapheader, "").Replace("DC=", "").Replace(",", ".");
                            
                            // Eliminate unnecessary entries that sometimes appear
                            if (hostname.StartsWith("DomainDnsZones") || hostname.StartsWith("ForestDnsZones"))
                            {
                                continue;
                            }
                            
                            if (!privhostList.Contains(hostname))
                            {
                                privhostList.Add(hostname);
                            }
                        }
                    }
                }
                
                // Iterating each entry
                foreach (KeyValuePair<string, byte[]> host in hostList)
                {
                    ResolveDNSRecord(host.Key, server, host.Value);
                }
                foreach (var host in privhostList)
                {
                    PrintIP(host, server, includeTombstoned);
                }
            }
            catch (Exception e)
            {
                output.Add(String.Format("[-] ERROR: {0}", e.Message));
            }
        }
        
        
        // Retrieve IP from LDAP dnsRecord
        public static void ResolveDNSRecord(string hostname, string server, byte[] dnsByte)
        {
            var rdatatype = dnsByte[2];
            string ip = null;
            
            if (rdatatype == 1)
            {
                ip = dnsByte[24] + "." + dnsByte[25] + "." + dnsByte[26] + "." + dnsByte[27];
            }
            
            // If ip is still null, fall back to normal DNS resolution
            if (ip == null)
            {
                ip = GetIP(hostname, server);
            }
            
            output.Add(String.Format("    {0,-40}           {1,-40}", hostname, ip));
        }
        
        
        // Save formatted strings to output list
        public static void PrintIP(string hostname, string server, bool includeTombstoned)
        {
            try
            {
                string ip = GetIP(hostname, server);
                
                output.Add(String.Format("    {0,-40}           {1,-40}", hostname, ip));
            }
            catch (Exception)
            {
                if (includeTombstoned)
                {
                    output.Add(String.Format("    {0,-40}           {1,-40}", hostname, "Tombstone"));
                }
            }
        }
        
        
        // Retrieve IP from DNS
        public static string GetIP(string hostname)
        {
            return Dns.GetHostEntry(hostname).AddressList[0].ToString();
        }
        
        
        // Resolve IP using custom DNS server
        // Unfortunately there is no native way to do this in C# so get ready to handcraft a DNS request...
        public static string GetIP(string hostname, string server)
        {
            // Credit where credit is due
            // Source: https://stackoverflow.com/questions/1315758/specify-which-dns-servers-to-use-to-resolve-hostnames-in-net
            
            // Ensure dnsAddr is set if the server was auto-detected
            VerifyDNSAddr(server);
            
            List<string> addresses = new List<string>();
            
            using (MemoryStream ms = new MemoryStream())
            {
                Random rnd = new Random();
                // About the dns message:http://www.ietf.org/rfc/rfc1035.txt

                // Write message header.
                ms.Write(new byte[] {
                    (byte)rnd.Next(0, 0xFF),(byte)rnd.Next(0, 0xFF),
                    0x01,
                    0x00,
                    0x00,0x01,
                    0x00,0x00,
                    0x00,0x00,
                    0x00,0x00
                }, 0, 12);

                // Write the hostname to query.
                foreach (string block in hostname.Split('.'))
                {
                    byte[] data = Encoding.UTF8.GetBytes(block);
                    ms.WriteByte((byte)data.Length);
                    ms.Write(data, 0, data.Length);
                }
                
                // The end of query, must end with 0(null string)
                ms.WriteByte(0);
                
                //Query type:A
                ms.WriteByte(0x00);
                ms.WriteByte(0x01);
                
                //Query class:IN
                ms.WriteByte(0x00);
                ms.WriteByte(0x01);
                
                Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                try
                {
                    // Send request to DNS server
                    byte[] buffer = ms.ToArray();
                    while (socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, new IPEndPoint(dnsAddr, 53)) < buffer.Length);
                    buffer = new byte[0x100];
                    EndPoint ep = socket.LocalEndPoint;
                    
                    // Receive response
                    int num = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref ep);
                    
                    // The response message has the same header and question structure, so we can move the index directly to the answer section
                    int index = (int)ms.Length;
                    int length;
                    
                    while (index < num)
                    {
                        // The name of the record is useless, so we just need to get the next index after name.
                        while (index < num)
                        {
                            length = buffer[index++];
                            if (length == 0)
                            {
                                break;
                            }
                            else if (length > 191)
                            {
                                break;
                            }
                            index += length;
                        }
                        
                        byte type = buffer[index += 2];
                        
                        // Skip class and ttl
                        index += 7;
                        
                        // Get record data's length
                        length = buffer[index++] << 8 | buffer[index++];
                        
                        if (type == 0x01) // A record
                        {
                            // Parse record data to IPv4
                            if (length == 4)
                            {
                                addresses.Add(new IPAddress(new byte[] { buffer[index], buffer[index + 1], buffer[index + 2], buffer[index + 3] }).ToString());
                            }
                        }
                        
                        index += length;
                    }
                }
                finally
                {
                    socket.Dispose();
                }
            }
            
            return String.Join(", ", addresses);
        }
        
        
        // Write output to file, if specified; otherwise output to screen
        public static void WriteOutput(string outputFilepath)
        {
            if (outputFilepath != "")
            {
                if (!File.Exists(outputFilepath))
                {
                    Console.WriteLine("[*] Writing output to: {0}", outputFilepath);
                    
                    try
                    {
                        System.IO.File.WriteAllLines(outputFilepath, output);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] ERROR: {0}", e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("[-] ERROR: Output file exists");
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
    }
}