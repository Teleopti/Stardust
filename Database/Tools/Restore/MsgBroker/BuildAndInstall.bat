::------------------
:: note: ROOTDIR
:: comes from calling batch file
::------------------
::extra pill om man vuill köra _bara_ denna batshshsh
IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..

::Stop  Message Broker service
IF EXIST "%ROOTDIR%\MsgBroker\Teleopti.Messaging.Svc.exe" (
NET STOP TeleoptiBrokerService

::Un-install Message Broker service
ECHO Un-Install MsgBroker Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%ROOTDIR%\MsgBroker\Teleopti.Messaging.Svc.exe" > BrokerUninstall.log

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)

::Clean
ECHO %DEVENV% "%ROOTDIR%\..\..\..\Teleopti.Messaging\Teleopti.Messaging.sln" /Clean > NUL
%DEVENV% "%ROOTDIR%\..\..\..\Teleopti.Messaging\Teleopti.Messaging.sln" /Clean > NUL

::Build
ECHO %DEVENV% "%ROOTDIR%\..\..\..\Teleopti.Messaging\Teleopti.Messaging.sln" /Rebuild "Debug" > BrokerBuild.log
%DEVENV% "%ROOTDIR%\..\..\..\Teleopti.Messaging\Teleopti.Messaging.sln" /Rebuild "Debug" > BrokerBuild.log

::Copy
XCOPY "%ROOTDIR%\..\..\..\Teleopti.Messaging\TeleOpti.Messaging.Bin\*" "%ROOTDIR%\MsgBroker\" /E /Y > NUL

::Install
ECHO Install MsgBroker Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "%ROOTDIR%\MsgBroker\Teleopti.Messaging.Svc.exe" > BrokerInstall.log