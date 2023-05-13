// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:check_sig.exe check_sig.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal class CheckSign
{
    public static void PrintUsage()
    {
        Console.WriteLine(@"Checks whether an EXE/DLL is signed and, if so, validates the signature.

USAGE:
    check_sig [/online] <path_to_exe/dll> [...] [/?]");
    }

    private static void Main(string[] args)
    {
        try
        {
            List<string> filepaths = new List<string>();
            bool onlineCheck = false;

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "-ONLINE":
                    case "/ONLINE":
                        // Peform an online revocation list check
                        onlineCheck = true;
                        break;
                    case "/?":
                        // Print usage and exit
                        PrintUsage();
                        return;
                    default:
                        filepaths.Add(arg);
                        break;
                }
            }

            // Check whether filepath(s) specified
            if (filepaths.Count == 0)
            {
                Console.Error.WriteLine("[!] [CheckSign] ERROR: No file(s) specified");
                return;
            }

            // List the number of filepaths parsed
            Console.WriteLine("[*] [CheckSign] Checking {0} file{1}{2}", filepaths.Count, (filepaths.Count > 1) ? "s" : "", Environment.NewLine);

            // Process each file
            int fileNum = 0;

            foreach (string filepath in filepaths)
            {
                Console.WriteLine("[*] [CheckSign] File {0}: {1}", ++fileNum, filepath);

                if (!File.Exists(filepath))
                {
                    Console.WriteLine("    [-] [CheckSign] ERROR: File not found ({0})", filepath);
                    continue;
                }

                // Check for Signature
                X509Certificate2 cert;
                try
                {
                    X509Certificate theSigner = X509Certificate.CreateFromSignedFile(filepath);
                    cert = new X509Certificate2(theSigner);
                }
                catch (CryptographicException)
                {
                    Console.WriteLine("    [-] [CheckSign] No digital signature found!\n");
                    continue;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("    [-] [CheckSign] ERROR: {0}\n", e.Message.Trim());
                    continue;
                }

                Console.WriteLine("    [*] Digital signature found");

                // Signature was Found, Gathering Chain information (if available).
                // This will not reach out to the internet to verify validity, instead will check the local system.
                bool chainIsValid = false;
                var certChain = new X509Chain();
                certChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                certChain.ChainPolicy.RevocationMode = (onlineCheck) ? X509RevocationMode.Online : X509RevocationMode.Offline;
                certChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                certChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                chainIsValid = certChain.Build(cert);

                Console.WriteLine("        Publisher  : " + cert.SubjectName.Name);
                Console.WriteLine("        Valid From : " + cert.GetEffectiveDateString());
                Console.WriteLine("        Valid To   : " + cert.GetExpirationDateString());
                Console.WriteLine("        Issued By  : " + cert.Issuer);

                if (chainIsValid)
                {
                    Console.WriteLine("    [+] Signature valid!");
                }
                else
                {
                    DateTime expirationDate = DateTime.Parse(cert.GetExpirationDateString());

                    if (DateTime.Today > expirationDate)
                    {
                        Console.WriteLine("    [-] Signature expired");
                    }
                    else
                    {
                        Console.WriteLine("    [-] Chain invalid or unable to verify chain; certificate may be self-signed");
                    }
                }

                Console.WriteLine();
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