@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET ToggleMode=%~3
set configuration=%4
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

if "%configuration%"=="" (
set configuration=Debug
)

if "%CCC7DB%" == "" (
SET CCC7DB=Infratest_CCC7
)

if "%AnalyticsDB%" == "" (
SET AnalyticsDB=Infratest_Analytics
)

if "%ToggleMode%" == "" (
CHOICE /C drc /M "Do you want to run (d)ev, (r)c or (c)ustomer toggle settings"
IF ERRORLEVEL 1 SET ToggleMode=ALL
IF ERRORLEVEL 2 SET ToggleMode=RC
IF ERRORLEVEL 3 SET ToggleMode=R
)

SET SourceSettings=%ROOTDIR%\.debug-setup\config\settingsInfraTest.txt
SET AppliedSettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

COPY "%SourceSettings%" "%AppliedSettings%" >> NUL

::Replace some config values
cscript %ROOTDIR%\.debug-setup\common\replace.vbs "TeleoptiAnalytics_Demo" "%AnalyticsDB%" "%AppliedSettings%" > NUL
cscript %ROOTDIR%\.debug-setup\common\replace.vbs "TeleoptiApp_Demo" "%CCC7DB%" "%AppliedSettings%" > NUL
cscript %ROOTDIR%\.debug-setup\common\replace.vbs "TOGGLE_MODE_VALUE" "%ToggleMode%" "%AppliedSettings%" > NUL

:: Build Teleopti.Support.Tool.exe if not built and source files are available 
IF NOT EXIST "%ROOTDIR%\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" (
	IF EXIST "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" (
		%MSBUILD% /property:Configuration=%Configuration% /t:rebuild "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"
	)
)

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTEST

ECHO Done!

ENDLOCAL
goto:eof

