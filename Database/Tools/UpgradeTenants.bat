::UpgradeTenants
@ECHO off
@ECHO off

SET SUSER=%~1
SET SPASS=%~2
SET URL=%~3

IF  "%SUSER%"=="" (
SET /P SUSER=SQL Admin user:
)

IF "%SPASS%"=="" (
SET /P SPASS=SQL Admin password:
)

IF "%URL%"=="" (
SET /P URL=Url to admin site:
)

set WORKING_DIRECTORY=%~dp0
SET Kommandompigg=%WORKING_DIRECTORY%UpgradeTenants.ps1

powershell.exe -ExecutionPolicy Bypass -File "%Kommandompigg%" %SUSER% %SPASS% "%URL%
