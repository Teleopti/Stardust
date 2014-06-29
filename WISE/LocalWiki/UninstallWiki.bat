@ECHO OFF

:: Remove virtual directory from IIS
%systemroot%\system32\inetsrv\APPCMD delete vdir /vdir.name:"Default Web Site/TeleoptiWFM/LocalWiki"

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

:: Remove local content
RMDIR /S /Q "%INSTALLPATH%\TeleoptiWFM\LocalWiki"


PAUSE