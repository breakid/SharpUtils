# Pipe all stdout to $null to give nice clean output that still shows error messages

Write-Output "[*] Building SharpUtils..."
mkdir "bin" -ErrorAction SilentlyContinue

Write-Output "  [*] arp.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\arp_2.0.exe arp.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\arp_3.5.exe arp.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\arp_4.0.exe arp.cs | Select-String error

Write-Output "  [*] auditpol.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\auditpol_2.0.exe auditpol.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\auditpol_3.5.exe auditpol.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\auditpol_4.0.exe auditpol.cs | Select-String error

Write-Output "  [*] check_sig.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\check_sig_4.0.exe check_sig.cs | Select-String error

Write-Output "  [*] create_process.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\create_process_3.5.exe create_process.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\create_process_4.0.exe create_process.cs | Select-String error

Write-Output "  [*] driver_list.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\driver_list_4.0.exe driver_list.cs | Select-String error

Write-Output "  [*] dsquery.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\dsquery_4.0.exe dsquery.cs | Select-String error

Write-Output "  [*] dump_dns.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\dump_dns_4.0.exe dump_dns.cs | Select-String error

Write-Output "  [*] env.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\env_2.0.exe env.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\env_3.5.exe env.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\env_4.0.exe env.cs | Select-String error

Write-Output "  [*] eventlog.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\eventlog_3.5.exe eventlog.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\eventlog_4.0.exe eventlog.cs | Select-String error

Write-Output "  [*] farm_dns.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\farm_dns_3.5.exe farm_dns.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\farm_dns_4.0.exe farm_dns.cs | Select-String error

Write-Output "  [*] freespace.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\freespace_2.0.exe freespace.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\freespace_3.5.exe freespace.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\freespace_4.0.exe freespace.cs | Select-String error

Write-Output "  [*] get_chrome_tab_info.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll" /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationClient\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationClient.dll" /t:exe /out:bin\get_chrome_tab_info_4.0.exe get_chrome_tab_info.cs  | Select-String error

Write-Output "  [*] get_hotfix.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\get_hotfix_3.5.exe get_hotfix.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\get_hotfix_4.0.exe get_hotfix.cs | Select-String error

Write-Output "  [*] icacls.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\icacls_2.0.exe icacls.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\icacls_3.5.exe icacls.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\icacls_4.0.exe icacls.cs | Select-String error

Write-Output "  [*] lld.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\lld_3.5.exe lld.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\lld_4.0.exe lld.cs | Select-String error

Write-Output "  [*] netstat.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\netstat_3.5.exe netstat.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\netstat_4.0.exe netstat.cs | Select-String error

Write-Output "  [*] pagegrab.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\pagegrab_2.0.exe pagegrab.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\pagegrab_3.5.exe pagegrab.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\pagegrab_4.0.exe pagegrab.cs | Select-String error

Write-Output "  [*] readfile.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\readfile_2.0.exe readfile.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\readfile_3.5.exe readfile.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\readfile_4.0.exe readfile.cs | Select-String error

Write-Output "  [*] taskkill.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\taskkill_2.0.exe taskkill.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\taskkill_3.5.exe taskkill.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\taskkill_4.0.exe taskkill.cs | Select-String error

Write-Output "  [*] tasklist_svc.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\tasklist_svc_3.5.exe tasklist_svc.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\tasklist_svc_4.0.exe tasklist_svc.cs | Select-String error

Write-Output "  [*] tasklist_wmi.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\tasklist_wmi_3.5.exe tasklist_wmi.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\tasklist_wmi_4.0.exe tasklist_wmi.cs | Select-String error

Write-Output "  [*] test_ad_creds.exe"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.DirectoryServices.Protocols\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.DirectoryServices.Protocols.dll" /t:exe /out:bin\test_ad_creds_4.0.exe test_ad_creds.cs  | Select-String error

Write-Output "  [*] window_list.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\window_list_2.0.exe window_list.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\window_list_3.5.exe window_list.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\window_list_4.0.exe window_list.cs | Select-String error

Write-Output "  [*] wmi_query.exe"
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:bin\wmi_query_3.5.exe wmi_query.cs | Select-String error
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\wmi_query_4.0.exe wmi_query.cs | Select-String error

Write-Output "[+] Build Complete!"

Write-Host -NoNewLine "Press any key to continue..."
$host.ui.RawUI.ReadKey() | Out-Null