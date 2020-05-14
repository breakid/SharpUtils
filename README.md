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
    arp.exe



### auditpol.exe
Displays the computer's auditing policy (i.e., what actions will be logged)

**Requires administrative privileges**

#### Usage
    auditpol.exe



### create_process.exe
Uses WMI to execute a command on the specified remote host

**Uses WMI**
**Requires administrative privileges on the remote host**

#### Usage
    create_process.exe <host> <full_path_to_exe_on_remote_host> <executable_arguments>



### env.exe
Displays environment variables (in alphabetical order)

#### Usage
    env.exe



### eventlog.exe
Reads events from the specified log file; optionally limit by EventID and number of events returned

#### Usage
    eventlog.exe <log name> [/C <count>] [/E <event IDs (comma-separated, no space)>]



### freespace.exe
Lists logical drives, including total and available free space. Mapped drives will only be shown when freespace.exe is run within the same session or with the same credentials used to map the drive.

#### Usage
    freespace.exe



### get_chrome_tab_info.exe
Displays the URL of the current tab and the titles of all tabs in foremost Chrome window. Will NOT display information about other Chrome windows.

#### Usage
    get_chrome_tab_info.exe



### get_chrome_tab_info.exe
Lists patches, optionally including verbose information such as Description, InstalledBy, and InstalledOn.

#### Usage
    get_hotfix.exe [/S system] [/U [domain\]username /P password] [/V]



### icacls.exe
Displays permissions, grouped by user/group, for each file or directory specified

#### Usage
    icacls.exe <file_or_directory> <...>



### lld.exe
Lists logical drives (using WMI), including total and available free space. Mapped drives will only be shown when lld.exe is run within the same session or with the same credentials used to map the drive. This differs from freespace.exe because it can be used against a remote system.

**Uses WMI**
**Requires administrative privileges when used against a remote host**

#### Usage
    lld.exe [system]



### pagegrab.exe
Makes a web request; can be used to check external connectivity or get the content of an internal web page without going through the trouble of setting up a SOCKS proxy.

Can optionally specify the HTTP method, POST request data, and/or a proxy server address/port. The `-v` flag causes the HTML contents of the page to be printed; if omitted, only the response headers will be shown.

#### Usage
    pagegrab.exe [-p http(s)://<proxy>:<proxy_port>] [-m <method>] [-d <URL encoded POST data>] [-v] <URL>
      -v  Print the HTML contents of the response



### readfile.exe
Read the contents of a file; optionally limit to X number of lines from the beginning of the file or Y number of lines from the end

#### Usage
    readfile.exe [+X] [-Y] <path_to_file>



### taskkill.exe
Kills a single process by PID, or one or more processes by imagename. Optionally, target a remote host and/or provide plaintext username and password from the command-line.

**Uses WMI**
**Requires administrative privileges if used on a remote host and for some local tasks (i.e., killing processes with a different owner)**

#### Usage
    taskkill.exe [/S <system> [/U [domain\]<username> /P <password>]] { [/PID <processid> | /IM <imagename>] }



### test_ad_creds.exe
Authenticates against the specified Active Directory (AD) domain using the provided username and password; indicates whether the credentials are valid or not. Does not work with local credentials, only AD creds.

**This WILL create a failed logon event if the credentials are not valid; use sparingly to avoid account lockout**

#### Usage
    test_ad_creds.exe <domain> <username> <password>



### window_list.exe
Displays a list of visible windows and their associated process.

**BETA: Has only been tested locally under Medium and High Integrity accounts. SYSTEM-level access *may* provide information from multiple sessions, and remote access *may* provide results (initial testing resulted in "Network path not found" errors)**

#### Usage
    window_list.exe [/S <host>]

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
* [autorunner](https://github.com/woanware/autorunner "autorunner") - emulates the Sysinternals Autoruns tool, using C# / .NET
* [taskscheduler](https://github.com/dahall/taskscheduler "taskscheduler") - provides a .NET wrapper for the Windows Task Scheduler
* .NET Binary Obfuscator / Packer
  * [ConfuserEx](https://github.com/mkaring/ConfuserEx "ConfuserEx")
  * [neo-ConfuserEx](https://github.com/XenocodeRCE/neo-ConfuserEx "neo-ConfuserEx")
