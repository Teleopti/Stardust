@echo off

::Get path to this batchfile
SET ROOTDIR=%~dp0
SET DRIVELETTER=%ROOTDIR:~0,2%

::Change drive letter, needed for powershell
%DRIVELETTER%
CD "%ROOTDIR%"

::init as "false"
SET /A exec=0

IF "%~1"=="" (
call :Userinput exec
) else (
set /A exec=1
)

if %exec% equ 1 call :Reset
GOTO Done

:Userinput
SETLOCAL
ECHO This will stop and start all Teleopti services and App Pools
CHOICE /C yn /M "Do you want to continue"
IF ERRORLEVEL 1 SET /a ANSWER=1
IF ERRORLEVEL 2 SET /a ANSWER=0
(
ENDLOCAL
set /A "%~1=%ANSWER%"
)
exit /b

:Reset
For /f "tokens=2 delims=[]" %%G in ('ver') Do (set version=%%G)
For /f "tokens=2,3,4 delims=. " %%G in ('echo %version%') Do (set /A major=%%G & set minor=%%H & set build=%%I) 

if %major% equ 5 (
call StopSystem.bat
call StartSystem.bat
)

if %major% GEQ 6 (
powershell set-executionpolicy unrestricted
ECHO powershell . .\RestartTeleopti.ps1
powershell . .\RestartTeleopti.ps1
)

exit /b

:done
echo done!
PING -n 4 127.0.0.1>nul