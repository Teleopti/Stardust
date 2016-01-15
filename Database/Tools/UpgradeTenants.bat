::UpgradeTenants
@ECHO off
@ECHO off

SET INTEGRATED=%~1
SET SERVER=%~2
SET DB=%~3 
SET SUSER=%~4
SET SPASS=%~5


set WORKING_DIRECTORY=%~dp0
SET Kommandompigg=%WORKING_DIRECTORY%UpgradeTenants.ps1
powershell.exe -ExecutionPolicy Bypass -File "%Kommandompigg%" "%INTEGRATED%" "%SERVER%" "%DB%" "%SUSER%" "%SPASS%" >> "%WORKING_DIRECTORY%\..\UpgradeTenants.log"
::PAUSE
