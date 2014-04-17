@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET masterSettings=%ROOTDIR%\config\settings.txt
SET CCC7DB=%1
SET AnalyticsDB=%2
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET MySettings=%ROOTDIR%\..\Teleopti.Support.Code\settings.txt
 
cd %ROOTDIR%

if "%1" == "" (
SET /P CCC7DB=CCC7DB?
)

if "%2" == "" (
SET /P AnalyticsDB=AnalyticsDB?
)

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"

::Replace some parameters according to current RestoreToLocal.bat
cscript .\common\replace.vbs "TeleoptiAnalytics_Demo" "%AnalyticsDB%" "%MySettings%" > NUL
cscript .\common\replace.vbs "TeleoptiCCC7_Demo" "%CCC7DB%" "%MySettings%" > NUL

::Build Teleopti.Support.Tool.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj
%MSBUILD% /t:rebuild "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"

::Run supportTool to replace all config
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\Debug\Teleopti.Support.Tool.exe" -MODebug

ENDLOCAL
goto:eof