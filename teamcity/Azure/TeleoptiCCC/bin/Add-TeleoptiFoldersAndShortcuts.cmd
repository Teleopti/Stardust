SET PROCESSNAME=Add-TeleoptiFoldersAndShortcuts
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::Identify current drive letter
SET DRIVELETTER=%CD:~0,3%

IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  ECHO Pending reboot flag exists.. exiting %PROCESSNAME%>> "%DIRECTORY%\StartupLog.txt" 2>&1
  GOTO Finish
)

::allow un-signed
powershell set-executionpolicy unrestricted

::Try copy Log4Net .dll
set srcFile=%DRIVELETTER%approot\Services\ETL\Tool\log4net.dll
set targetFile=%DIRECTORY%\log4Net\log4net.dll
COPY "%srcFile%" "%targetFile%" /Y

::execute
if exist "%targetFile%" (
powershell . .\Add-TeleoptiFoldersAndShortcuts.ps1 >nul
)

exit /b 0

:Finish
exit /b 0
