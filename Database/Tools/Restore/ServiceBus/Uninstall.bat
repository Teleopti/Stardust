IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..
set OutputDir=%ROOTDIR%\output\servicebus


::Stop  service bus
IF EXIST "%OutputDir%\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" (
NET STOP TeleoptiServiceBus

::Un-install service bus
ECHO Un-Install TeleoptiServiceBus
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%OutputDir%\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" /LogFile=""

::Safty ...
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 5 > NUL
)