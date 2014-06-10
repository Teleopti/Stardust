@echo off

::Get path to this batchfile
SET ROOTDIR=%~dp0

IF %1.==. GOTO Manual
GOTO Reset

:Manual
ECHO This will stop and start all Teleopti services and App Pools
SET /P ANSWER=Do you want to continue (Y/N)?
if /I "%ANSWER%"=="y" goto yes
if /I "%ANSWER%"=="yes" goto yes

goto no

:no
echo ok, fine with me. Will abort.
goto done

:yes
echo System will be stopped and restarted ...
call :Reset
goto done

:Reset
Setlocal
For /f "tokens=2 delims=[]" %%G in ('ver') Do (set _version=%%G) 
For /f "tokens=2,3,4 delims=. " %%G in ('echo %_version%') Do (set /A _major=%%G& set _minor=%%H& set _build=%%I) 

if %_major% equ 5 (
call "%ROOTDIR%StopSystem.bat"
call "%ROOTDIR%StartSystem.bat"
)

if %_major% GEQ 6 (
powershell set-executionpolicy unrestricted
powershell ". %ROOTDIR%RestartTeleopti.ps1; main"
)

endlocal
exit /b

:done
echo done!
PING -n 4 127.0.0.1>nul