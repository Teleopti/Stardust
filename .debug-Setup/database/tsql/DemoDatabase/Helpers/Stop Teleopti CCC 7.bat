@echo off
:Input
::Play Hardball?
set /p yn=Would you like to shut down SQL Server, AS Services and IIS as well [y/n]?
if "%yn%"=="" (echo No input given) & (GOTO :Input) 
if /I "%yn%"=="n" (SET hardBall=N) & (GOTO :clients)
if /I "%yn%"=="y" (SET hardBall=Y) else (echo Entry not y or n!) & (GOTO :Input)

:clients
tasklist /M Teleopti.CCC.smartClientportal* | FIND "INFO: No tasks" > NUL
if %errorlevel% NEQ 0 taskkill /IM Teleopti.Ccc.SmartClientPortal.Shell.exe /F

tasklist /M Teleopti.CCC.AgentPortal* | FIND "INFO: No tasks" > NUL
if %errorlevel% NEQ 0 taskkill /IM Teleopti.Ccc.AgentPortal.exe /F

::The list of services to manuipulate
::set serviceList=teleoptiRtaService;teleoptiEtlService;TeleoptiBrokerService
set serviceList=teleoptiEtlService;TeleoptiServiceBus;TeleoptiBrokerService
if "%hardBall%"=="Y" SET serviceList=%serviceList%;MSSQLServerOLAPService;sqlserveragent;mssqlserver

::For each service in list
goto :processServiceList

:hardBall
::IIS?
if "%hardBall%"=="Y" (
tasklist /M w3wp* | FIND "w3wp.exe" > NUL
if %errorlevel% EQU 0 taskkill /IM w3wp.exe /F
)
goto :done

:done
echo done!
PING -n 2 127.0.0.1>nul
exit

:processServiceList
for /f "tokens=1* delims=;" %%a in ("%serviceList%") do (
call :SetSvcModeAndAction "%%a" Stop Demand
set serviceList=%%b
)
if not "%serviceList%" == "" goto :processServiceList
if "%serviceList%" == "" goto :hardBall

:SetSvcModeAndAction
::Check if Service exist
SC query %1 | FIND "1060" > NUL
IF %errorlevel% NEQ 0 (
SC config %1 start= %3
)
SC query %1 | FIND "RUNNING" > NUL
IF %errorlevel% EQU 0 (
ECHO Will %2: %1
NET %2 %1
)