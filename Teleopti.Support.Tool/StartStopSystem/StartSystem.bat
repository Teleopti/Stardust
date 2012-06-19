@echo off
ECHO Starting all local Teleopti CCC 7 services and MsDtc
ECHO Start mode will be set to "Automatic"
PING -n 4 127.0.0.1>nul

::Set the list of services to manuipulate
::note: Order matters!!
SET serviceList=MsDtc;TeleoptiServiceBus;TeleoptiEtlService

::Finally start our two App pools
if exist "%windir%\system32\inetsrv\AppCmd.exe" (
echo trying to start iis 7.0 or 7.5 App Pools ...
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v3.5"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v3.5" /autoStart:true
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0 Web"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 Web" /autoStart:true
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0 Broker"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 Broker" /autoStart:true
) else (
echo trying to start iis 5.1 or 6.0 App Pools ...
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v3.5"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 Web"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 Broker"
)

echo Start App Pools. Done
echo.

::For each service in list
goto :processServiceList

:done
echo done!
PING -n 2 127.0.0.1>nul
exit /b 0

:processServiceList
for /f "tokens=1* delims=;" %%a in ("%serviceList%") do (
call :SetSvcModeAndAction "%%a" Start Auto
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
::Finaly start service
SC query %1 | FIND "RUNNING" > NUL
IF %errorlevel% NEQ 0 (
ECHO Will %2: %1
NET %2 %1
)