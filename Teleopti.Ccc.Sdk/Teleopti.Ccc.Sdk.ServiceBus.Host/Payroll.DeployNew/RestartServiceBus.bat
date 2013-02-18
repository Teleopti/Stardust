@echo off
CHOICE /C yn /M "Would you like to clean up obsolete files from the Payroll folder?"
IF ERRORLEVEL 1 SET /A clean=1
IF ERRORLEVEL 2 SET /A clean=0

net stop TeleoptiServiceBus
ping -n 3 127.0.0.1 > NUL
if %clean% equ 1 call:clean
net start TeleoptiServiceBus
pause
goto :eof

:clean
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
CD "%ROOTDIR%\..\Payroll"
del /F /Q /S *.*
exit /b