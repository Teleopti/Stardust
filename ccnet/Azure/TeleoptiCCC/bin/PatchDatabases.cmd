::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
ECHO powershell . .\PatchDatabases.ps1 -ScriptPath "%DIRECTORY%\PatchDatabasesSub.ps1" > PatchDatabases.log
powershell . .\PatchDatabases.ps1 -ScriptPath "%DIRECTORY%\PatchDatabasesSub.ps1" >> PatchDatabases.log
exit %ERRORLEVEL%