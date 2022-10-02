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
            throw new ArgumentException(String.Format("{0} is not a valid IP address", ip));
        }
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
        catch (Exception e)
        {
            Console.Error.WriteLine("  [-] ERROR: {0}", e.Message.Trim());
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine(@"Performs reverse DNS lookups on the specified range of IP addresses (IPv4 only)
    
USAGE:
    farm_dns.exe [/T <seconds_to_sleep>] <start_IP> <end_IP> [/?]");
    }

    public static void Main(string[] args)
    {
        try
        {
            if (args.Length >= 2)
            {
                int throttle = 0;
                IPAddress start = null;
                IPAddress end = null;

                // Parse arguments
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    switch (arg.ToUpper())
                    {
                        case "-T":
                        case "/T":
                            i++;

                            try
                            {
                                // Catch error while attempting to parse the throttle time to prevent exception
                                if (int.TryParse(args[i], out throttle) == false)
                                {
                                    throw new ArgumentException("Invalid throttle");
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new ArgumentException("No throttle specified");
                            }
                            break;
                        case "/?":
                            PrintUsage();
                            return;
                        default:
                            try
                            {
                                start = ParseIP(args[i++]);

                                if (i < args.Length)
                                {
                                    end = ParseIP(args[i]);
                                }
                            }
                            catch (Exception e)
                            {
                                throw new ArgumentException("Invalid start or end value");
                            }
                            break;
                    }
                }

                if (start != null && end != null)
                {
                    Console.WriteLine("[*] Farming IPs from: {0} to {1}", start, end);

                    foreach (string ip in GetIPRange(start, end))
                    {
                        GetHostname(ip);

                        // Sleep for throttle seconds
                        if (throttle > 0)
                        {
                            System.Threading.Thread.Sleep(throttle * 1000);
                        }
                    }
                }
                else
                {
                    throw new Exception("Start and/or end address not specified");
                }
                return;
            }

            PrintUsage();
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
