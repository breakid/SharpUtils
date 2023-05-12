/*
 * USAGE: pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-v] <URL>
 *
 *    -v  Print the HTML contents of the response
 * 
 * EXAMPLES:
 *   pagegrab.exe -m post https://postman-echo.com/post -d query=test -v -p http://127.0.0.1:8000
 */

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:pagegrab.exe pagegrab.cs

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

// Needed for SSL / TLS certificate support
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class PageGrab
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || (args.Length > 0 && args[0] == "/?"))
            {
                // Print usage
                Console.WriteLine(@"USAGE: pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-h <header_1_name> <header_1_value> [-h <header_2_name> <header_2_value>]] [-c] [-v] <URL> [/?]
            
    -c  Display the SSL / TLS certificate
    -v  Print the HTML contents of the response");
                return 0;
            }

            string url = "";
            string edgeVersion = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{56EB18F8-B008-4CBD-B6D2-8C97FE7E9062}", "pv", "").ToString();
            string userAgent = "";
            string proxyAddress = "";
            string method = "GET";
            string postData = "";
            bool displaySSLCert = false;
            bool verbose = false;
            Dictionary<string, string> headers = new Dictionary<string, string>();

            List<string> unsupportedHeaders = new List<string>();
            unsupportedHeaders.Add("Date");
            unsupportedHeaders.Add("Host");
            unsupportedHeaders.Add("If-Modified-Since");

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "-C": // Display SSL / TLS certificate
                        displaySSLCert = true;
                        break;
                    
                    case "-D": // POST data
                        i++;

                        try
                        {
                            postData = args[i];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No POST data specified");
                        }
                        break;

                    case "-H": // Header (multiples are allowed)
                        i++;

                        // Throw an error and exit if an unsupported header is specified
                        if (i < args.Length && unsupportedHeaders.Contains(args[i]))
                        {
                            throw new ArgumentException("Unsupported header specified:" + args[i]);
                        }
                        else if (i + 1 < args.Length)
                        {
                            // Treat specified headers are a key-value pair
                            headers.Add(args[i++], args[i]);
                        }
                        else
                        {
                            throw new ArgumentException("Incomplete header specified");
                        }
                        break;

                    case "-M": // HTTP Method
                        i++;

                        if (i < args.Length)
                        {
                            method = args[i].ToUpper();
                        }
                        else
                        {
                            throw new ArgumentException("No HTTP Method specified");
                        }

                        string[] methods = { "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE" };

                        // Verify the method is a valid
                        if (Array.IndexOf(methods, method) == -1)
                        {
                            throw new ArgumentException(String.Format("Invalid method '{0}'", method));
                        }
                        break;

                    case "-P": // Proxy
                        i++;

                        if (i < args.Length)
                        {
                            proxyAddress = args[i];
                        }
                        else
                        {
                            throw new ArgumentException("No Proxy info specified");
                        }

                        Regex rex = new Regex(@"https?://.*:\d{1,5}", RegexOptions.IgnoreCase);

                        if (!rex.IsMatch(proxyAddress))
                        {
                            throw new ArgumentException("Invalid proxy configuration; use 'http(s)://<host>:<port>'");
                        }

                        break;
                    
                    case "-Q": // Query user agent
                        Console.WriteLine("[*] INFO: Edge version: " + edgeVersion);
                        return 0;

                    case "-V": // Verbose output
                        verbose = true;
                        break;

                    default: // URL
                        url = arg;
                        break;
                }
            }

            if (url.Length != 0)
            {
                // Initialize request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = method;

                // Add user-specified headers
                foreach (KeyValuePair<string, string> header in headers)
                {
                    switch (header.Key)
                    {
                        case "Accept":
                            request.Accept = header.Value;
                            break;
                        case "Connection":
                            request.Connection = header.Value;
                            break;
                        case "Content-Length":
                            request.ContentLength = long.Parse(header.Value);
                            break;
                        case "Content-Type":
                            request.ContentType = header.Value;
                            break;
                        case "Expect":
                            request.Expect = header.Value;
                            break;
                        case "Range":
                            string[] ranges = header.Value.Split('-');

                            if (ranges.Length >= 2)
                            {
                                if (ranges[0] == "")
                                {
                                    // Specified range is negative
                                    request.AddRange(int.Parse("-" + ranges[1]));
                                }
                                else
                                {
                                    // To and From are specified; additional ranges unsupported at this time
                                    request.AddRange(int.Parse(ranges[0]), int.Parse(ranges[1]));
                                }
                            }
                            else
                            {
                                // Specified range is negative
                                request.AddRange(int.Parse(ranges[0]));
                            }

                            break;
                        case "Referer":
                            request.Referer = header.Value;
                            break;
                        case "Transfer-Encoding":
                            request.TransferEncoding = header.Value;
                            break;
                        case "User-Agent":
                            userAgent = header.Value;
                            break;
                        default:
                            request.Headers[header.Key] = header.Value;
                            break;
                    }
                }

                // If no user agent is specified, pull the current version of Edge as a default
                // NOTE: This isn't perfect because the AppleWebKit / Safari version number won't match, but it's better probably better than sending nothing
                if (userAgent == "")
                {
                    if (edgeVersion == "")
                    {
                        throw new Exception("Unable to auto-detect Edge version; please specify a user agent");
                    }
                    else
                    {
                        userAgent = String.Format("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{0} Safari/537.36 Edg/{0}", edgeVersion);
                    }
                }

                request.UserAgent = userAgent;

                // Print request data
                Console.WriteLine("REQUEST");
                Console.WriteLine("---------------");
                Console.WriteLine(request.Method + " " + request.RequestUri);

                if (proxyAddress.Length != 0)
                {
                    // Manually set the proxy
                    WebProxy proxy = new WebProxy();
                    proxy.Address = new Uri(proxyAddress);
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    request.Proxy = proxy;
                    Console.WriteLine("Proxy: " + proxyAddress);
                }
                else
                {
                    // Detect the system proxy, if applicable
                    Uri resource = new Uri(url);

                    IWebProxy proxy = System.Net.WebRequest.GetSystemWebProxy();
                    Uri resourceProxy = proxy.GetProxy(resource);

                    // Test to see whether a proxy was selected
                    if (resourceProxy == resource)
                    {
                        Console.WriteLine("Proxy: None");
                    }
                    else
                    {
                        Console.WriteLine("Proxy: " + resourceProxy.ToString());
                    }
                }

                // Add POST data, if applicable
                if (method == "POST")
                {
                    request.ContentType = "application/x-www-form-urlencoded";

                    // Add the post data to the request
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    request.ContentLength = byteArray.Length;

                    Stream dataStream = request.GetRequestStream();
                    dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                Console.WriteLine(request.Headers);

                // Print POST data after headers
                if (method == "POST")
                {
                    Console.WriteLine(postData);
                }

                Console.WriteLine("\n\nRESPONSE");
                Console.WriteLine("---------------");

                // Get the response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Display the status and headers
                Console.WriteLine("Status: " + response.StatusDescription, Environment.NewLine);
                Console.WriteLine(response.Headers);

                // Optionally print the HTML from the response
                if (verbose)
                {
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();

                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);

                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    Console.WriteLine(responseFromServer);

                    // Cleanup the streams and the response.
                    reader.Close();
                    dataStream.Close();
                }

                response.Close();

                if (displaySSLCert) {
                    X509Certificate cert = request.ServicePoint.Certificate;

                    Console.WriteLine("SSL / TLS Certificate");
                    Console.WriteLine("---------------------");
                    Console.WriteLine("  Subject:           " + cert.Subject);
                    Console.WriteLine("  Issuer:            " + cert.Issuer);
                    Console.WriteLine("  Valid From:        " + cert.GetEffectiveDateString());
                    Console.WriteLine("  Valid To:          " + cert.GetExpirationDateString());
                    Console.WriteLine("  Serial Number:     " + cert.GetSerialNumberString());
                    Console.WriteLine("  SHA-1 Fingerprint: " + cert.GetCertHashString());
                    Console.WriteLine("  Public Key:        " + cert.GetPublicKeyString());
                    Console.WriteLine("");
                    Console.WriteLine("Certificate PEM:");
                    Console.WriteLine("----BEGIN CERTIFICATE----");
                    Console.WriteLine(System.Convert.ToBase64String(cert.GetRawCertData()));
                    Console.WriteLine("----END CERTIFICATE----");
                }
            }
            else
            {
                throw new ArgumentException("No URL specified");
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("[-] ERROR: {0}", e.Message.Trim());
            return 1;
        }
        finally
        {
            Console.WriteLine("\nDONE");
        }
    }
}