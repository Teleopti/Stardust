@ECHO off
color A
IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..
Set ImagePath=

::exist as service?
reg query HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\TeleoptiServiceBus /v ImagePath
if %errorlevel% NEQ 0 (
ECHO.
ECHO service does not exist on this system & GOTO :eof
GOTO :EOF
)

::get install path
for /f "tokens=2*" %%A in ('REG QUERY "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\TeleoptiServiceBus" /v ImagePath') DO (
  for %%F in (%%B) do (
    set ImagePath=%%F
  )
)

if not %ImagePath%=="" (
NET STOP TeleoptiServiceBus
"C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" /u %ImagePath% /LogFile=""
ECHO Safty stop. Wait for a few secs ...
PING 127.0.0.1 -n 3 > NUL
)

