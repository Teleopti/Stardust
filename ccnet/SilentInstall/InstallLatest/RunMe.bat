@echo off
SETLOCAL
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%
SET SrcCode=\\hebe\c$\Program Files (x86)\CruiseControl.NET\server\RaptorMain\WorkingDirectory
SET SrcShare=\\hebe\Installation\msi
SET DestShare=%ROOTDIR%\LatestMSI
SET INSTALLDIR=C:\Program Files (x86)\Teleopti
SET /p ActiveMajorVersion= < "%SrcCode%\ActiveMajorVersion.txt"
SET /p ActiveBranchVersion= < "%SrcCode%\ActiveBranchVersion.txt"

::make last figure a integer
SET /a ActiveBranchVersion=%ActiveBranchVersion%

::del old files, 2 steps
::1 - Delete files older then 30 days
 forfiles /P "%DestShare%" /S /D -30 /c "cmd /c del /q @path"
::2 - Remove empty dirs
for /f "delims=" %%i in ('dir "%DestShare%" /s /b /ad ^| sort /r') do rd "%%i" > NUL

::uninstall current server MSI
Call "%DestShare%\SilentInstall\server\UnInstall.bat"

::Get latest version figure
CALL %ROOTDIR%\head.bat 1 %SrcShare%\%ActiveMajorVersion%.%ActiveBranchVersion%.* > %temp%\Version.txt
SET /p version= < %temp%\Version.txt
echo New version is: %version%
SET CCCEXE=%DestShare%\%version%\Teleopti CCC %version%.msi

::Copy to local
ROBOCOPY "%SrcShare%\%version%" "%DestShare%\%version%" "*.msi"

::Copy Latest SrcCode for silent install
ROBOCOPY "%SrcCode%\ccnet\SilentInstall" "%DestShare%\SilentInstall" /MIR

::installation
Call "%DestShare%\SilentInstall\server\SilentInstall.bat" "%CCCEXE%" "localhostDemoNoPM" install

::Add Lic
SQLCMD -S. -E -d"TeleoptiCCC7_Demo" -i"%SrcCode%\Database\Tools\Restore\tsql\AddLic.sql" -v LicFile="%SrcCode%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"

ENDLOCAL