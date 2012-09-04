@echo off
SETLOCAL
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%
SET SrcCode=\\hebe\CruiseControl.NET\server\BuildMSI-main\WorkingDirectory\ccnet\SilentInstall
SET SrcShare=\\hebe\Installation\msi
SET DestShare=%ROOTDIR%\LatestMSI
SET INSTALLDIR=C:\Program Files (x86)\Teleopti

::del old files, 2 steps
::1 - Delete files older then 30 days
 forfiles /P "%DestShare%" /S /D -30 /c "cmd /c del /q @path"
::2 - Remove empty dirs
for /f "delims=" %%i in ('dir "%DestShare%" /s /b /ad ^| sort /r') do rd "%%i" > NUL

::uninstall current server MSI
Call "%DestShare%\SilentInstall\server\UnInstall.bat"

::Get latest version figure
CALL %ROOTDIR%\head.bat 1 %SrcShare%\7.2.* > %temp%\Version.txt
SET /p version= < %temp%\Version.txt
echo New version is: %version%
SET CCCEXE=%DestShare%\%version%\Teleopti CCC %version%.msi

::Copy to local
ROBOCOPY "%SrcShare%\%version%" "%DestShare%\%version%" "*.msi"

::Copy Latest SrcCode for silent install
ROBOCOPY "%SrcCode%" "%DestShare%\SilentInstall" /E

::installation
Call "%DestShare%\SilentInstall\server\SilentInstall.bat" "%CCCEXE%" "localhostDemoNoPM"

ENDLOCAL