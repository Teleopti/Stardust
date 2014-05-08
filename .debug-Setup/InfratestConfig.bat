@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET masterSettings=%ROOTDIR%\config\settingsInfraTest.txt
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET FEATURETOGGLE=%~3
SET HGGUID=%~4
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET MySettings=%ROOTDIR%\..\Teleopti.Support.Code\settings.txt
SET nhibFolder=c:\nhib\%HGGUID%
 
cd %ROOTDIR%

if "%1" == "" (
SET CCC7DB=Infratest_CCC7
)

if "%2" == "" (
SET AnalyticsDB=Infratest_Analytics
)

::init toggle values to "ALL"
SET TOGGLEURL=ALL
SET TOGGLE_FILE=ALL

if "%FEATURETOGGLE%" == "RC" (
SET TOGGLE_FILE=RC.toggles.txt
SET TOGGLE_URL=http://localhost:52858/
)

if "%FEATURETOGGLE%" == "R" (
SET TOGGLE_FILE=released.toggles.txt
SET TOGGLE_URL=http://localhost:52858/
)

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"
ECHO. >> "%MySettings%"
ECHO $(SDK_nhibConfPath)^|%nhibFolder%>>"%MySettings%"
ECHO $(ETL_SERVICE_nhibConfPath)^|%nhibFolder%>>"%MySettings%"
ECHO $(ETL_TOOL_nhibConfPath)^|%nhibFolder%>>"%MySettings%"
ECHO $(AGENTPORTALWEB_nhibConfPath)^|bin>>"%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(TOGGLE_FILE)^|%TOGGLE_FILE%>>"%MySettings%"
ECHO $(TOGGLE_URL)^|%TOGGLE_URL%>>"%MySettings%"
ECHO $(DATASOURCE_NAME)^|TestData>>"%MySettings%"

if not exist "%nhibFolder%" mkdir "%nhibFolder%"

::Ugly Betty Was here!
::Copy dynamic parameters into file structure in order to run parallel builds 
copy "%ROOTDIR%\..\Teleopti.Support.Code\ConfigFiles\BuildServerConfigFiles.txt.template" "%ROOTDIR%\..\Teleopti.Support.Code\ConfigFiles\BuildServerConfigFiles.txt"
ECHO. >> "%ROOTDIR%\..\Teleopti.Support.Code\ConfigFiles\BuildServerConfigFiles.txt"
ECHO %nhibFolder%\TestData.nhib.xml,BuildArtifacts\TeleoptiCCC7.nhib.xml >> "%ROOTDIR%\..\Teleopti.Support.Code\ConfigFiles\BuildServerConfigFiles.txt"

::Build Teleopti.Support.Tool.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj
%MSBUILD% /t:rebuild "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log" 

::Run supportTool to replace all config
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\Debug\Teleopti.Support.Tool.exe" -MOTEST

ENDLOCAL
goto:eof