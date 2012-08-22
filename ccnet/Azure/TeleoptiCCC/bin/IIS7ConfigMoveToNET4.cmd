@ECHO OFF
ECHO setting security for PowerShell on Azure instance >> IISConfigMoveToNET4.log
powershell set-executionpolicy unrestricted  >> IISConfigMoveToNET4.log

ECHO Setting .NET 4.0 for Web >> IISConfigMoveToNET4.log
ECHO PowerShell .\ChangeAppPoolVersion.ps1 "Web" >> IISConfigMoveToNET4.log
PowerShell .\ChangeAppPoolVersion.ps1 "Web" >> IISConfigMoveToNET4.log
ECHO Web Done >> IISConfigMoveToNET4.log
ECHO. >> IISConfigMoveToNET4.log

ECHO Setting .NET 4.0 for Broker >> IISConfigMoveToNET4.log
ECHO PowerShell .\ChangeAppPoolVersion.ps1 "Broker"  >> IISConfigMoveToNET4.log
PowerShell .\ChangeAppPoolVersion.ps1 "Broker" >> IISConfigMoveToNET4.log
ECHO Broker Done >> IISConfigMoveToNET4.log
ECHO. >> IISConfigMoveToNET4.log