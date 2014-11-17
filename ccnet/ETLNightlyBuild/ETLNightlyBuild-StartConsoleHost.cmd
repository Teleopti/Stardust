@ECHO off
SET CCNetWorkDir=%~1
SET Config=%~2
tasklist /FI "IMAGENAME eq Teleopti.Analytics.Etl.ServiceConsoleHost.exe" 2>NUL | find /I /N "Teleopti.Analytics.Etl.ServiceConsoleHost.exe">NUL
if "%ERRORLEVEL%"=="0" (
	taskkill /IM Teleopti.Analytics.Etl.ServiceConsoleHost.exe
)
start "%CCNetWorkDir%\Teleopti.Analytics.Etl.ServiceConsoleHost\bin\%Config%\Teleopti.Analytics.Etl.ServiceConsoleHost.exe"