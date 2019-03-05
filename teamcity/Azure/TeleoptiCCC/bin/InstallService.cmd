@ECHO OFF
ECHO Starting Startup task > Install.log
ECHO. >> Install.log

SET ETLService=TeleoptiEtlService
SET ServiceBus=TeleoptiServiceBus

SET DRIVELETTER=%CD:~0,3%
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DRIVE=%ROOTDIR:~0,2%
%DRIVE%
CD %ROOTDIR%

ECHO Rootdir is: "%ROOTDIR%" >> Install.log

::================
::Install Services
::================
ECHO --------------------- >> Install.log
ECHO Service status before install [%ETLService%] >> Install.log
SC QUERY %ETLService%  >> Install.log
SC QUERY %ETLService%
IF NOT ERRORLEVEL 1060 (
	NET STOP %ETLService%
	SC DELETE %ETLService%
	SC QUERY %ETLService%
	IF NOT ERRORLEVEL 1060 CALL :ServiceError

)
sc create %ETLService% binPath= "%DRIVELETTER%approot\Services\ETL\Service\Teleopti.Analytics.Etl.ServiceHost.exe" DisplayName= "%ETLService%" >> Install.log
sc config %ETLService% start=Auto

ECHO Service status after install [%ETLService%] >> Install.log
SC QUERY %ETLService%  >> Install.log
ECHO --------------------- >> Install.log
ECHO Service status before install [%ServiceBus%] >> Install.log
SC QUERY %ServiceBus%  >> Install.log
SC QUERY %ServiceBus%
IF NOT ERRORLEVEL 1060 (
	NET STOP %ServiceBus%
	SC DELETE %ServiceBus%
	SC QUERY %ServiceBus%
	IF NOT ERRORLEVEL 1060 CALL :ServiceError
)
sc create %ServiceBus% binPath= "%DRIVELETTER%approot\Services\ServiceBus\Teleopti.CCC.Sdk.ServiceBus.Host.exe" DisplayName= "%ServiceBus%" >> Install.log
sc config %ServiceBus% start=Auto
ECHO Service status after install [%ServiceBus%] >> Install.log
SC QUERY %ServiceBus%  >> Install.log

sc failure "%ServiceBus%" reset= 0 actions= restart/60000/restart/60000/restart/60000

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
ECHO Set unrestricted Powershell scripting  >> Install.log
powershell set-executionpolicy unrestricted >> Install.log
ECHO Set unrestricted Powershell scripting. Done >> Install.log

ECHO Install Powershel ISE >> Install.log
PowerShell .\InstallPowershell-ISE.ps1 >> Install.log
ECHO Install Powershel ISE. Done >> Install.log

::================
::Register all service and application names
::NOTE: Must corrrepsond to the name used in Log4Net.config => BuildArtifacts
::================
ECHO --------------------- >> Install.log
ECHO Register all service and application names in Event Log  >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiAnalyticsWebPortal" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiETLService" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiETLTool" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiRtaWebService" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiSdkWebService" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiServiceBus" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiWebApps" >> Install.log
CALL "RegisterEventLogSource.exe" "TeleoptiWebBroker" >> Install.log
ECHO Register all service and application names in Event Log. Done >> Install.log

ECHO.
ECHO Done! >> Install.log
ECHO --------------------- >> Install.log

GOTO :eof

:ServiceError
ECHO Failed to install one or more services! >> Install.log
exit /b