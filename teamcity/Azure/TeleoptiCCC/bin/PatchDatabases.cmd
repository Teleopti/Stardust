::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . .\PatchDatabases.ps1  >nul
exit /b 0
::exit %ERRORLEVEL%