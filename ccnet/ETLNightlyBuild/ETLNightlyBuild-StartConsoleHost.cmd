@ECHO off
SET CCNetWorkDir=%~1
SET Config=%~2
tasklist /FI "IMAGENAME eq Teleopti.Analytics.Etl.ServiceConsoleHost.exe" |find ":" > nul
if "%ERRORLEVEL%"=="1" (
	taskkill /F /IM Teleopti.Analytics.Etl.ServiceConsoleHost.exe
)
start %CCNetWorkDir%\Teleopti.Analytics.Etl.ServiceConsoleHost\bin\%Config%\Teleopti.Analytics.Etl.ServiceConsoleHost.exe