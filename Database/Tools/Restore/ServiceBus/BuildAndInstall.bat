::------------------
:: note: ROOTDIR and TEAMFOUNDATION
:: comes from calling batch file
::------------------

::Stop  ServiceBus Service
IF EXIST "%ROOTDIR%\ServiceBus\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" (
NET STOP TeleoptiServiceBus

::Un-install  ServiceBus Service
ECHO Un-Install ServiceBus Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%ROOTDIR%\ServiceBus\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" > BusUninstall.log

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)

::Add payroll dummy .dll if not exist, else Post-build event will fail
IF EXIST "%ROOTDIR%\..\..\..\Teleopti.Ccc.Payroll\Teleopti.Ccc.Payroll\bin\Debug\Teleopti.Ccc.Payroll.dll" (
ECHO Payroll .dll exist) ELSE (ECHO dummy > "%ROOTDIR%\..\..\..\Teleopti.Ccc.Payroll\Teleopti.Ccc.Payroll\bin\Debug\Teleopti.Ccc.Payroll.dll")

::Clean + Build
ECHO %DEVENV% "%ROOTDIR%\ServiceBus\ServiceBus.sln" /Clean > NUL
%DEVENV% "%ROOTDIR%\ServiceBus\ServiceBus.sln" /Clean > NUL

ECHO %DEVENV% "%ROOTDIR%\ServiceBus\ServiceBus.sln" /Rebuild "Debug"
%DEVENV% "%ROOTDIR%\ServiceBus\ServiceBus.sln" /Rebuild "Debug" > BusBuild.log

::Deploy
XCOPY "%ROOTDIR%\..\..\..\Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.ServiceBus.Host\bin\Debug\*" "%ROOTDIR%\ServiceBus\" /E /Y > NUL

::Install
ECHO Install ServiceBus Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "%ROOTDIR%\ServiceBus\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" > BusInstall.log