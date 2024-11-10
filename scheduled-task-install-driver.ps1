# Define the action (path to your executable or script)
$action = New-ScheduledTaskAction -Execute "C:\<path to>\install-driver.exe"

# Define the trigger (the task will run once, immediately)
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddSeconds(10)

# Define the principal (run as SYSTEM with highest privileges)
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

# Define the task settings (run with highest privileges and settings)
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

# Register the task
Register-ScheduledTask -Action $action -Trigger $trigger -Principal $principal -Settings $settings -TaskName "InstallWinRing0Driver"
