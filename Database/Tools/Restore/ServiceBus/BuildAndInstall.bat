::------------------
:: note: ROOTDIR and TEAMFOUNDATION
:: comes from calling batch file
::------------------

IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..
set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set OutputDir=%ROOTDIR%\output\servicebus

::Stop  ServiceBus Service
IF EXIST "%OutputDir%\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" (
NET STOP TeleoptiServiceBus

::Un-install  ServiceBus Service
ECHO Un-Install ServiceBus Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%OutputDir%\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" /LogFile="" > "%OutputDir%\BusUninstall.log"

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)

::Add payroll dummy .dll if not exist, else Post-build event will fail
IF EXIST "%ROOTDIR%\..\..\..\Teleopti.Ccc.Payroll\Teleopti.Ccc.Payroll\bin\Debug\Teleopti.Ccc.Payroll.dll" (
ECHO Payroll .dll exist) ELSE (ECHO dummy > "%ROOTDIR%\..\..\..\Teleopti.Ccc.Payroll\Teleopti.Ccc.Payroll\bin\Debug\Teleopti.Ccc.Payroll.dll")

ECHO %msbuild% "%ROOTDIR%\..\..\..\ServiceBus.sln" /t:Clean;Rebuild "Debug"
%msbuild% "%ROOTDIR%\..\..\..\\ServiceBus.sln" /t:Clean;Rebuild > "%OutputDir%\BusBuild.log"

::Deploy
XCOPY "%ROOTDIR%\..\..\..\Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.ServiceBus.Host\bin\Debug\*" "%OutputDir%" /E /Y /i > NUL

::Install
ECHO Install ServiceBus Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "%OutputDir%\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" /LogFile="" > "%OutputDir%\BusInstall.log"