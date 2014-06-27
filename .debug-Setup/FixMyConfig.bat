@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET masterSettings=%ROOTDIR%\config\settings.txt
SET CCC7DB=%1
SET AnalyticsDB=%2
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET MySettings=%ROOTDIR%\..\Teleopti.Support.Code\settings.txt

SET validationKey=754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1
SET decryptionKey=3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429
IF "%configuration%"==""  set configuration=Debug

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
cscript .\common\replace.vbs "TeleoptiApp_Demo" "%CCC7DB%" "%MySettings%" > NUL

::Build Teleopti.Support.Tool.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj
IF EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:rebuild "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"
COPY "%MySettings%" "%ROOTDIR%\..\Teleopti.Support.Tool\bin\%configuration%\settings.txt"

::add fixed encryption/decryption keys to match the ones used in WebTest
ECHO %validationKey%>"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%configuration%\validation.key"
ECHO %decryptionKey%>"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%configuration%\decryption.key"

::Run supportTool to replace all config
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MODebug

ENDLOCAL
goto:eof