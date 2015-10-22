::UpgradeTenants
@ECHO off
@ECHO off

SET INTEGRATED=%~1
SET SUSER=%~2
SET SPASS=%~3
SET URL=%~4
SET WinAuth=0

IF  "%INTEGRATED%"=="" (
	GOTO :WinAuth
)

:Auth
IF  "%INTEGRATED%"=="1" (
	SET WinAuth=1
	GOTO :Url
)
:User
IF  "%SUSER%"=="" (
SET /P SUSER=SQL Admin user:
)

IF "%SPASS%"=="" (
SET /P SPASS=SQL Admin password:
)
:Url
IF "%URL%"=="" (
SET /P URL=Url to admin site:
) 
::ECHO "%Kommandompigg%"
::ECHO "%WinAuth%"
::ECHO "%SUSER%"
::ECHO "%SPASS%"
::ECHO "%URL%"
::PAUSE

set WORKING_DIRECTORY=%~dp0
SET Kommandompigg=%WORKING_DIRECTORY%UpgradeTenants.ps1
powershell.exe -ExecutionPolicy Bypass -File "%Kommandompigg%" "%WinAuth%" "%SUSER%" "%SPASS%" "%URL%"
::PAUSE
GOTO :end

:WinAuth
SET INTEGRATED=0
CHOICE /C yn /M "Do you want to use WinAuth?"
IF %ERRORLEVEL% EQU 1 (
SET INTEGRATED=1
)
GOTO :Auth

:end