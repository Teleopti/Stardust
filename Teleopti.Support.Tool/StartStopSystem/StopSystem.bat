@echo off
::ToDo:
::Find a better way to handle our AppPool names and folder permissions
ECHO Stopping all local Teleopti WFM services
ECHO Start mode will be set to "Manual"
PING -n 4 127.0.0.1>nul

IF "%ROOTDIR%"=="" SET ROOTDIR=%~dp0

::Set the list of services to manuipulate
::note: Order matters!!
set serviceList=TeleoptiEtlService;TeleoptiServiceBus

::For each service in list, stop
call :processServiceList

::Finally stop our two App pools
if exist "%windir%\system32\inetsrv\AppCmd.exe" (
echo trying to stop iis 7.0 or 7.5 App Pools ...
"%windir%\system32\inetsrv\AppCmd.exe" Stop Apppool "Teleopti ASP.NET v4.0"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0" /autoStart:false
"%windir%\system32\inetsrv\AppCmd.exe" Stop Apppool "Teleopti ASP.NET v4.0 Web"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 Web" /autoStart:false
"%windir%\system32\inetsrv\AppCmd.exe" Stop Apppool "Teleopti ASP.NET v4.0 Broker"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 Broker" /autoStart:false
"%windir%\system32\inetsrv\AppCmd.exe" Stop Apppool "Teleopti ASP.NET v4.0 SDK"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 SDK" /autoStart:false
"%windir%\system32\inetsrv\AppCmd.exe" Stop Apppool "Teleopti ASP.NET v4.0 RTA"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 RTA" /autoStart:false
) else (
echo trying to stop iis 5.1 or 6.0 App Pools ...
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" STOP_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" STOP_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 Web"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" STOP_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 Broker"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" STOP_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 SDK"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" STOP_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 RTA"
)

echo stopping App Pools. Done
echo.
goto :done

:done
echo done!
PING -n 5 127.0.0.1>nul
exit /b 0

:processServiceList
for /f "tokens=1* delims=;" %%a in ("%serviceList%") do (
call :SetSvcModeAndAction "%%a" Stop Demand
set serviceList=%%b
)
if not "%serviceList%" == "" goto :processServiceList

goto :done

:SetSvcModeAndAction
::Check if Service is disabled, then exit
SC qc %1 | FIND "DISABLED" > NUL
IF %errorlevel% EQU 0 (
ECHO Service %1 is diabled, I won't touch it
ECHO.
exit /b 0
)

::Check if Service exist, if so set startup type
SC query %1 | FIND "1060" > NUL
IF %errorlevel% NEQ 0 (
SC config %1 start= %3
)
::finaly, stop
SC query %1 | FIND "RUNNING" > NUL
IF %errorlevel% EQU 0 (
ECHO Will %2: %1
NET %2 %1
)
exit /b 0