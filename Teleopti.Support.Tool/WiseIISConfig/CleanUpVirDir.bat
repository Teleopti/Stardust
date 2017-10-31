@ECHO off
SET SSL=%~1
IF "%SSL%" == "" SET SSL=0

::=============
::Main
::=============
ECHO Clean up IIS web sites and applictions ...

SET "DefaultSite="
SET MainSiteName=TeleoptiWFM
SET appcmd=%systemroot%\system32\inetsrv\APPCMD.exe

for /f "delims==" %%g in ('"%appcmd%" list site /text:name') do (
	setlocal EnableDelayedExpansion
	CALL:SetDefaultWebSiteName "%SSL%" "%%g" "%appcmd%" IsDefault
	set DefaultSite=%%g
	if !IsDefault! equ 1 goto:Break1
	endlocal
)
:Break1
::if we can't find any site on 80/443 go for "Default Web Site" as site name
IF "%DefaultSite%"=="" SET DefaultSite=Default Web Site

::remove applications
%appcmd% list app /apppool.name:"$=*Teleopti*" /xml | %appcmd% delete app /in
%appcmd% DELETE vdir "%DefaultSite%/%MainSiteName%/Client"
%appcmd% list apppool /name:"$=*Teleopti*" /xml | %appcmd% delete apppool /in

::remove web site
%appcmd% DELETE vdir "%DefaultSite%/%MainSiteName%"

iisreset /restart

DEL 
ECHO.
ECHO Done!
GOTO Done

::=============
::Functions
::=============
:SetDefaultWebSiteName
SETLOCAL
SET SSL=%~1
SET SiteName=%~2
SET appcmd=%~3
SET "DefaultSite="

if "%SSL%"=="False" (
	"%appcmd%" list site /name:"%SiteName%" | findstr /C:":80:"
	) else (
	"%appcmd%" list site /name:"%SiteName%" | findstr /C:":443:"
)
set /a output=%errorlevel%
(
ENDLOCAL
set /a "%~4=%output%+1"
)
goto:eof

:Done
echo done
GOTO :EOF
