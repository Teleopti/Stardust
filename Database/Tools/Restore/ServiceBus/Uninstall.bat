::Stop  service bus
IF EXIST Teleopti.Ccc.Sdk.ServiceBus.Host.exe(
NET STOP TeleoptiServiceBus

::Un-install service bus
ECHO Un-Install TeleoptiServiceBus
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u Teleopti.Ccc.Sdk.ServiceBus.Host.exe

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)