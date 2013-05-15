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
SET logfile=IIS7ConfigWebAppsAndPool.log
SET SSLPORT=443

IF "%SDKCREDPROT%"=="" GOTO NoInput

::=============
::Main
::=============
ECHO Settings up IIS web sites and applictions ...
ECHO Call was: IIS6ConfigWebAppsAndPool.bat %~1 %~2 %~3 %~4 > %logfile%

SET DefaultSite=Default Web Site
SET MainSiteName=TeleoptiCCC
SET appcmd=%systemroot%\system32\inetsrv\APPCMD.exe

::remove applications
for /f "tokens=2,3,4,5 delims=;" %%g in ('FINDSTR /C:"Level2;" Apps\ApplicationsInAppPool.txt') do CALL:DeleteApp "%DefaultSite%/%MainSiteName%" "%%g" "%%j" >> %logfile%
for /f "tokens=2,3,4,5 delims=;" %%g in ('FINDSTR /C:"Level1;%MainSiteName%;" Apps\ApplicationsInAppPool.txt') do CALL:DeleteApp "%DefaultSite%" "%%g" "%%j" >> %logfile%

::create AppPools
for /f "tokens=3,4 delims=;" %%g in (Apps\ApplicationsInAppPool.txt) do CALL:CreateAppPool "%%g" "%%h" >> %logfile%

::create applications
for /f "tokens=2,3,4,5 delims=;" %%g in ('FINDSTR /C:"Level1;%MainSiteName%;" Apps\ApplicationsInAppPool.txt') do CALL:CreateApp "%DefaultSite%" "%%g" "%%g" "%%j" "%INSTALLDIR%" >> %logfile%
for /f "tokens=2,3,4,5 delims=;" %%g in ('FINDSTR /C:"Level2;" Apps\ApplicationsInAppPool.txt') do CALL:CreateApp "%DefaultSite%" "%MainSiteName%/%%g" "%%g" "%%j" "%INSTALLDIR%\%MainSiteName%" >> %logfile%

::config applications
for /f "tokens=2,3,4,5 delims=;" %%g in (Apps\ApplicationsInAppPool.txt) do CALL:ForEachApplication "%%g" "%%h" "%%i" "%%j" >> %logfile%

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
"%appcmd%" list apppool /name:"%PoolName%"
if %errorlevel% NEQ 0 (
	ECHO Creating Teleopti App pool ...
	"%appcmd%" add apppool /name:"%PoolName%" /managedRuntimeVersion:%NETVersion% /managedPipelineMode:Integrated /commit:apphost
	ECHO Creating Teleopti App pool. Done!
) else (
	ECHO Teleopti app pool already exist: "%PoolName%"
	ECHO updating managedRuntimeVersion to: %NETVersion%
	"%appcmd%" set apppool /APPPOOL.NAME:"%PoolName%" /managedRuntimeVersion:%NETVersion% /managedPipelineMode:Integrated
)
echo.
exit /B

:DeleteApp
if "%~3"=="app" (
ECHO "%appcmd%" delete app /app.name:"%~1/%~2"
"%appcmd%" delete app /app.name:"%~1/%~2"
)

if "%~3"=="vdir" (
ECHO "%appcmd%" delete vdir /vdir.name:"%~1/%~2"
"%appcmd%" delete vdir /vdir.name:"%~1/%~2"
)

goto:eof

:CreateApp
echo %~1 %~2 %~3 %~4

if "%~4"=="app" (
ECHO "%appcmd%" add app /site.name:"%~1" /path:/%~2 /physicalPath:"%~5\%~3"
"%appcmd%" add app /site.name:"%~1" /path:/%~2 /physicalPath:"%~5\%~3"
)

if "%~4"=="vdir" (
echo "%appcmd%" add vdir /app.name:"%~1/" /path:/%~2 /physicalPath:"%~5\%~3"
"%appcmd%" add vdir /app.name:"%~1/" /path:/%~2 /physicalPath:"%~5\%~3"
)
goto:eof

:ForEachApplication
SET SubSiteName=%~1
SET PoolName=%~2
SET NETVersion=%~3
SET SiteOrApp=%~4

SET SitePath=%MainSiteName%/%SubSiteName%
SET FolderPath=%MainSiteName%\%SubSiteName%

::special case for TeleoptCCC root site, skip subsite
if "%SubSiteName%"=="TeleoptiCCC" SET SitePath=%MainSiteName%
if "%SubSiteName%"=="TeleoptiCCC" SET FolderPath=%MainSiteName%

::2 - Change app pool
%windir%\system32\inetsrv\APPCMD.exe set app "%DefaultSite%/%SitePath%" /applicationPool:"%PoolName%" /commit:apphost
echo.

::3 - Set AppPool credentials
if "%CustomIISUsr%"=="" (
echo using ApplicationPoolIdentity as identity on AppPool
%windir%\system32\inetsrv\appcmd.exe set AppPool /apppool.name:"%PoolName%" -processModel.identityType:ApplicationPoolIdentity
) else (
echo using %CustomIISUsr% as identity on AppPool
"%appcmd%" set apppool /apppool.name:"%PoolName%" -processModel.identityType:SpecificUser -processModel.userName:"%CustomIISUsr%" -processModel.password:"%CustomIISPwd%"
)
echo.

::4 SSL seetings
if "%SSL%"=="True" "%appcmd%" set config "%DefaultSite%/%SitePath%" /section:access /sslFlags:Ssl /commit:APPHOST
if "%SSL%"=="False" "%appcmd%" set config "%DefaultSite%/%SitePath%" /section:access /sslFlags:None /commit:APPHOST

::5 Athentication for the virtual dir
::-----
::Different authentication based on "SDKCREDPROT"
::-----
SET authentication=anonymousAuthentication
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\%authentication%.txt) do CALL:IISSecuritySet "%%g" "%authentication%" "%%h"
SET authentication=basicAuthentication
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\%authentication%.txt) do CALL:IISSecuritySet "%%g" "%authentication%" "%%h"
SET authentication=windowsAuthentication
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\%authentication%.txt) do CALL:IISSecuritySet "%%g" "%authentication%" "%%h"
::a little different for Forms section
SET authentication=FormsAuthentication
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\%authentication%.txt) do CALL::IISSecurityFormsSet "%%g" "%%h"

::6 Impersonate
::-----
::Different identity based on "SDKCREDPROT"
::-----
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\Impersonate.txt) do CALL:IISIdentitySet "%%g" "%%h"
exit /B

:IISSecurityFormsSet
if "%SubSiteName%"=="%~1" (
"%appcmd%" set config "%DefaultSite%/%SitePath%" /section:system.web/authentication /mode:%~2
)
exit /B

:IISSecuritySet
if "%SubSiteName%"=="%~1" (
ECHO "%appcmd%" set config "%DefaultSite%/%SitePath%" -section:system.webServer/security/authentication/%~2 /enabled:"%~3" /commit:apphost
"%appcmd%" set config "%DefaultSite%/%SitePath%" -section:system.webServer/security/authentication/%~2 /enabled:"%~3" /commit:apphost
)
exit /B

:IISIdentitySet
if "%SubSiteName%"=="%~1" "%appcmd%" set config "%DefaultSite%/%SitePath%" -section:system.web/identity /impersonate:"%~2"
exit /B

:Done
echo done
GOTO :EOF

:NoInput
echo You need to provide valid parameters
GOTO :EOF