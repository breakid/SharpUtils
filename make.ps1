Write-Output "[*] Building SharpUtils..."
mkdir "bin" -ErrorAction SilentlyContinue | Out-Null

Write-Output "  [*] arp.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\arp.exe arp.cs | Out-Null

Write-Output "  [*] auditpol.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\auditpol.exe auditpol.cs | Out-Null

Write-Output "  [*] env.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\env.exe env.cs | Out-Null

Write-Output "  [*] freespace.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\freespace.exe freespace.cs | Out-Null

Write-Output "  [*] pagegrab.exe"
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:bin\pagegrab.exe pagegrab.cs | Out-Null

Write-Output "[+] Build Complete!"