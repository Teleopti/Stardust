@echo off
ECHO Starting all local Teleopti WFM services and MsDtc
PING -n 1 127.0.0.1>nul

IF "%ROOTDIR%"=="" SET ROOTDIR=%~dp0

::WinXp and 2003 can't handle start mode: "delayed-auto"
SET action=delayed-auto
cscript "%ROOTDIR%OsMajorVersionGet.vbs" //NoLogo
SET /a OsMajorVersion = %ERRORLEVEL%
IF %OsMajorVersion% LEQ 5 SET action=auto
ECHO OsMajorVersion is: %OsMajorVersion%
ECHO Start mode will be set to "%action%"
ECHO.

::Set the list of services to manuipulate
::note: Order matters!!
SET serviceList=MsDtc;TeleoptiServiceBus;TeleoptiEtlService

::Finally start our two App pools
if exist "%windir%\system32\inetsrv\AppCmd.exe" (
echo trying to start iis 7.0 or 7.5 App Pools ...
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0" /autoStart:true
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0 Web"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 Web" /autoStart:true
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0 Broker"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 Broker" /autoStart:true
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0 SDK"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 SDK" /autoStart:true
"%windir%\system32\inetsrv\AppCmd.exe" Start Apppool "Teleopti ASP.NET v4.0 RTA"
"%windir%\system32\inetsrv\AppCmd.exe" Set Apppool "Teleopti ASP.NET v4.0 RTA" /autoStart:true
) else (
echo trying to start iis 5.1 or 6.0 App Pools ...
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 Web"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 Broker"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 SDK"
CSCRIPT "%ROOTDIR%..\WiseIISConfig\adsutil.vbs" START_SERVER "W3SVC/AppPools/Teleopti ASP.NET v4.0 RTA"
)

echo Start App Pools. Done
echo.

if exist "%ROOTDIR%\..\..\TeleoptiCCC\SDK\TeleoptiCccSdkService.svc" call:browseSDK
Echo.

::For each service in list
goto :processServiceList

:done
echo done!
PING -n 2 127.0.0.1>nul
exit /b 0

:processServiceList
for /f "tokens=1* delims=;" %%a in ("%serviceList%") do (
call :SetSvcModeAndAction "%%a" Start %action%
set serviceList=%%b
)
if not "%serviceList%" == "" goto :processServiceList
goto :done

:browseSDK
::32-bit
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Teleopti\TeleoptiCCC\InstallationSettings /v AGENT_SERVICE > nul
if %errorlevel% EQU 0 set confirmedPath=HKEY_LOCAL_MACHINE\SOFTWARE\Teleopti\TeleoptiCCC\InstallationSettings
::64-bit
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings /v AGENT_SERVICE > nul
if %errorlevel% EQU 0 set confirmedPath=HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings

::get install path
set SDKUrl=
for /f "tokens=2*" %%A in ('REG QUERY "%confirmedPath%" /v AGENT_SERVICE') DO (
  for %%F in (%%B) do (
    set SDKUrl=%%F
  )
)

Echo Browsing SDK url
cscript "%ROOTDIR%BrowseUrl.vbs" "%SDKUrl%"
if %errorlevel% neq 200 Echo WARNING: could not send GET request to SDK!
exit /b 0


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

