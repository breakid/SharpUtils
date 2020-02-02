# SharpUtils
A collection of C# utilities intended to be used with Cobalt Strike's execute-assembly

## arp.exe
Displays cached mappings between IP addresses and MAC addresses

#### Usage
    arp.exe

## auditpol.exe
Displays the computer's auditing policy (i.e., what actions will be logged)

**Requires administrative privileges**

#### Usage
    auditpol.exe

## env.exe
Displays environment variables (in alphabetical order)

#### Usage
    env.exe

## freespace.exe
Lists logical drives, including total and available free space.

#### Usage
    freespace.exe

## pagegrab.exe
Makes a web request; can be used to check external connectivity or get the content of an internal web page without going through the trouble of setting up a SOCKS proxy.

Can optionally specify the HTTP method, POST request data, and/or a proxy server address/port. The `-v` flag causes the HTML contents of the page to be printed; if omitted, only the response headers will be shown.

#### Usage
    pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-v] <URL>
      -v  Print the HTML contents of the response

# Credits
Much of this code is derived from online examples. Where applicable, sources are listed as comments in the code.