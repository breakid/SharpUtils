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

using System;
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
        string proxyAddress = "";
        string method = "GET";
        string postData = "";
        bool verbose = false;
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
        
            switch (arg.ToUpper()) {
                case "-D": // POST data
                    i++;
                    postData = args[i];
                    break;
                    
                case "-M": // HTTP Method
                    i++;
                    method = args[i].ToUpper();
                    
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
                    proxyAddress = args[i];
                    
                    Regex rex = new Regex(@"https?://.*:\d{1,5}", RegexOptions.IgnoreCase);
                    
                    if (!rex.IsMatch(proxyAddress)) {
                        Console.WriteLine("[-] ERROR: Invalid proxy configuration; use 'http(s)://<host>:<port>'");
                        Console.WriteLine("\nDONE");
                        return 1;
                    }
                    
                    break;
                    
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
                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = method;
                
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