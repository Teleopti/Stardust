::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  ECHO Pending reboot flag exists.. exiting >> "%DIRECTORY%\StartupLog.txt" 2>&1
  GOTO Finish
)
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . .\InitializeCopyPayroll.ps1  >nul

exit /b 0

:Finish
exit /b 0
