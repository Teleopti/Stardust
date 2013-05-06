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
SET identity=Impersonate
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\%identity%.txt) do CALL:IISIdentitySet "%%g" "%%h"
GOTO Done

:IISSecurityFormsSet
if "%SubSiteName%"=="%~1" (
"%appcmd%" set config "%SitePath%" /section:system.web/authentication /mode:%~2 /commit:apphost
)
exit /B

:IISSecuritySet
if "%SubSiteName%"=="%~1" (
"%appcmd%" set config "%SitePath%" -section:system.webServer/security/authentication/%~2 /enabled:"%~3" /commit:apphost
)
exit /B

:IISIdentitySet
if "%SubSiteName%"=="%~1" "%appcmd%" set config "%SitePath%" -section:system.web/identity /impersonate:"%~2"
exit /B

:Done
echo done
GOTO :EOF

:NoInput
echo You need to provide valid parameters
GOTO :EOF