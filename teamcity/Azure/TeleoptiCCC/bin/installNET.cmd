REM Set the value of netfx to install appropriate .NET Framework. 
REM ***** To install .NET 4.5.2 set the variable netfx to "NDP452" *****
REM ***** To install .NET 4.6 set the variable netfx to "NDP46" *****
REM ***** To install .NET 4.6.1 set the variable netfx to "NDP461" *****
REM ***** To install .NET 4.7.2 set the variable netfx to "NDP472" ***** http://go.microsoft.com/fwlink/?LinkId=863262

::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%

set netfx="NDP472"

REM ***** Set script start timestamp *****
set timehour=%time:~0,2%
set timestamp=%date:~-4,4%%date:~-10,2%%date:~-7,2%-%timehour: =0%%time:~3,2%
set "log=install.cmd started %timestamp%."

REM ***** Exit script if running in Emulator *****
if %ComputeEmulatorRunning%=="true" goto exit

REM ***** Needed to correctly install .NET 4.6.1, otherwise you may see an out of disk space error *****
set TMP=%PathToNETFXInstall%
set TEMP=%PathToNETFXInstall%

REM ***** Setup .NET filenames and registry keys *****
if %netfx%=="NDP472" goto NDP472
if %netfx%=="NDP46" goto NDP46
    set "netfxinstallfile=NDP452-KB2901954-Web.exe"
    set netfxregkey="0x5cbf5"
    goto logtimestamp

:NDP46
set "netfxinstallfile=NDP46-KB3045560-Web.exe"
set netfxregkey="0x60051"
goto logtimestamp

:NDP461
set "netfxinstallfile=NDP461-KB3102438-Web.exe"
set netfxregkey="0x6041f"

:NDP472
set "netfxinstallfile=NDP472-KB4054531-Web.exe"
set netfxregkey="0x70BF6"
goto logtimestamp

:logtimestamp
REM ***** Setup LogFile with timestamp *****
md "%PathToNETFXInstall%\log"
set startuptasklog="%PathToNETFXInstall%log\startuptasklog-%timestamp%.txt"
set netfxinstallerlog="%PathToNETFXInstall%log\NetFXInstallerLog-%timestamp%"
echo %log% >> %startuptasklog%
echo Logfile generated at: %startuptasklog% >> %startuptasklog%
echo TMP set to: %TMP% >> %startuptasklog%
echo TEMP set to: %TEMP% >> %startuptasklog%

REM ***** Check if .NET is installed *****
echo Checking if .NET (%netfx%) is installed >> %startuptasklog%
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release | Find %netfxregkey%
if %ERRORLEVEL%== 0 goto installed

REM ***** Installing .NET *****
echo Installing .NET with commandline: start /wait %~dp0%netfxinstallfile% /q /serialdownload /log %netfxinstallerlog%  /chainingpackage "CloudService Startup Task" >> %startuptasklog%
start /wait %~dp0%netfxinstallfile% /q /norestart /serialdownload /log %netfxinstallerlog% /chainingpackage "CloudService Startup Task" >> %startuptasklog% 2>>&1
if %ERRORLEVEL%== 0 goto installed
    echo .NET installer exited with code %ERRORLEVEL% >> %startuptasklog%   
    if %ERRORLEVEL%== 3010 goto restart
    if %ERRORLEVEL%== 1641 goto restart
    echo .NET (%netfx%) install failed with Error Code %ERRORLEVEL%. Further logs can be found in %netfxinstallerlog% >> %startuptasklog%

:restart
echo PendingReboot >> PendingReboot.txt
echo Setting Flag for "Restarting to complete .NET" (%netfx%) installation >> %startuptasklog%
goto end

:installed
del %DIRECTORY%\PendingReboot.txt
echo .NET (%netfx%) is installed >> %startuptasklog%

:end
echo install.cmd completed: %date:~-4,4%%date:~-10,2%%date:~-7,2%-%timehour: =0%%time:~3,2% >> %startuptasklog%

:exit
EXIT /B 0