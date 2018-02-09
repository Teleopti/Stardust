::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%

IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  del %DIRECTORY%\PendingReboot.txt /f
  Shutdown /r /t 0
)

::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . .\RestartService.ps1  >nul
exit /b 0

