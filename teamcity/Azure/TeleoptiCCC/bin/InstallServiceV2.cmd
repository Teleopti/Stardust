SET PROCESSNAME=InstallServiceV2
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  ECHO Pending reboot flag exists.. exiting %PROCESSNAME%>> "%DIRECTORY%\StartupLog.txt" 2>&1
  GOTO Finish
)
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . .\InstallService.ps1  >nul
exit /b 0

:Finish
exit /b 0