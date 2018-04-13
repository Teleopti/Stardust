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

"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -LOAD "%ROOTDIR%\config\settings.txt"
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -SET $(machineKey.validationKey) "754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1"
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -SET $(machineKey.decryptionKey) "3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429"
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -SET $(DB_CCC7) "%CCC7DB%"
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -SET $(DB_ANALYTICS) "%AnalyticsDB%"
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -SET $(AS_DATABASE) "%AnalyticsDB%"
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -MODebug

ECHO Done!

ENDLOCAL
goto:eof