@ECHO off

SET PoolName=%~1
SET SubSiteName=%~2
SET NETVersion=%~3
SET SDKCREDPROT=%~4
SET CustomIISUsr=%~5
SET CustomIISPwd=%~6

IF "%PoolName%"=="" GOTO NoInput
IF "%SubSiteName%"=="" GOTO NoInput
IF "%NETVersion%"=="" GOTO NoInput
IF "%SDKCREDPROT%"=="" GOTO NoInput

SET DefaultSite=Default Web Site
SET MainSiteName=TeleoptiCCC

SET SitePath=%DefaultSite%/%MainSiteName%/%SubSiteName%

::special case for TeleoptCCC root site, skip subsite
if "%SubSiteName%"=="TeleoptiCCC" SET SitePath=%DefaultSite%/%MainSiteName%

::1 - Create app pool
"%systemroot%\system32\inetsrv\APPCMD.exe" list apppool /name:"%PoolName%"
if %errorlevel% NEQ 0 (
ECHO Creating Teleopti App pool ...
"%systemroot%\system32\inetsrv\APPCMD.exe" add apppool /name:"%PoolName%" /managedRuntimeVersion:%NETVersion% /managedPipelineMode:Integrated /commit:apphost
ECHO Creating Teleopti App pool. Done!
) else (
ECHO Teleopti app pool already exist: "%PoolName%"
ECHO updating managedRuntimeVersion to: %NETVersion%
"%systemroot%\system32\inetsrv\APPCMD.exe" set apppool /APPPOOL.NAME:"%PoolName%" /managedRuntimeVersion:%NETVersion% /managedPipelineMode:Integrated
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
"%systemroot%\system32\inetsrv\APPCMD.exe" set apppool /apppool.name:"%PoolName%" -processModel.identityType:SpecificUser -processModel.userName:"%CustomIISUsr%" -processModel.password:"%CustomIISPwd%"
)
echo.

::4 - Specify the authentication for the virtual dir

::basic, never used so far. Disable
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%SitePath%" -section:system.webServer/security/authentication/basicAuthentication /enabled:"False" /commit:apphost

::At this point we bail out for most vdir. Go for what Wise created
::Wise uses:
::anonymousAuthentication=true
::windowsAuthentication=true/false + Forms=true/false is depending on web.config which in turn depends on "%SDKCREDPROT%" which in turn depends on "same" vs. "different domain" in Msi GUI.
if not "%SubSiteName%"=="Web" GOTO Done

::But for vdir="Web" we continue
if "%SDKCREDPROT%"=="Ntlm" (
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%SitePath%" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"False" /commit:apphost
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%SitePath%" -section:system.webServer/security/authentication/windowsAuthentication /enabled:"True" /commit:apphost
) Else (
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%SitePath%" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"True" /commit:apphost
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%SitePath%" -section:system.webServer/security/authentication/windowsAuthentication /enabled:"False" /commit:apphost
)

GOTO Done

:Done
echo done
GOTO :EOF

:NoInput
echo You need to provide valid parameters
GOTO :EOF