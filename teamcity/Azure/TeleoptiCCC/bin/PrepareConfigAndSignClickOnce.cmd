::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
ECHO powershell . .\PrepareConfigAndSignClickOnce.ps1 -ScriptPath "%DIRECTORY%\PrepareConfigAndSignClickOnceSub.ps1" > PrepareConfigAndSignClickOnce.log
powershell . .\PrepareConfigAndSignClickOnce.ps1 -ScriptPath "%DIRECTORY%\PrepareConfigAndSignClickOnceSub.ps1" >> PrepareConfigAndSignClickOnce.log
exit %ERRORLEVEL%