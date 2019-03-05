SET PROCESSNAME=RestartService
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%

::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . %DIRECTORY%\%PROCESSNAME%.ps1 >> "%DIRECTORY%\StartupLog.txt" 2>&1

exit /b 0

