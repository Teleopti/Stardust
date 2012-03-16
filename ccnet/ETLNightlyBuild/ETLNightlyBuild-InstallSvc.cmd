@ECHO off
SET ServiceName=%~1
SET ExePath=%~2
SET ExeName=%~3

::IF Exists: Try to un-install
ECHO "%~dp0ETLNightlyBuild-UnInstallSvc.cmd" "%ServiceName%" "%ExePath%" %ExeName%
SC query %ServiceName% | FIND "1060" > NUL
IF %errorlevel% NEQ 0 CALL "%~dp0ETLNightlyBuild-UnInstallSvc.cmd" "%ServiceName%" "%ExePath%" %ExeName%


::IF Still Exists: Something is wrong
SC query %ServiceName% | FIND "1060" > NUL
IF %errorlevel% NEQ 0 GOTO :svc_exist

::Install service
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" "%ExePath%\%ExeName%"
if %errorlevel% NEQ 0 goto :failed_create

GOTO :finished

:svc_exist
ECHO The service %ServiceName% already exists!

GOTO :eof

:failed_create
ECHO The service %ServiceName% failed to install!
GOTO :eof

:finished
ECHO service %ServiceName% installed