# Pipe all stdout to $null to give nice clean output that still shows error messages

Write-Output "[*] Building SharpUtils..."
mkdir "bin" -ErrorAction SilentlyContinue

Write-Output "  [*] arp.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\arp.exe arp.cs > $null

Write-Output "  [*] auditpol.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\auditpol.exe auditpol.cs > $null

Write-Output "  [*] create_process.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\create_process.exe create_process.cs > $null

Write-Output "  [*] driver_list.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\driver_list.exe driver_list.cs > $null

Write-Output "  [*] dsquery.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\dsquery.exe dsquery.cs > $null

Write-Output "  [*] dump_dns.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\dump_dns.exe dump_dns.cs > $null

Write-Output "  [*] env.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\env.exe env.cs > $null

Write-Output "  [*] eventlog.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\eventlog.exe eventlog.cs > $null

Write-Output "  [*] farm_dns.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\farm_dns.exe farm_dns.cs > $null

Write-Output "  [*] freespace.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\freespace.exe freespace.cs > $null

Write-Output "  [*] get_chrome_tab_info.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll" /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationClient\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationClient.dll" /t:exe /out:bin\get_chrome_tab_info.exe get_chrome_tab_info.cs  > $null

Write-Output "  [*] get_hotfix.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\get_hotfix.exe get_hotfix.cs > $null

Write-Output "  [*] icacls.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\icacls.exe icacls.cs > $null

Write-Output "  [*] lld.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\lld.exe lld.cs > $null

Write-Output "  [*] netstat.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\netstat.exe netstat.cs > $null

Write-Output "  [*] pagegrab.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\pagegrab.exe pagegrab.cs > $null

Write-Output "  [*] readfile.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\readfile.exe readfile.cs > $null

Write-Output "  [*] taskkill.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\taskkill.exe taskkill.cs > $null

Write-Output "  [*] tasklist_svc.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\tasklist_svc.exe tasklist_svc.cs > $null

Write-Output "  [*] tasklist_wmi.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\tasklist_wmi.exe tasklist_wmi.cs > $null

Write-Output "  [*] test_ad_creds.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.DirectoryServices.Protocols\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.DirectoryServices.Protocols.dll" /t:exe /out:bin\test_ad_creds.exe test_ad_creds.cs  > $null

Write-Output "  [*] window_list.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\window_list.exe window_list.cs > $null

Write-Output "  [*] wmi_query.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\wmi_query.exe wmi_query.cs > $null

Write-Output "[+] Build Complete!"

Write-Host -NoNewLine "Press any key to continue..."
$host.ui.RawUI.ReadKey() | Out-Null