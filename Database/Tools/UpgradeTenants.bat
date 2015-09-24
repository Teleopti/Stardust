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

SET Kommandompigg=%CD%\UpgradeTenants.ps1 %SUSER% %SPASS% "%URL%"

powershell.exe -ExecutionPolicy Bypass -Command %Kommandompigg%
