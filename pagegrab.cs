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

public class PageGrab {
    public static int Main(string[] args) {
        if (args.Length == 0) {
            // Print usage
            Console.WriteLine(@"USAGE: pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-v] <URL>
            
    -v  Print the HTML contents of the response
            ");
            
            Console.WriteLine("\nDONE");
            return 0;
        }
        
        string url = "";
        string edgeVersion = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{56EB18F8-B008-4CBD-B6D2-8C97FE7E9062}", "pv", "").ToString();
        string userAgent = "";
        string proxyAddress = "";
        string method = "GET";
        string postData = "";
        bool verbose = false;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        
        List<string> unsupportedHeaders = new List<string>();
        unsupportedHeaders.Add("Date");
        unsupportedHeaders.Add("Host");
        unsupportedHeaders.Add("If-Modified-Since");
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
            
            switch (arg.ToUpper()) {
                case "-D": // POST data
                    i++;
                    
                    if (i < args.Length) {
                        postData = args[i];
                    } else {
                        Console.WriteLine("[-] ERROR: No POST data specified");
                        Console.WriteLine("\nDONE");
                        return 1;
                    }
                    break;
                
                case "-H": // Header (multiples are allowed)
                    i++;
                    
                    // Throw an error and exit if an unsupported header is specified
                    if (i < args.Length && unsupportedHeaders.Contains(args[i])) {
                        Console.WriteLine("[-] ERROR: Unsupported header specified:" + args[i]);
                        Console.WriteLine("\nDONE");
                        return 1;
                    } else if (i + 1 < args.Length) {
                        // Treat specified headers are a key-value pair
                        headers.Add(args[i++], args[i]);
                    } else {
                        Console.WriteLine("[-] ERROR: Incomplete header specified");
                        Console.WriteLine("\nDONE");
                        return 1;
                    }
                    break;
                
                case "-M": // HTTP Method
                    i++;
                    
                    if (i < args.Length) {
                        method = args[i].ToUpper();
                    } else {
                        Console.WriteLine("[-] ERROR: No HTTP Method specified");
                        Console.WriteLine("\nDONE");
                        return 1;
                    }
                    
                    string[] methods = {"GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE"};
                    
                    // Verify the method is a valid
                    if (Array.IndexOf(methods, method) == -1) {
                        Console.WriteLine("[-] ERROR: Invalid method '{0}'", method);
                        Console.WriteLine("\nDONE");
                        return 1;
                    }
                    break;
                
                case "-P": // Proxy
                    i++;
                    
                    if (i < args.Length) {
                        proxyAddress = args[i];
                    } else {
                        Console.WriteLine("[-] ERROR: No Proxy info specified");
                        Console.WriteLine("\nDONE");
                        return 1;
                    }
                    
                    Regex rex = new Regex(@"https?://.*:\d{1,5}", RegexOptions.IgnoreCase);
                    
                    if (!rex.IsMatch(proxyAddress)) {
                        Console.WriteLine("[-] ERROR: Invalid proxy configuration; use 'http(s)://<host>:<port>'");
                        Console.WriteLine("\nDONE");
                        return 1;
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
        
        if (url.Length != 0) {
            try {
                // Initialize request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = method;
                
                // Add user-specified headers
                foreach (KeyValuePair<string, string> header in headers) {
                    switch(header.Key) {
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
                            
                            if (ranges.Length >= 2) {
                                if (ranges[0] == "") {
                                    // Specified range is negative
                                    request.AddRange(int.Parse("-" + ranges[1]));
                                } else {
                                    // To and From are specified; additional ranges unsupported at this time
                                    request.AddRange(int.Parse(ranges[0]), int.Parse(ranges[1]));
                                }
                            } else {
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
                // NOTE: This isn't perfect because the Chrome version number won't match, but it's better probably better than sending nothing
                if (userAgent == "") {
                    if (edgeVersion == "") {
                        Console.WriteLine("[-] ERROR: Unable to auto-detect Edge version; please specify a user agent");
                        Console.WriteLine("\nDONE");
                        return 1;
                    } else {
                        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36 Edg/" + edgeVersion;
                    }
                }
                
                request.UserAgent = userAgent;
                
                
                // Print request data
                Console.WriteLine("REQUEST");
                Console.WriteLine("---------------");
                Console.WriteLine(request.Method + " " + request.RequestUri);
                
                if (proxyAddress.Length != 0) {
                    // Manually set the proxy
                    WebProxy proxy = new WebProxy();
                    proxy.Address = new Uri(proxyAddress);
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    request.Proxy = proxy;
                    Console.WriteLine("Proxy: " + proxyAddress);
                } else {
                    // Detect the system proxy, if applicable
                    Uri resource = new Uri(url);
                    
                    IWebProxy proxy = System.Net.WebRequest.GetSystemWebProxy();
                    Uri resourceProxy = proxy.GetProxy(resource);
                    
                    // Test to see whether a proxy was selected
                    if (resourceProxy == resource) {
                        Console.WriteLine("Proxy: None");
                    } else {
                        Console.WriteLine("Proxy: " + resourceProxy.ToString());
                    }
                }
                
                // Add POST data, if applicable
                if (method == "POST") {
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
                if (method == "POST") {
                    Console.WriteLine(postData);
                }
                
                
                Console.WriteLine("\n\nRESPONSE");
                Console.WriteLine("---------------");
                
                // Get the response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
                // Display the status and headers
                Console.WriteLine("Status: " + response.StatusDescription + "\n");
                Console.WriteLine(response.Headers);
                
                // Optionally print the HTML from the response
                if (verbose) {
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
                
                response.Close ();
            } catch (WebException e) {
                Console.WriteLine("\n\n[-] ERROR: " + e.Message);
            }
            
            Console.WriteLine("\nDONE");
            return 0;
        } else {
            Console.WriteLine("[-] ERROR: No URL specified");
        }
        
        Console.WriteLine("\nDONE");
        return 1;
    }
}