::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
ECHO powershell . .\SignClickOnce.ps1 -ScriptPath "%DIRECTORY%\SignClickOnceSub.ps1" > SignClickOnce.log
powershell . .\SignClickOnce.ps1 -ScriptPath "%DIRECTORY%\SignClickOnceSub.ps1" >> SignClickOnce.log
exit %ERRORLEVEL%