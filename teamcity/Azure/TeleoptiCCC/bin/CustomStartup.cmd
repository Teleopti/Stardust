::NOTE!!!
::This script dependence on one action in PrepareConfigAdnSignClickOnce.ps1
::In that script we try pull in a script called: CustomStartup.ps1
::In this scripts we executes it.
::Depending on the order of the StartUp task

::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
if exist "%DIRECTORY%\CustomStartup.ps1"
powershell "%DIRECTORY%\CustomStartup.ps1"
exit %ERRORLEVEL%