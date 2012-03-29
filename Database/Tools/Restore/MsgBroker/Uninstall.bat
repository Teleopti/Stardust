IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..
set OutputDir=%ROOTDIR%\output\msgbroker

::Stop  Message Broker service
IF EXIST "%OutputDir%\Teleopti.Messaging.Svc.exe" (
NET STOP TeleoptiBrokerService

::Un-install Message Broker service
ECHO Un-Install MsgBroker Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%OutputDir%\Teleopti.Messaging.Svc.exe" /LogFile=""

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)