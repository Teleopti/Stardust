@ECHO off
::Example Call:
::IIS7ConfigWebAppsAndPool.bat "Teleopti ASP.NET v3.5" SDK v2.0 False Skip
::IIS7ConfigWebAppsAndPool.bat "Teleopti ASP.NET v3.5" SDK v2.0 True Skip
::IIS7ConfigWebAppsAndPool.bat "Teleopti ASP.NET v4.0" web v4.0 True Ntlm MySpecialIISuser MySpecialPwd
SET PoolName=%~1
SET SubSiteName=%~2
SET NETVersion=%~3
SET SSL=%~4
SET SDKCREDPROT=%~5
SET CustomIISUsr=%~6
SET CustomIISPwd=%~7

SET SSLPORT=443

IF "%PoolName%"=="" GOTO NoInput
IF "%SubSiteName%"=="" GOTO NoInput
IF "%NETVersion%"=="" GOTO NoInput
IF "%SDKCREDPROT%"=="" GOTO NoInput

SET DefaultSite=Default Web Site
SET MainSiteName=TeleoptiCCC
SET appcmd=%systemroot%\system32\inetsrv\APPCMD.exe

SET SitePath=%DefaultSite%/%MainSiteName%/%SubSiteName%

::special case for TeleoptCCC root site, skip subsite
if "%SubSiteName%"=="TeleoptiCCC" SET SitePath=%DefaultSite%/%MainSiteName%

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

::2 - Change app pool
%windir%\system32\inetsrv\APPCMD.exe set app "%SitePath%" /applicationPool:"%PoolName%" /commit:apphost
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
if "%SSL%"=="True" "%appcmd%" set config "%SitePath%" /section:access /sslFlags:Ssl /commit:APPHOST
if "%SSL%"=="False" "%appcmd%" set config "%SitePath%" /section:access /sslFlags:None /commit:APPHOST

::5 Athentication for the virtual dir
::-----
::Basic Auth, never used so far. Always Disable
::-----
"%appcmd%" set config "%SitePath%" -section:system.webServer/security/authentication/basicAuthentication /enabled:"False" /commit:apphost

::At this point we bail out for most vdir and go for the Authetication created by Wise
::Wise uses:
::windowsAuthentication=true/false + Forms=true/false is depending on web.config which in turn depends on "%SDKCREDPROT%" which in turn depends on "same" vs. "different domain" in Msi GUI.
if not "%SubSiteName%"=="Web" GOTO Done

::But for vdir="Web" we continue
if "%SDKCREDPROT%"=="Ntlm" (
"%appcmd%" set config "%SitePath%" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"False" /commit:apphost
"%appcmd%" set config "%SitePath%" -section:system.webServer/security/authentication/windowsAuthentication /enabled:"True" /commit:apphost
) Else (
"%appcmd%" set config "%SitePath%" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"True" /commit:apphost
"%appcmd%" set config "%SitePath%" -section:system.webServer/security/authentication/windowsAuthentication /enabled:"False" /commit:apphost
)

GOTO Done

:Done
echo done
GOTO :EOF

:NoInput
echo You need to provide valid parameters
GOTO :EOF