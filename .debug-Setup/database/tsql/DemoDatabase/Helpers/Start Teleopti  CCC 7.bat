@echo off
ECHO Start all services needed by Teleopti WFM, inluding default instance of OLAP and SQL Server ...
PING -n 2 127.0.0.1>nul
::The list of services to manuipulate
::set serviceList=mssqlserver;MSSQLServerOLAPService;TeleoptiBrokerService;teleoptiRtaService;teleoptiEtlService
set serviceList=mssqlserver;MSSQLServerOLAPService;TeleoptiBrokerService;teleoptiEtlService

::For each service in list
goto :processToken

:done
echo done!
PING -n 2 127.0.0.1>nul
exit

:processToken
for /f "tokens=1* delims=;" %%a in ("%serviceList%") do (
call :SetSvcModeAndAction "%%a" Start Auto
set serviceList=%%b
)
if not "%serviceList%" == "" goto :processToken
if "%serviceList%" == "" goto :done

:SetSvcModeAndAction
::Check if Service exist
SC query %1 | FIND "1060" > NUL
IF %errorlevel% NEQ 0 (
SC config %1 start= %3
)
SC query %1 | FIND "RUNNING" > NUL
IF %errorlevel% NEQ 0 (
ECHO Will %2: %1
NET %2 %1
)