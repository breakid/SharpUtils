// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:check_sig.exe check_sig.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

internal class CheckSign
{
    public static void PrintUsage()
    {
        Console.WriteLine(@"Check if EXE/DLL is signed. Returns Chain if available.
        
USAGE:
    check_sig <full_path_to_exe/dll_on_host> [/?]");
    }
    private static void Main(string[] args)
    {
        try
        {
            // Check for Help
            if (args.Length > 0 && args[0] == "/?")
            {
                //Print Usage
                PrintUsage();
                return;
            }

            // Check for Supplied File Argument
            if (args == null || args.Length == 0 )
            {
                Console.WriteLine("[!] [CheckSign] No File Supplied");
                return;
            }

            // Check if File was supplied
            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[-] [CheckSign] File not found");
                return;
            }

            // Check for Signature
            X509Certificate2 theCertificate;
            try
            {
                X509Certificate theSigner = X509Certificate.CreateFromSignedFile(filePath);
                theCertificate = new X509Certificate2(theSigner);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] [CheckSign] No digital signature found!");
                Console.WriteLine("    [*] Message: " + ex.Message);
                return;
            }
            
            // Signature was Found, Gathering Chain information (if available).
            // This will not reach out to the internet to verify validity, instead will check the local system.
            bool chainIsValid = false;
            var theCertificateChain = new X509Chain();
            theCertificateChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            theCertificateChain.ChainPolicy.RevocationMode = X509RevocationMode.Offline;
            theCertificateChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
            theCertificateChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            chainIsValid = theCertificateChain.Build(theCertificate);

            if (chainIsValid)
            {
                Console.WriteLine("[+] [CheckSign] Digital Signature Found!");
                Console.WriteLine("    [*] Publisher Information : " + theCertificate.SubjectName.Name);
                Console.WriteLine("    [*] Valid From: " + theCertificate.GetEffectiveDateString());
                Console.WriteLine("    [*] Valid To: " + theCertificate.GetExpirationDateString());
                Console.WriteLine("    [*] Issued By: " + theCertificate.Issuer);
                return;
            }
            else
            {
                Console.WriteLine("[+] [CheckSign] Digital Signature Found!");
                Console.WriteLine("    [-] Chain Not Valid or Unable to Verify Chain.");
                Console.WriteLine("    [-] Certificate is/may be self-signed.");
                return;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("[-] [CheckSign] ERROR: {0}", e.Message.Trim());
        }
        finally
        {
            Console.WriteLine("\nDONE");
        }
    }
}