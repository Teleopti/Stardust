@echo off
::Example: 7.1.334.48819 0 C:\Installation
::Get CCNet input
SET version=%1

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DriveLetter=%ROOTDIR:~0,2%

SET buildServerRoot=%DriveLetter%\installation
SET OUTDIR=%buildServerRoot%\AzurePackage
SET MSIDIR=%buildServerRoot%\msi
SET DEPLOYSHARE=%buildServerRoot%\PreviousBuilds

IF "%version%"=="" GOTO :noinput

SET AzureWork=%ROOTDIR%\Temp
SET ConfigPath=%AzureWork%
SET ContentDest=%AzureWork%\AzureContent
SET ContentSource=%DEPLOYSHARE%\%version%
SET msi=%MSIDIR%\%version%\Teleopti CCC AzureConfig %version%.msi
SET output=%OUTDIR%\%version%

::Get us to correct reletive location
%DriveLetter%
CD "%ROOTDIR%"

::create working + output dir
IF NOT EXIST "%AzureWork%" MKDIR "%AzureWork%"
IF NOT EXIST "%output%" MKDIR "%output%"

::Copy StartupTask and root dir
XCOPY /e /d /y "%ROOTDIR%\TeleoptiCCC" "%ContentDest%\TeleoptiCCC\"

::Get content from Previous build (1) to Content foler (2)
Echo Getting Previous build ...
for /f "tokens=1,2 delims=," %%g in (contentMapping.txt) do ROBOCOPY "%ContentSource%\%%g" "%ContentDest%\%%h" /mir /XF *.pdb*
Echo Getting Previous build. Done

::Get ReportViewer
XCOPY /d /y "%buildServerRoot%\Dependencies\ccc7_server\ReportViewer2010.exe" "%ContentDest%\TeleoptiCCC\bin"

::update config and run scpack
FOR /F %%G IN ('DIR /B Customer\*.txt') DO CALL DeployConfig.bat %%G

GOTO :eof

:noinput
ECHO Sorry, you need to provide a version!
ping 127.0.0.1 -n 5 > NUL
exit /b