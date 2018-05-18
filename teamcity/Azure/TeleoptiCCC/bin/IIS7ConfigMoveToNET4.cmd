@ECHO OFF
SET PROCESSNAME=IIS7ConfigMoveToNET4
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
SET FILE=%~n0

IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  ECHO Pending reboot flag exists.. exiting %PROCESSNAME%>> "%DIRECTORY%\StartupLog.txt" 2>&1
  GOTO Finish
)

ECHO setting security for PowerShell on Azure instance >> %FILE%.log
powershell set-executionpolicy unrestricted  >> %FILE%.log
Call :ChangeAppPoolVersion Web v4.0
Call :ChangeAppPoolVersion SDK v4.0
Call :ChangeAppPoolVersion Analytics v4.0
Call :ChangeAppPoolVersion RTA v4.0
Call :ChangeAppPoolVersion Client v4.0
Call :ChangeAppPoolVersion AuthenticationBridge v4.0
Call :ChangeAppPoolVersion WindowsIdentityProvider v4.0
Call :ChangeAppPoolVersion Administration v4.0
Call :ChangeAppPoolVersion API v4.0
ECHO ..\Tools\SupportTools\FixServerConfiguration.bat >> %FILE%.log
..\Tools\SupportTools\FixServerConfiguration.bat
goto :eof
exit /b 0

:ChangeAppPoolVersion
ECHO Setting ManagedRuntimeVersion: %2 for %1 >> %FILE%.log
ECHO PowerShell .\ChangeAppPoolVersion.ps1 "%1" "%2" >> %FILE%.log
PowerShell .\ChangeAppPoolVersion.ps1 "%1" "%2" >> %FILE%.log
ECHO %1 Done >> %FILE%.log
ECHO. >> %FILE%.log
goto :eof

exit /b 0

:Finish
exit /b 0
