# SharpUtils
A collection of C# utilities intended to be used with Cobalt Strike's execute-assembly.

When feasible, I have tried to emulate native Windows output formats for matching commands to aid potential text parsing and reduce the learning curve for users familiar with Windows commands. One caveat to this is that the word "DONE" is printed at the end of every execution; this is to provide confirmation that the command completed successfully and all data was received.

## Compilation
Open a PowerShell prompt and run the included `make.ps1` script. This will create a `bin\` directory containing the compiled executables; no fancy IDE required.

Whenever possible, the oldest version of .NET is used for compatibility with older systems.

I opted to use the C# Command-Line Compiler (csc.exe) instead of Visual Studio to lower the barrier to entry. That is, users should be able to simply clone the repo and build the utilities with little to no configuration, rather than having to download, install, and configure a full development suite.

---

## Utilities

### arp.exe
Displays cached mappings between IP addresses and MAC addresses

#### Usage
    arp.exe [/?]



### auditpol.exe
Displays the computer's auditing policy (i.e., what actions will be logged)

**Requires administrative privileges**

#### Usage
    auditpol.exe [/?]



### create_process.exe
Uses WMI to execute a command on the specified remote host

**Uses WMI**

**Requires administrative privileges on the remote host**

#### Usage
    create_process.exe <host> <full_path_to_exe_on_remote_host> <executable_arguments>



### dsquery.exe
Queries directory services (e.g., Active Directory). Only complex queries (dsquery *) are supported at this time, and the '-filter' option is required.

- Can provide a space-delimited list of attributes to return or '*' to get all attributes. If the '-attr' argument is omitted, it defaults to '-attr *'
- Can optionally limit the number of results returned using '-limit #'
- Can optionally query a specified server (-s) or domain (-d); if both are provided, it will default to the specified server
- Outputs in native table format by default (unless '-attr *' is specified, in which case list format is used); list format can be specified using the '-l' option. A '-t' option prints in table format with ASCII borders around each cell.
- Can optionally provide a username and password to use for the query (-u and -p, respectively)
- Can optionally provide a start node such as a specific OU; forestroot and domainroot are NOT supported at this time
- Can optionally provide an output filepath (-o) where the results will be written; the '-b' option allows users to specify a buffer size in MB for the file writer

The number of results returned by the search will be printed at the top of the output; while this deviates from the native output format, it can be very useful to help gauge the size of your query. Similarly the '-c' (count only) option was included for this reason.

This can be run **without administrative privileges from any system that can communicate with directory services on the intended target**. If run from a domain-joined host, it will automatically run against the current domain unless the '-s' or '-d' options are specified. If run from a host that is not joined to a domain, the '-s' or '-d' options must be specified. The '-u' and '-p' options may be required as well if not running with domain credentials (e.g., from a runas session).

#### Usage
    dsquery * [startNode] -filter <filter_string> [-attr <attributes>] [-limit <number>] [-c | -l | -t] [[-s <server>] | [-d <domain>]] [-u <UserName>] [-p <password>] [-b <buffer_size_in_MB>] [-o <output_file>] [/?]
    
        [startNode]             Optional start node (e.g., specific OU; forestroot and domainroot 
                                are NOT supported at this time)
        -filter <filter>        Standard dsquery filter string
        -attr <attributes>      Space-delimited list of attributes; use '*' to return all attributes
                                If omitted, defaults to '-attr *'
        -limit <number>         Limits query to <number> records
        -c                      Count only; prints the number of records returned by a search and exits
        -l                      Print in list format
        -t                      Print in table format with ASCII borders around each cell
        -s <server>             Query the specified server
        -d <domain>             Query the specified domain
        -u <username>           Authenticate using the specified username
        -p <password>           Authenticate using the specified password
        -o <output_filepath>    Write output to the specified file; will not overwrite an existing file
        -b <buffer_size>        Write to output file in 'buffer_size' chunks (specified in MB)
        /?                      Prints help



### driver_list.exe
Displays info about installed drivers. Name and signer are displayed by default; normal verbose flag (/V) adds version number and DeviceID, and the very verbose flag (/VV) lists all driver properties.

**Uses WMI**

#### Usage
    driver_list.exe [/V | /VV] [/?]



### dump_dns.exe
Uses Active Directory Search Interface (ADSI) to dump DNS data from Active Directory. If the DNS records cannot be parsed, it will fallback to performing DNS requests for individual hostnames.

It can be run with no arguments from a domain-joined host with normal user credentials. If running from a non-domain-joined host, use the "/s" option to specify a server to query. The "/d" and "/f" options can be used to specify specific domains and forests to dump.

Users can optionally specify an output file using "/o"; if omitted, output will be written to STDOUT.

Since it queries Active Directory, it may need to be run using domain user credentials (non-privileged will suffice). If necessary and missing, a "[-] ERROR: The user name or password is incorrect." message will be displayed.

#### Usage
    dump_dns.exe [/S <dns_server>] [/D <domain_name>] [/F <forest_name>] [/T] [/O <output_filepath>] [/?]
    
    /T    Optionally include tombstoned values



### env.exe
Displays environment variables (in alphabetical order)

#### Usage
    env.exe [/?]



### eventlog.exe
Reads events from the specified log file; optionally limit by EventID and number of events returned

#### Usage
    eventlog.exe <log name> [/C <count>] [/E <event IDs (comma-separated, no space)>] [/?]



### farm_dns.exe
Performs reverse DNS lookups on the specified range of IP addresses (IPv4 only). Opted to remove the output file support from the original design because all data was written at the end; if a user choose to end the farming early all data would be lost. Writing to STDOUT in real-time allows a user to end the farming and resume later with the IP address where they left off without losing any data.

#### Usage
    farm_dns.exe [/T <seconds_to_sleep>] <start_IP> <end_IP> [/?]



### freespace.exe
Lists logical drives, including total and available free space. Mapped drives will only be shown when freespace.exe is run within the same session or with the same credentials used to map the drive.

#### Usage
    freespace.exe [/?]



### get_chrome_tab_info.exe
Displays the URL of the current tab and the titles of all tabs in foremost Chrome window. Will NOT display information about other Chrome windows.

**Currently Broken**

#### Usage
    get_chrome_tab_info.exe [/?]



### get_hotfix.exe
Lists patches, optionally including verbose information such as Description, InstalledBy, and InstalledOn.

#### Usage
    get_hotfix.exe [/S <system>] [/U [<domain>\]<username> /P <password>] [/V] [/?]



### icacls.exe
Displays permissions, grouped by user/group, for each file or directory specified

#### Usage
    icacls.exe <file_or_directory> [...] [/?]



### lld.exe
Lists logical drives (using WMI), including total and available free space. Mapped drives will only be shown when lld.exe is run within the same session or with the same credentials used to map the drive. This differs from freespace.exe because it can be used against a remote system.

**Uses WMI**
**Requires administrative privileges when used against a remote host**

#### Usage
    lld.exe [system] [/?]



### netstat.exe
Lists listening TCP and UDP ports, and active TCP connections (equivalent to 'netstat -ano'); optionally writes output to a file. TCP or UDP can be specified to show only the matching data.

**Uses WMI**

#### Usage
    netstat.exe [/S <system> [/U [<domain>\]<username> /P <password>]] [/O <output_filepath>] [TCP | UDP] [/?]

#### Examples
    netstat.exe
    
    netstat.exe udp
    
    netstat.exe -S DC01.MGMT.LOCAL tcp
    
    netstat.exe -S DC01.MGMT.LOCAL -U MGMT\Administrator -P password



### pagegrab.exe
Makes a web request; can be used to check external connectivity or get the content of an internal web page without going through the trouble of setting up a SOCKS proxy.

Can optionally specify the HTTP method, POST request data (if applicable), a proxy server/port, and an arbitrary number of headers.

Custom headers are supported; however, `Date`, `Host`, and `If-Modified-Since` are not. Headers are case sensitive and will be sent the way they are specified on the command-line. Headers must be specified in key-value pairs (header name, header value). If no `User-Agent` is specified, a hardcoded User-Agent string using the current version of Edge will be used; this is not perfect since the current version of Edge will not match the Chrome version, but it's probably better than sending no user-agent.

The `Proxy` header will always show in the printed output; however, if the value is `None`, no `Proxy` header was sent as part of the request.

The `-v` flag causes the HTML contents of the page to be printed; if omitted, only the response headers will be shown. 

#### Usage
    pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-h <header_1_name> <header_1_value> [-h <header_2_name> <header_2_value>]] [-v] <URL> [/?]
      -v  Print the HTML contents of the response

#### Examples
    pagegrab.exe https://google.com -h User-Agent "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36 Edg/89.0.774.75"
    
    GET https://example.com
    User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36 Edg/89.0.774.75
    
    pagegrab.exe https://example.com -h Referer https://www.google.com -h DNT 1 -h Cache-Control no-cache
        
    GET https://example.com
    Proxy: None
    Referer: https://www.google.com
    DNT: 1
    Cache-Control: no-cache
    User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36 Edg/92.0.902.67



### readfile.exe
Read the contents of a file; optionally limit to X number of lines from the beginning of the file or Y number of lines from the end

#### Usage
    readfile.exe [+X] [-Y] <path_to_file> [/?]



### tasklist_svc.exe
Lists processes, their PIDs, and their associated services (if applicable)

**Uses WMI**

#### Usage
    tasklist_svc.exe [/?]



### tasklist_wmi.exe
List processes on local or remote system, optionally filter by ID, name, or WQL query.

When using /FI, you must provide a Win32_Process-compatible WMI query language (WQL) condition string rather than a standard tasklist filter. You may use '%' as a wildcard

Verbose mode (/V) will return the user name under which the process is running; however, this uses .NET reflection (InvokeMethod) which can be slow.

**Uses WMI**
**Uses .NET Reflection**
**Requires administrative privileges if used against a remote host and for some local tasks (i.e., retrieving user context for processes owned by other users)**

#### Usage
    tasklist_wmi.exe [/S <system> [/U [<domain>\]<username> /P <password>]] [ [/PID <processid> | /IM <imagename> | /FI <"WQL where clause">] ] [/V] [/D "<delimiter>"] [/?]

#### Examples
    tasklist_wmi.exe /v
    
    tasklist_wmi.exe /S 192.168.20.10 /FI "Name Like 'cmd%'"
    
    tasklist_wmi.exe /S 192.168.20.10 /FI "CommandLine Like '%svchost%'"
    
    tasklist_wmi.exe /S 192.168.20.10 /U Desktop-624L8K3\Administrator /P password /FI "CommandLine Like '%svchost%'"



### taskkill.exe
Kills one or more processes by PID or imagename; multiple PIDs should be provided as a comma-separated list. Optionally, target a remote host and/or provide plaintext username and password from the command-line.

**Uses WMI**
**Requires administrative privileges if used against a remote host and for some local tasks (i.e., killing processes with a different owner)**

#### Usage
    taskkill.exe [/S <system> [/U [<domain>\]<username> /P <password>]] { [/PID <processid> | /IM <imagename>] } [/?]

#### Examples
    taskkill.exe /PID 964
    
    taskkill.exe /PID 340,1432
    
    taskkill.exe /IM Calculator.exe
    
    taskkill.exe /IM Calculator.exe /S 192.168.2.14



### test_ad_creds.exe
Authenticates against the specified Active Directory (AD) domain using the provided username and password; indicates whether the credentials are valid or not. Can optionally specify a specific server with '/S'. Does not work with local credentials, only AD creds.

**This WILL create a failed logon event if the credentials are not valid; use sparingly to avoid account lockout**

#### Usage
    test_ad_creds.exe [/S <server>] <domain> <username> <password> [/?]



### window_list.exe
Displays a list of visible windows and their associated process.

**BETA: Has only been tested locally under Medium and High Integrity accounts. SYSTEM-level access *may* provide information from multiple sessions, and remote access *may* provide results (initial testing resulted in "Network path not found" errors)**

#### Usage
    window_list.exe [/S <host>] [/?]



### wmi_query.exe
Runs the specified WMI query and displays properties in key-value pairs. The query string must be in quotes. The default server is localhost, and the default namespace is "root\cimv2".

**Uses WMI** (...obviously)

#### Usage
    wmi_query.exe [/S <system> [/U [<domain>\]<username> /P <password>]] [-N <namespace>] [/O <output_filepath>] [/V] "<wmi_query>" [/?]

#### Examples
    wmi_query.exe "Select * from win32_process"
    
    wmi_query.exe -S DC01.MGMT.LOCAL -N root\standardcimv2 "Select * from MSFT_NetTCPConnection"
    
    wmi_query.exe -S DC01.MGMT.LOCAL -U MGMT\Administrator -P password -N root\standardcimv2 "Select * from MSFT_NetTCPConnection"



---

# Credits
Much of this code is derived from online examples. Where applicable, sources are listed as comments in the code.

---

# Other Resources
Below is a list of other utilities that I have collected (though not personally tested) that may be of interest.

* [Covenant](https://github.com/cobbr/Covenant "Covenant") - a collaborative .NET C2 framework for red teamers
* [GhostPack](https://github.com/GhostPack "GhostPack") - a collection of security tools from Specter Ops
* [SharpView](https://github.com/tevora-threat/SharpView "SharpView") - C# implementation of harmj0y's PowerView
* [SharpSploit](https://github.com/cobbr/SharpSploit "SharpSploit") - a .NET post-exploitation library and spiritual successor to PowerSploit
* [SharpHound](https://github.com/BloodHoundAD/SharpHound3 "SharpHound") - C# Data Collector for the BloodHound Project, Version 3
* [SharpClipHistory](https://github.com/FSecureLABS/SharpClipHistory "SharpClipHistory") - a .NET application written in C# that can be used to read the contents of a user's clipboard history in Windows 10
* [SharpGPOAbuse](https://github.com/FSecureLABS/SharpGPOAbuse "SharpGPOAbuse") - a .NET application written in C# that can be used to take advantage of a user's edit rights on a Group Policy Object (GPO) in order to compromise the objects that are controlled by that GPO.
* [SharpGPO-RemoteAccessPolicies](https://github.com/FSecureLABS/SharpGPO-RemoteAccessPolicies "SharpGPO-RemoteAccessPolicies") - a C# tool for enumerating remote access policies through group policy
* [SharpRDPCheck](https://github.com/3gstudent/SharpRDPCheck "SharpRDPCheck") and [SharpRDPUploader](https://github.com/RDPUploader/RDPUploader "SharpRDPUploader") - tools for exploiting RDP
* [autorunner](https://github.com/woanware/autorunner "autorunner") - emulates the Sysinternals Autoruns tool, using C# / .NET
* [taskscheduler](https://github.com/dahall/taskscheduler "taskscheduler") - provides a .NET wrapper for the Windows Task Scheduler
* .NET Binary Obfuscator / Packer
  * [ConfuserEx](https://github.com/mkaring/ConfuserEx "ConfuserEx")
  * [neo-ConfuserEx](https://github.com/XenocodeRCE/neo-ConfuserEx "neo-ConfuserEx")
