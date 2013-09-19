@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET masterSettings=%ROOTDIR%\..\settings.txt
SET CCC7DB=%1
SET AnalyticsDB=%2
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET MySettings=%ROOTDIR%\..\..\Teleopti.Support.Code\bin\debug\settings.txt
 
cd %ROOTDIR%

taskkill /IM WebDev.WebServer40.EXE /F
taskkill /IM WebDev.WebServer20.EXE /F

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"

::Build Teleopti.Support.Tool.exe
ECHO Building %ROOTDIR%\..\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj
%MSBUILD% "%ROOTDIR%\..\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"

::Replace some parameters according to current RestoreToLocal.bat
cscript replace.vbs "TeleoptiAnalytics_Demo" "%AnalyticsDB%" "%MySettings%" > NUL
cscript replace.vbs "TeleoptiCCC7_Demo" "%CCC7DB%" "%MySettings%" > NUL

::Run supportTool to replace all config
"%ROOTDIR%\..\..\Teleopti.Support.Tool\bin\Debug\Teleopti.Support.Tool.exe" -MODebug

ENDLOCAL
goto:eof