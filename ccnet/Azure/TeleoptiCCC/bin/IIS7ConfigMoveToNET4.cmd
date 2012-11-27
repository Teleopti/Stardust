@ECHO OFF

ECHO setting security for PowerShell on Azure instance >> IISConfigMoveToNET4.log
powershell set-executionpolicy unrestricted  >> IISConfigMoveToNET4.log
Call :ChangeAppPoolVersion Web v4.0
Call :ChangeAppPoolVersion Broker v4.0
Call :ChangeAppPoolVersion SDK v4.0
Call :ChangeAppPoolVersion Analytics v4.0
Call :ChangeAppPoolVersion RTA v4.0
Call :ChangeAppPoolVersion MyTime v4.0
Call :ChangeAppPoolVersion Client v4.0
goto :eof

:ChangeAppPoolVersion
ECHO Setting ManagedRuntimeVersion: %2 for %1 >> IISConfigMoveToNET4.log
ECHO PowerShell .\ChangeAppPoolVersion.ps1 "%1" "%2" >> IISConfigMoveToNET4.log
PowerShell .\ChangeAppPoolVersion.ps1 "%1" "%2" >> IISConfigMoveToNET4.log
ECHO %1 Done >> IISConfigMoveToNET4.log
ECHO. >> IISConfigMoveToNET4.log
goto :eof
