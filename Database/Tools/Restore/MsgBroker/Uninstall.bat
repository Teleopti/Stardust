::Stop  Message Broker service
IF EXIST Teleopti.Messaging.Svc.exe (
NET STOP TeleoptiBrokerService

::Un-install Message Broker service
ECHO Un-Install MsgBroker Service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u Teleopti.Messaging.Svc.exe

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)