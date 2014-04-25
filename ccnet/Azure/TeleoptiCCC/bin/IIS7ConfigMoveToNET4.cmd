@ECHO OFF
SET FILE=%~n0

ECHO setting security for PowerShell on Azure instance >> %FILE%.log
powershell set-executionpolicy unrestricted  >> %FILE%.log
Call :ChangeAppPoolVersion Web v4.0
Call :ChangeAppPoolVersion Broker v4.0
Call :ChangeAppPoolVersion SDK v4.0
Call :ChangeAppPoolVersion Analytics v4.0
Call :ChangeAppPoolVersion RTA v4.0
Call :ChangeAppPoolVersion MyTime v4.0
Call :ChangeAppPoolVersion Client v4.0
Call :ChangeAppPoolVersion Broker.backplane v4.0
goto :eof

:ChangeAppPoolVersion
ECHO Setting ManagedRuntimeVersion: %2 for %1 >> %FILE%.log
ECHO PowerShell .\ChangeAppPoolVersion.ps1 "%1" "%2" >> %FILE%.log
PowerShell .\ChangeAppPoolVersion.ps1 "%1" "%2" >> %FILE%.log
ECHO %1 Done >> %FILE%.log
ECHO. >> %FILE%.log
goto :eof
