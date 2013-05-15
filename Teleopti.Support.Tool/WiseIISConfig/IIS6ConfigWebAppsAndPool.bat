@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
%ROOTDIR:~0,2%
CD "%ROOTDIR%"

SET INSTALLDIR=%ROOTDIR:~0,-27%

::Example Call:
::note: parameter 3,4 are optional
::IIS7ConfigWebAppsAndPool.bat [IS_SSL] [SDK_CREDPROT] [MYUSER] [MYPASSWORD]
::IIS7ConfigWebAppsAndPool.bat True Ntlm MySpecialIISuser MySpecialPwd
SET SSL=%~1
SET SDKCREDPROT=%~2
SET CustomIISUsr=%~3
SET CustomIISPwd=%~4
SET logfile=IIS6ConfigWebAppsAndPool.log
SET SSLPORT=443

IF "%SDKCREDPROT%"=="" GOTO NoInput

::=============
::Main
::=============
ECHO Settings up IIS web sites and applictions ...
ECHO Call was: IIS6ConfigWebAppsAndPool.bat %~1 %~2 %~3 %~4 > %logfile%

SET DefaultSite=Default Web Site
SET MainSiteName=TeleoptiCCC

for /f "tokens=3,4 delims=;" %%g in (Apps\ApplicationsInAppPool.txt) do CALL:CreateAppPool "%%g" "%%h" >> %logfile%

for /f "tokens=2,3,4,5,6,7 delims=;" %%g in (Apps\ApplicationsInAppPool.txt) do CALL:ForEachApplication "%%g" "%%h" "%%i" "%%j" "%%k" "%%l" >> %logfile%

::just in case
iisreset /restart
ECHO.
ECHO Done!
GOTO Done

::=============
::Functions
::=============
:CreateAppPool
SET PoolName=%~1
SET NETVersion=%~2

::1 - Create app pool
echo cscript "%ROOTDIR%\adsutil.vbs" ENUM "w3svc/AppPools/%PoolName%"
cscript "%ROOTDIR%\adsutil.vbs" ENUM "w3svc/AppPools/%PoolName%"
if %errorlevel% NEQ 0 (
	ECHO Creating Teleopti App pool ...
	echo cscript "%ROOTDIR%\adsutil.vbs" CREATE "w3svc/AppPools/%PoolName%" IIsApplicationPool
	cscript "%ROOTDIR%\adsutil.vbs" CREATE "w3svc/AppPools/%PoolName%" IIsApplicationPool
	ECHO Creating Teleopti App pool. Done!
) else (
	ECHO Teleopti app pool already exist: "%PoolName%"
)
echo.
Exit /B

:ForEachApplication
SET SubSiteName=%~1
SET PoolName=%~2
SET NETVersion=%~3
SET SiteOrApp=%~4
SET DefaultDocs=%~5
SET aspnetisapi=%~6

SET SitePath=%MainSiteName%/%SubSiteName%
SET FolderPath=%MainSiteName%\%SubSiteName%

::special case for TeleoptCCC root site, skip subsite
if "%SubSiteName%"=="TeleoptiCCC" SET SitePath=%MainSiteName%
if "%SubSiteName%"=="TeleoptiCCC" SET FolderPath=%MainSiteName%

::remove old stuff
echo cscript "%ROOTDIR%\adsutil.vbs" delete w3svc/1/root/%MainSiteName%/ContextHelp
cscript "%ROOTDIR%\adsutil.vbs" delete w3svc/1/root/%MainSiteName%/ContextHelp

::remove + re-add
CALL:CreateApplication "%SitePath%" "%SubSiteName%" "%INSTALLDIR%\%FolderPath%" "%SiteOrApp%" "%DefaultDocs%"

if "%SiteOrApp%"=="app" (
	echo Change app pool
	echo cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/AppPoolId "%PoolName%"
	cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/AppPoolId "%PoolName%"
	echo Set identity on App Pool
	if "%CustomIISUsr%"=="" (
		echo using Network Service as identity on AppPool
		cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/AppPoolIdentityType" 2
	) else (
		echo using %CustomIISUsr% as identity on AppPool
		cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/WamUserName" "%CustomIISUsr%"
		cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/WamUserPass" "%CustomIISPwd%"
		cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/AppPoolIdentityType" 3
	)
	echo setting .Net version
	echo cscript "%ROOTDIR%\ASPNetVersion.vbs" "%SitePath%" "%NETVersion%"
	cscript "%ROOTDIR%\ASPNetVersion.vbs" "%SitePath%" "%NETVersion%"
	echo.
)
echo.

::SSL seetings
echo cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%SitePath%/AccessSSL %SSL%
cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%SitePath%/AccessSSL %SSL%
echo.

::Authentication for the virtual dir
::AuthFlag is a bitmask: http://msdn.microsoft.com/en-us/library/ms524513(v=vs.90).aspx
SET /A AuthFlag=0

SET authentication=anonymousAuthentication
for /f "tokens=1,2 delims=;" %%g in ('findstr /C:"%SubSiteName%;" /I %SDKCREDPROT%\%authentication%.txt') do CALL:BitMaskGet "%%g" "%authentication%" "%%h" %AuthFlag% AuthFlag
SET authentication=windowsAuthentication
for /f "tokens=1,2 delims=;" %%g in ('findstr /C:"%SubSiteName%;" /I %SDKCREDPROT%\%authentication%.txt') do CALL:BitMaskGet "%%g" "%authentication%" "%%h" %AuthFlag% AuthFlag

CALL:IISSecuritySet "%SitePath%" "%AuthFlag%"

::application mapping. Needed for asp.net MVC on IIS6
ECHO.
ECHO setting script maps aka isapi filter ...
if defined ProgramFiles(x86) (
echo is Wow6432
set aspnet_isapi=C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\aspnet_isapi.dll
) else (
echo is 32-bit
set aspnet_isapi=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\aspnet_isapi.dll
)

::Remove + Add existing MVC mapping
if not "%aspnetisapi%"=="None" (
	if exist %aspnet_isapi% cscript "%ROOTDIR%\IIS6ManageMapping.vbs" W3SVC/1/root/%SitePath%/ScriptMaps "*,%aspnet_isapi%,0" "" /REMOVE /ALL /COMMIT
	if exist %aspnet_isapi% cscript "%ROOTDIR%\IIS6ManageMapping.vbs" W3SVC/1/root/%SitePath%/ScriptMaps "" "*,%aspnet_isapi%,0" /INSERT /COMMIT
)

::disable directoryBrowse
cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%SitePath%/enabledirbrowsing "False"

GOTO Done

:BitMaskGet
SETLOCAL
SET /A AuthFlag=0
if "%~2"=="anonymousAuthentication" (
	if "%~3"=="True" set /A AuthFlag=1
)
if "%~2"=="windowsAuthentication" (
	if "%~3"=="True" set /A AuthFlag=4
)
(
ENDLOCAL
set /a "%~5=%AuthFlag%+%5"
)
goto:eof

:CreateApplication
::delete VirDir
echo cscript "%ROOTDIR%\adsutil.vbs" delete w3svc/1/root/%~1
cscript "%ROOTDIR%\adsutil.vbs" delete w3svc/1/root/%~1

::create VirDir and App
echo cscript "%ROOTDIR%\adsutil.vbs" create_vdir w3svc/1/root/%~1
cscript "%ROOTDIR%\adsutil.vbs" create_vdir w3svc/1/root/%~1

if "%~4"=="app" (
	echo cscript "%ROOTDIR%\adsutil.vbs" appcreateinproc w3svc/1/root/%~1
	cscript "%ROOTDIR%\adsutil.vbs" appcreateinproc w3svc/1/root/%~1

	echo cscript "%ROOTDIR%\adsutil.vbs" appenable w3svc/1/root/%~1
	cscript "%ROOTDIR%\adsutil.vbs" appenable w3svc/1/root/%~1
)

echo cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%~1/appfriendlyname "%~2"
cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%~1/appfriendlyname "%~2"

::link to disk
echo cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%~1/path "%~3"
cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%~1/path "%~3"

::disbable default docs for all
cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/1/Root/%~1/DefaultDoc" ""
cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/1/Root/%~1/EnableDefaultDoc" False

::enable default docs for some
if not "%~5"=="None" (
	cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/1/Root/%~1/DefaultDoc" "%~5"
	cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/1/Root/%~1/EnableDefaultDoc" True
)
goto:eof

:IISSecuritySet
::http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/6cc53bc1-6487-412c-ae93-063cd86b4f6e.mspx?mfr=true
echo cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%~1/Authflags %~2
cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%~1/Authflags %~2
exit /B

:Done
echo done
GOTO :EOF

:NoInput
echo You need to provide valid parameters
GOTO :eof

:ErrorInfo
ECHO Somehing went wrong when we try to move the please review
ping 127.0.0.1 -n 4 > NUL
GOTO :eof