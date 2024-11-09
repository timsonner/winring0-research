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

Download link before they took it down...  
https://storage.cdn.evga.com/software/EVGA_Precision_X1_1.0.4.0.exe  

Check status of driver  
```powershell  
Get-WmiObject -Class Win32_SystemDriver | Where-Object { $_.Name -like "*WinRing0*" }
```  

Stop driver  
```powershell  
Stop-Service -Name WinRing0_1_2_0
```  

Further reading  
https://www.cyberark.com/resources/threat-research-blog/inglourious-drivers-a-journey-of-finding-vulnerabilities-in-drivers  



