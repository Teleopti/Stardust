@ECHO off

tasklist /FI "IMAGENAME eq Teleopti.Analytics.Etl.ServiceConsoleHost.exe" |find ":" > nul
if "%ERRORLEVEL%"=="1" (
	taskkill /F /IM Teleopti.Analytics.Etl.ServiceConsoleHost.exe
)