@ECHO off

tasklist /FI "IMAGENAME eq Teleopti.Analytics.Etl.ServiceConsoleHost.exe" 2>NUL | find /I /N "Teleopti.Analytics.Etl.ServiceConsoleHost.exe">NUL
if "%ERRORLEVEL%"=="0" (
	taskkill /IM Teleopti.Analytics.Etl.ServiceConsoleHost.exe
)