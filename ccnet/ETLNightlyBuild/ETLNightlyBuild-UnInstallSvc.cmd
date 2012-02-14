@ECHO off
SET ServiceName=%1
SET ExePath=%2
SET ExeName=%3

::If Running then Stop
SC query "%ServiceName%" | FIND "STATE" > NUL
IF errorlevel 0 GOTO :running

GOTO :uninstall

:running
ECHO stopping service ...
ECHO NET STOP %ServiceName%
NET STOP %ServiceName%

GOTO :uninstall

::Service Is installed and stopped Then uninstall
:uninstall
ECHO "C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%ExePath%\%ExeName%"
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" /u "%ExePath%\%ExeName%"
::SC delete %ServiceName%
if %errorlevel% NEQ 0 goto :failed

GOTO :finished

:failed
ECHO The service %ServiceName% failed to uninstall!
set %errorlevel%=0
GOTO :eof

:svc_not_exist
ECHO The service %ServiceName% does not exist
set %errorlevel%=0
GOTO :eof

:finished
ECHO service %ServiceName% uninstalled