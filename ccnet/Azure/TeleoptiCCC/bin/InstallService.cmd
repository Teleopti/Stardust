ECHO Starting Startup task > Install.log
ECHO. >> Install.log

SET ServiceStartError=0

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
ECHO Rootdir is: "%ROOTDIR%" >> Install.log

::================
::Install Services
::================
ECHO Start Service install >> Install.log
SC QUERY TeleoptiETLService
IF NOT ERRORLEVEL 1060 CALL :ServiceError
"%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "..\Services\ETL\Service\Teleopti.Analytics.Etl.ServiceHost.exe" >> Install.log

SC QUERY TeleoptiBrokerService
IF NOT ERRORLEVEL 1060 CALL :ServiceError
"%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "..\Services\MessageBroker\Teleopti.Messaging.Svc.exe" >> Install.log

ECHO. >> Install.log
ECHO Done install! >> Install.log

::================
::Open Firewall for TeleoptiBrokerService
::================
::ECHO. >> Install.log
ECHO Open up local firewall rule for MsgBroker broker. >> Install.log
netsh advfirewall firewall add rule name = TeleoptiBrokerService dir = in protocol = tcp action = allow localport = 10000 profile=PUBLIC

::================
::Install Report Viewer
::================
ECHO installing Report Viewer  >> Install.log
CALL "ReportViewer2010.exe" /norestart /log "%ROOTDIR%\ReportViewer2010-installlog.htm" /install /q
ECHO installing Report Viewer. Done  >> Install.log

::================
::Start Services
::================
ping 127.0.0.1 -n 5 > nul

ECHO. >> Install.log
ECHO Starting services... >> Install.log

NET START TeleoptiBrokerService
IF %ERRORLEVEL% NEQ 0 CALL :ServiceStartError TeleoptiBrokerService

NET START AnalyticsEtlService
IF %ERRORLEVEL% NEQ 0 CALL :ServiceStartError AnalyticsEtlService

IF %ServiceStartError% EQU 0
ECHO Services started successfully >> Install.log

IF %ServiceStartError% NEQ 0
ECHO Error: Some services could not be started!! >> Install.log

ECHO.
ECHO Done!

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
