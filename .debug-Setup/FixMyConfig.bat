@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET CCC7DB=%1
SET AnalyticsDB=%2
SET Configuration=%3

CALL "%~dp0CheckMsbuildPath.bat"
IF %ERRORLEVEL% NEQ 0 GOTO :error

IF "%Configuration%"=="" SET Configuration=Debug

CD %ROOTDIR%

IF "%1" == "" (
	SET /P CCC7DB=CCC7DB?
)

IF "%2" == "" (
	SET /P AnalyticsDB=AnalyticsDB?
)

SET probablyStardustPorts=%WORKING_DIRECTORY%SetUrlAcl.ps1
PowerShell.exe -NoProfile -Command "& {Start-Process PowerShell.exe -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File ""%probablyStardustPorts%""' -Verb RunAs}"

IF NOT EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" (
	IF EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" (
		%MSBUILD% /property:Configuration=%Configuration% /t:rebuild "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"
	)
)
 
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -FixMyConfig "%CCC7DB%" "%AnalyticsDB%"

ECHO Done!

ENDLOCAL
goto:eof