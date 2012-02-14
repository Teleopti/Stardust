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
GOTO Reset

:Reset
call "%ROOTDIR%StopSystem.bat"
call "%ROOTDIR%StartSystem.bat"
goto done

:done
echo done!
PING -n 4 127.0.0.1>nul