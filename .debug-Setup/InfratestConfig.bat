@ECHO off
SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET DriveLetter=%ROOTDIR:~0,2%
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET ToggleMode=%~3
SET configuration=%~4
SET sqlAuthString=%~5

CALL "%~dp0CheckMsbuildPath.bat"
IF %ERRORLEVEL% NEQ 0 GOTO :error

IF "%configuration%"=="" (
	SET configuration=Debug
)

IF "%CCC7DB%" == "" (
	SET CCC7DB=Infratest_CCC7
)

IF "%AnalyticsDB%" == "" (
	SET AnalyticsDB=Infratest_Analytics
)
 
IF "%ToggleMode%" == "" (
	CHOICE /C drc /M "Do you want to run (d)ev, (r)c or (c)ustomer toggle settings"
	IF ERRORLEVEL 1 SET ToggleMode=ALL
	IF ERRORLEVEL 2 SET ToggleMode=RC
	IF ERRORLEVEL 3 SET ToggleMode=R
)

set splitsub=;
for /f "tokens=1* delims=%splitsub%" %%A in ("%sqlAuthString%") do set part1=%%A & set part2=%%B
set splitsub==
for /f "tokens=1* delims=%splitsub%" %%A in ("%part1%") do set part1=%%A & set SRV=%%B
IF NOT "%SRV%" == "" (
	SQLCMD -S%SRV% -E -d"%CCC7DB%" -i"%ROOTDIR%\.debug-setup\database\tsql\AddUserForTest.sql"
)

IF EXIST "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" (
	IF NOT EXIST "%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" (
		%MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%
	)
)
%DriveLetter%
CD "%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\"
Teleopti.Support.Tool.exe -LOAD "%ROOTDIR%\.debug-setup\config\settings.txt"
Teleopti.Support.Tool.exe -SET $(DB_CCC7) "%CCC7DB%"
Teleopti.Support.Tool.exe -SET $(DB_ANALYTICS) "%AnalyticsDB%"
Teleopti.Support.Tool.exe -SET $(AS_DATABASE) "%AnalyticsDB%"
Teleopti.Support.Tool.exe -SET $(ToggleMode) "%ToggleMode%"
IF NOT "%sqlAuthString%" == "" CALL Teleopti.Support.Tool.exe -SET $(SQL_AUTH_STRING) "%sqlAuthString%"
Teleopti.Support.Tool.exe -MOTEST

ECHO Done!

ENDLOCAL
