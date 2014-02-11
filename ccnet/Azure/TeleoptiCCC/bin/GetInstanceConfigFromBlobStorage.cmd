::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell ". %DIRECTORY%\GetInstanceConfigFromBlobStorage.ps1; main -directory \"%DIRECTORY%\""
exit %ERRORLEVEL%