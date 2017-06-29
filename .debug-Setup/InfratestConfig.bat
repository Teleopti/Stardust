@ECHO off
SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET ToggleMode=%~3
set configuration=%~4
set sqlAuthString=%~5

call "%~dp0CheckMsbuildPath.bat"
IF %ERRORLEVEL% NEQ 0 GOTO :error

if "%configuration%"=="" (
set configuration=Debug
)

if "%CCC7DB%" == "" (
SET CCC7DB=Infratest_CCC7
)

if "%AnalyticsDB%" == "" (
SET AnalyticsDB=Infratest_Analytics
)

if "%sqlAuthString%" == "" (
SET sqlAuthString=Data Source=.;Integrated Security=SSPI
)

if "%ToggleMode%" == "" (
CHOICE /C drc /M "Do you want to run (d)ev, (r)c or (c)ustomer toggle settings"
IF ERRORLEVEL 1 SET ToggleMode=ALL
IF ERRORLEVEL 2 SET ToggleMode=RC
IF ERRORLEVEL 3 SET ToggleMode=R
)

SET SourceSettings=%ROOTDIR%\.debug-setup\config\settings.txt
SET AppliedSettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

::Build Teleopti.Support.Tool.exe if source files are available (they aren't in pipeline)
if exist "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%

::get a fresh Settings.txt
COPY "%SourceSettings%" "%AppliedSettings%"

set splitsub=;
for /f "tokens=1* delims=%splitsub%" %%A in ("%sqlAuthString%") do set part1=%%A & set part2=%%B
set splitsub==
for /f "tokens=1* delims=%splitsub%" %%A in ("%part1%") do set part1=%%A & set SRV=%%B
::echo SRV  = %SRV%
SQLCMD -S%SRV% -E -d"%CCC7DB%" -i"%ROOTDIR%\.debug-setup\database\tsql\AddUserForTest.sql"

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -SET $(DB_CCC7) "%CCC7DB%"
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -SET $(DB_ANALYTICS) "%AnalyticsDB%"
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -SET $(AS_DATABASE) "%AnalyticsDB%"
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -SET $(ToggleMode) "%ToggleMode%"
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -SET $(SQL_AUTH_STRING) "%sqlAuthString%"
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTEST

ENDLOCAL
goto:eof

