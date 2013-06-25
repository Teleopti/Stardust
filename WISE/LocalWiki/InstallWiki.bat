@ECHO OFF

SET ROOTDIR=%~dp0

:: Get installpath from registry
SETLOCAL ENABLEEXTENSIONS
SET KEY=HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings
SET VALUE_NAME=INSTALLDIR

FOR /F "SKIP=2 TOKENS=1,2*" %%A IN ('REG QUERY "%KEY%" /v "%VALUE_NAME%" 2^>nul') DO (
	SET VALUENAME=%%A
	SET INSTALLPATH=%%C
)
IF NOT DEFINED VALUENAME (
@ECHO "%KEY%"\"%VALUE_NAME%" not found.
SET /P INSTALLDIR="Please enter Installpath (eg C:\Program Files(x86)\Teleopti\)"
)

:: Unzip wikifiles to Local folder
7za.exe x -y -o"%INSTALLPATH%\TeleoptiCCC\LocalWiki" "%ROOTDIR%wiki.teleopti.com.zip"

:: Deploy virtual directory to IIS
%SYSTEMROOT%\system32\inetsrv\APPCMD add vdir /app.name:"Default Web Site/" /path:/TeleoptiCCC/LocalWiki /physicalPath:"%INSTALLPATH%TeleoptiCCC\LocalWiki"

PAUSE