::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . .\RestartService.ps1  >nul
exit /b 0