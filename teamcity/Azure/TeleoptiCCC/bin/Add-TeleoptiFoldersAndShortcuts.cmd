::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted

::execute
powershell . .\PrepareConfigAndSignClickOnce.ps1 -ScriptPath "%DIRECTORY%\Add-TeleoptiFoldersAndShortcuts.ps1" >nul
exit %ERRORLEVEL%
