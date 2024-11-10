# Bring Your Own Vulnerable Driver Research  

CVE-2021-41285  
https://voidsec.com/crucial-mod-utility-lpe-cve-2021-41285/  

CVE-2020-14979  
https://medium.com/@matterpreter/cve-2020-14979-local-privilege-escalation-in-evga-precisionx1-cf63c6b95896  

POCs:  
https://github.com/CrackerCat/WinRing0-poc  
https://gist.github.com/NSG650/d718fcf5aa78a407783e3404d7b54128  
https://gist.github.com/NSG650/aac65399bbe74519636efc0a651c0425  
https://github.com/VoidSec/Exploit-Development/blob/master/windows/x64/kernel/crucial_Ballistix_MOD_Utility_v.2.0.2.5/crucial_Ballistix_MOD_Utility_v.2.0.2.5_memory_dump_PoC.cpp  

Download link before EVGA took it down...  
https://storage.cdn.evga.com/software/EVGA_Precision_X1_1.0.4.0.exe  

File.io download link (expires Nov 9, 2025)  
https://file.io/SSidBPffxU82  

Check status of driver  
```powershell  
Get-WmiObject -Class Win32_SystemDriver | Where-Object { $_.Name -like "*WinRing0*" }
```  

Check status of driver  
```powershell  
Get-Service -Name WinRing0_1_2_0
```  

Install driver (cmd.exe)  
```  
sc.exe create WinRing0_1_2_0 type= kernel start= demand binPath= "C:\<path to>\WinRing0x64.sys"
```  

Compile install-driver.exe (microsoft developer console for visual studio)  
```  
csc install-driver.cs
```  

Check registry key for driver  
```  
reg query "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WinRing0_1_2_0"
```  

Start driver  
```powershell  
Start-Service -Name WinRing0_1_2_0
```  

Stop driver  
```powershell  
Stop-Service -Name WinRing0_1_2_0
```  

Remove driver (cmd.exe)  
```  
sc.exe delete WinRing0_1_2_0
```  

Remove Driver (powerShell)  
```powershell  
$service = Get-WmiObject -Class Win32_SystemDriver -Filter "Name = 'WinRing0_1_2_0'"
if ($service) {
    $service.Delete()
    Write-Output "Service 'WinRing0_1_2_0' deleted successfully."
} else {
    Write-Output "Service 'WinRing0_1_2_0' not found."
}
```  

Further reading  
https://www.cyberark.com/resources/threat-research-blog/inglourious-drivers-a-journey-of-finding-vulnerabilities-in-drivers  



