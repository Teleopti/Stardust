ECHO Starting Startup task > Install.log
ECHO. >> Install.log

SET ServiceStartError=0

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
ECHO Rootdir is: "%ROOTDIR%" >> Install.log

::================
::Install Services
::================
ECHO --------------------- >> Install.log
ECHO Start Service install >> Install.log
SC QUERY AnalyticsEtlService
IF NOT ERRORLEVEL 1060 (
	SC DELETE AnalyticsEtlService
	SC QUERY AnalyticsEtlService
	IF NOT ERRORLEVEL 1060 CALL :ServiceError
)
"%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "..\Services\ETL\Service\Teleopti.Analytics.Etl.ServiceHost.exe" >> Install.log

SC QUERY TeleoptiServiceBus
IF NOT ERRORLEVEL 1060 (
	SC DELETE TeleoptiServiceBus
	SC QUERY TeleoptiServiceBus
	IF NOT ERRORLEVEL 1060 CALL :ServiceError
)
"%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "..\Services\ServiceBus\Teleopti.CCC.Sdk.ServiceBus.Host.exe" >> Install.log

ECHO. >> Install.log
ECHO Done install! >> Install.log

::================
::Install Report Viewer
::================
ECHO --------------------- >> Install.log
ECHO installing Report Viewer  >> Install.log
CALL "ReportViewer2010.exe" /norestart /log "%ROOTDIR%\ReportViewer2010-installlog.htm" /install /q
ECHO installing Report Viewer. Done  >> Install.log


::================
::Install Powershell ISE
::================
ECHO --------------------- >> Install.log
ECHO Set unrestricted Powershell scripting
powershell set-executionpolicy unrestricted >> Install.log
ECHO Set unrestricted Powershell scripting. Done >> Install.log

ECHO Install Powershel ISE >> Install.log
PowerShell .\InstallPowershell-ISE.ps1 >> Install.log
ECHO Install Powershel ISE. Done >> Install.log


::================
::Start Services
::================
ECHO --------------------- >> Install.log
ping 127.0.0.1 -n 5 > nul

ECHO. >> Install.log
ECHO Starting services... >> Install.log

NET START AnalyticsEtlService
IF %ERRORLEVEL% NEQ 0 ECHO Error: ETL services could not be started!! >> Install.log

NET START TeleoptiServiceBus
IF %ERRORLEVEL% NEQ 0 ECHO Error: Service bus could not be started!! >> Install.log

ECHO.
ECHO Done!
ECHO --------------------- >> Install.log

GOTO :eof

:ServiceStartError
ECHO The service %1 could not be started! >> Install.log
ECHO Please check the Windows eventlog for errors. >> Install.log
SET ServiceStartError=1
exit /b

:ServiceError
ECHO Failed to install one or more services! >> Install.log
exit /b

GOTO :eof
