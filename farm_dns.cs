// Source: https://stackoverflow.com/questions/4172677/c-enumerate-ip-addresses-in-a-range/4172982
// Source: https://docs.microsoft.com/en-us/dotnet/api/system.net.dns.gethostbyaddress


// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:farm_dns.exe farm_dns.cs


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

/* ====================================================================================
                    C# IP address range finder helper class (C) Nahum Bazes
 * Free for private & commercial use - no restriction applied, please leave credits.
 *                              DO NOT REMOVE THIS COMMENT
 * ==================================================================================== */


namespace FarmDNS
{
    public class Program
    {
        public static IEnumerable<string> GetIPRange(IPAddress startIP, IPAddress endIP)
        {
            uint sIP = ipToUint(startIP.GetAddressBytes());
            uint eIP = ipToUint(endIP.GetAddressBytes());
            
            while (sIP <= eIP)
            {
                yield return new IPAddress(reverseBytesArray(sIP)).ToString();
                sIP++;
            }
        }


        /* reverse byte order in array */
        protected static uint reverseBytesArray(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            bytes = bytes.Reverse().ToArray();
            return (uint)BitConverter.ToInt32(bytes, 0);
        }


        /* Convert bytes array to 32 bit long value */
        protected static uint ipToUint(byte[] ipBytes)
        {
            ByteConverter bConvert = new ByteConverter();
            uint ipUint = 0;

            int shift = 24; // indicates number of bits left for shifting
            foreach (byte b in ipBytes)
            {
                if (ipUint == 0)
                {
                    ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                    shift -= 8;
                    continue;
                }

                if (shift >= 8)
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                else
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

                shift -= 8;
            }

            return ipUint;
        }
        
        
        public static IPAddress ParseIP(string ip)
        {
            try
            {
                return IPAddress.Parse(ip);
            }
            catch
            {
                Console.WriteLine("[-] ERROR: {0} is not a valid IP address", ip);
                System.Environment.Exit(1);
            }
            
            return null;
        }
        
        
        protected static void GetHostname(string ip)
        {
            Console.WriteLine(ip);
            
            try
            {
                IPAddress ipAddr = ParseIP(ip);
                IPHostEntry hostInfo = Dns.GetHostEntry(ipAddr);
                
                Console.WriteLine("  Hostname: {0}", hostInfo.HostName);
                
                // Get the IP address list that resolves to the host names contained in
                // the Alias property.
                IPAddress[] address = hostInfo.AddressList;
                
                // Get the alias names of the addresses in the IP address list.
                String[] aliases = hostInfo.Aliases;
                
                if (aliases.Length > 0)
                {
                    Console.WriteLine("  Aliases:");
                    for (int index = 0; index < aliases.Length; index++)
                    {
                      Console.WriteLine("    {0}", aliases[index]);
                    }
                }
                
                if (address.Length > 1)
                {
                    Console.WriteLine("\n  Other IPs:");
                    for (int index = 0; index < address.Length; index++)
                    {
                       Console.WriteLine("    {0}", address[index]);
                    }
                }
                
                // Create a new line to separate IPs
                Console.WriteLine("");
            }
            catch(Exception e)
            {
                Console.WriteLine("  [-] ERROR: {0}", e.Message);
            }
        }
        
        private static void PrintUsage()
        {
            Console.WriteLine(@"Performs reverse DNS lookups on the specified range of IP addresses (IPv4 only)
    
    USAGE:
        farm_dns.exe <start_IP> <end_IP>");
            Console.WriteLine("\nDONE");
        }
        
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }
            
            // Parse arguments
            IPAddress start = ParseIP(args[0]);
            IPAddress end = ParseIP(args[1]);
            
            Console.WriteLine("[*] Farming IPs from: {0} to {1}", start, end);
            
            foreach (string ip in GetIPRange(start, end))
            {
                GetHostname(ip);
            }
            
            Console.WriteLine("\nDONE");
        }
    }
}