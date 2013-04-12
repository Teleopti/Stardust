@echo off
COLOR A

::Get path to this batchfile
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
CD "%ROOTDIR%"

::uninstall CruiseControl
SC qc CCService | FIND "The specified service does not exist as an installed service" > NUL
IF %errorlevel% NEQ 0 (
RMDIR "C:\Program Files (x86)\CruiseControl.NET\server" /S /Q
"C:\Program Files (x86)\CruiseControl.NET\uninst.exe"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/ccnet"
echo.
echo ========================
echo can not control the uninstall process. Press any key when it's done!
echo ========================
echo.
COLOR E
pause
COLOR A
)