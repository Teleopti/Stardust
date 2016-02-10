@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET CCC7DB=%1
SET AnalyticsDB=%2
SET Configuration=%3
SET MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

SET validationKey=754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1
SET decryptionKey=3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429
IF "%Configuration%"==""  set Configuration=Debug

cd %ROOTDIR%

if "%1" == "" (
SET /P CCC7DB=CCC7DB?
)

if "%2" == "" (
SET /P AnalyticsDB=AnalyticsDB?
)

SET SourceSettings=%ROOTDIR%\config\settings.txt
SET AppliedSettings=%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\settings.txt

COPY "%SourceSettings%" "%AppliedSettings%"

::Replace some parameters according to current RestoreToLocal.bat
cscript .\common\replace.vbs "TeleoptiAnalytics_Demo" "%AnalyticsDB%" "%AppliedSettings%" > NUL
cscript .\common\replace.vbs "TeleoptiApp_Demo" "%CCC7DB%" "%AppliedSettings%" > NUL

IF NOT EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" (
	::Build Teleopti.Support.Tool.exe
	ECHO Building %ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj
	IF EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /property:Configuration=%Configuration% /t:rebuild "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"
)

::add fixed encryption/decryption keys to match the ones used in WebTest
ECHO %validationKey%>"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\validation.key"
ECHO %decryptionKey%>"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\decryption.key"

::Run supportTool to replace all config
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -MODebug

ENDLOCAL
goto:eof