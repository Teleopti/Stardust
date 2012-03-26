::------------------
:: note: ROOTDIR
:: comes from calling batch file
::------------------
::extra pill om man vuill köra _bara_ denna batshshsh
IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..
set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set OutputDir=%ROOTDIR%\output\msgbroker

::Stop  Message Broker service
IF EXIST "%OutputDir%\Teleopti.Messaging.Svc.exe" (
NET STOP TeleoptiBrokerService

::Un-install Message Broker service
ECHO Un-Install MsgBroker Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%OutputDir%\Teleopti.Messaging.Svc.exe" /LogFile="" > "%OutputDir%\BrokerUninstall.log"

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)

::Build
ECHO %msbuild% "%ROOTDIR%\..\..\..\Teleopti.Messaging.sln" /t:Clean;Rebuild > "%OutputDir%\BrokerBuild.log"
%msbuild% "%ROOTDIR%\..\..\..\Teleopti.Messaging.sln" /t:Clean;Rebuild > "%OutputDir%\BrokerBuild.log"

::Copy
XCOPY "%ROOTDIR%\..\..\..\Teleopti.Messaging\TeleOpti.Messaging.Bin\*" "%OutputDir%" /E /Y /i > NUL

::Install
ECHO Install MsgBroker Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /LogFile="" "%OutputDir%\Teleopti.Messaging.Svc.exe" > "%OutputDir%\BrokerInstall.log"