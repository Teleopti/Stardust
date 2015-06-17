::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted

::Try copy Log4Net .dll
set srcFile=E:\approot\Services\ETL\Tool\log4net.dll
set targetFile=%DIRECTORY%\log4Net\log4net.dll
COPY "%srcFile%" "%targetFile%" /Y

::execute
if exist "%targetFile%" (
powershell . .\Add-TeleoptiFoldersAndShortcuts.ps1 >nul
)
exit %ERRORLEVEL%
