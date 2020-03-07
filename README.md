# SharpUtils
A collection of C# utilities intended to be used with Cobalt Strike's execute-assembly.

## Compilation
Open a PowerShell prompt and run the included `make.ps1` script. This will create a `bin\` directory containing the compiled executables; no fancy IDE required.

---

## Utilities

### arp.exe
Displays cached mappings between IP addresses and MAC addresses

#### Usage
    arp.exe

### auditpol.exe
Displays the computer's auditing policy (i.e., what actions will be logged)

**Requires administrative privileges**

#### Usage
    auditpol.exe

### env.exe
Displays environment variables (in alphabetical order)

#### Usage
    env.exe

### freespace.exe
Lists logical drives, including total and available free space. Mapped drives will only be shown when freespace.exe is run within the same session or with the same credentials used to map the drive.

#### Usage
    freespace.exe

### get-hotfix.exe
Lists patches, optionally including verbose information such as Description, InstalledBy, and InstalledOn.

#### Usage
    get-hotfix.exe [/S system] [/U [domain\]username /P password] [/V]

### lld.exe
Lists logical drives (using WMI), including total and available free space. Mapped drives will only be shown when lld.exe is run within the same session or with the same credentials used to map the drive. This differs from freespace.exe because it can be used against a remote system.

#### Usage
    lld.exe [system]

### pagegrab.exe
Makes a web request; can be used to check external connectivity or get the content of an internal web page without going through the trouble of setting up a SOCKS proxy.

Can optionally specify the HTTP method, POST request data, and/or a proxy server address/port. The `-v` flag causes the HTML contents of the page to be printed; if omitted, only the response headers will be shown.

#### Usage
    pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-v] <URL>
      -v  Print the HTML contents of the response

# Credits
Much of this code is derived from online examples. Where applicable, sources are listed as comments in the code.