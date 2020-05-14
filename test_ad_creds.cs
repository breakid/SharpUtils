// Source
//     - https://stackoverflow.com/questions/8949501/why-does-active-directory-validate-last-password
//     - LDAP Result Codes - https://ldapwiki.com/wiki/LDAP%20Result%20Codes

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.DirectoryServices.Protocols\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.DirectoryServices.Protocols.dll" /t:exe /out:.\test\TestAdCreds.exe .\TestAdCreds.cs

using System;
using System.Net;
using System.DirectoryServices.Protocols;

class TestADCreds {
    public static void PrintUsage() {
        Console.WriteLine(@"Given an Active Directory domain, username, and password, returns whether the credentials are valid. Does not work on local accounts.

USAGE:
    test_ad_creds.exe <domain> <username> <password>");
        Console.WriteLine("\nDONE");
    }
    
    private const int ERROR_LOGON_FAILURE = 0x31;
    private const int LDAP_SERVER_UNAVAILABLE = 0x51;

    public static void Main(string[] args) {
        if (args.Length != 3) {
            PrintUsage();
            return;
        }
        
        string domain = args[0];
        string username = args[1];
        string password = args[2];
        
        NetworkCredential credentials = new NetworkCredential(username, password, domain);

        LdapDirectoryIdentifier id = new LdapDirectoryIdentifier(domain);

        using (LdapConnection connection = new LdapConnection(id, credentials, AuthType.Kerberos)) {
            connection.SessionOptions.Sealing = true;
            connection.SessionOptions.Signing = true;

            try {
                connection.Bind();
            } catch (LdapException lEx) {
                if (lEx.ErrorCode == ERROR_LOGON_FAILURE) {
                    Console.WriteLine("Invalid");
                    Console.WriteLine("\nDONE");
                    return;
                } else if (lEx.ErrorCode == LDAP_SERVER_UNAVAILABLE) {
                    Console.WriteLine("[-] ERROR: LDAP server is unavailable");
                    Console.WriteLine("\nDONE");
                    return;
                }
                
                Console.WriteLine("[-] LDAP Error: " + lEx.ErrorCode.ToString("X"));
                Console.WriteLine("\nDONE");
                return;
            }
        }
        
        Console.WriteLine("Valid");
        Console.WriteLine("\nDONE");
        return;
    }
}