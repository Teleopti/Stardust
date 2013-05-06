@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
%ROOTDIR:~0,2%
CD "%ROOTDIR%"

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
ECHO Call was: IIS6ConfigWebAppsAndPool.bat %~1 %~3 %~3 %~4 > %logfile%

SET DefaultSite=Default Web Site
SET MainSiteName=TeleoptiCCC
SET appcmd=%systemroot%\system32\inetsrv\APPCMD.exe

for /f "tokens=2,3 delims=;" %%g in (Apps\ApplicationsInAppPool.txt) do CALL:CreateAppPool "%%g" "%%h" >> %logfile%

for /f "tokens=1,2,3 delims=;" %%g in (Apps\ApplicationsInAppPool.txt) do CALL:ForEachApplication "%%g" "%%h" "%%i" >> %logfile%
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

SET SitePath=%DefaultSite%/%MainSiteName%/%SubSiteName%

::special case for TeleoptCCC root site, skip subsite
if "%SubSiteName%"=="TeleoptiCCC" SET SitePath=%DefaultSite%/%MainSiteName%

::2 - Change app pool
echo Change app pool
echo cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/AppPoolId "%PoolName%"
cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/AppPoolId "%PoolName%"
if %errorlevel% NEQ 0 GOTO :ErrorInfo

::3 - Set AppPool credentials
::Not sure how to handle the case when user first run installation with %CustomIISUsr% and later
:: decides to revert to Network Service. Then the IWAM-user is = %CustomIISUsr% and password is still = %CustomIISPwd%
::this applies possible(?) only to "Teleopti ASP.NET 3.5/4.0", in that case a no brainer.
if "%CustomIISUsr%"=="" (
echo using Network Service as identity on AppPool
cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/AppPoolIdentityType" 2
::cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/WamUserName" "IWAM_%ComputerName%"
::cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/WamUserPass" "fakePassword"
) else (
echo using %CustomIISUsr% as identity on AppPool
cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/WamUserName" "%CustomIISUsr%"
cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/WamUserPass" "%CustomIISPwd%"
cscript "%ROOTDIR%\adsutil.vbs" SET "w3svc/AppPools/%PoolName%/AppPoolIdentityType" 3
)
echo.

::4 SSL seetings
echo cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%SitePath%/AccessSSL %SSL%
cscript "%ROOTDIR%\adsutil.vbs" set w3svc/1/root/%SitePath%/AccessSSL %SSL%
echo.

::5 .NET version
echo cscript "%ROOTDIR%\ASPNetVersion.vbs" "%MainSiteName%/%SubSiteName%" "%NETVersion%"
cscript "%ROOTDIR%\ASPNetVersion.vbs" "%MainSiteName%/%SubSiteName%" "%NETVersion%"
echo.

::5 Athentication for the virtual dir
::-----
::Different authentication based on "SDKCREDPROT"
::-----
for /f "tokens=1,2 delims=;" %%g in (%SDKCREDPROT%\IIS6Authflags.txt) do CALL:IISSecuritySet "%%g" "%%h"
SET authentication=basicAuthentication

::application mapping. Needed for asp.net MVC on IIS6
set aspnet_isapi=C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\aspnet_isapi.dll
if "%PROCESSOR_ARCHITECTURE%"=="x86" set aspnet_isapi=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\aspnet_isapi.dll

::Remove existing MVC mapping
if exist %aspnet_isapi% cscript "%ROOTDIR%\IIS6ManageMapping.vbs" W3SVC/1/root/%SitePath%/ScriptMaps "*,%aspnet_isapi%,0" "" /REMOVE /ALL /COMMIT
::Add MVC mapping
if exist %aspnet_isapi% cscript "%ROOTDIR%\IIS6ManageMapping.vbs" W3SVC/1/root/%SitePath%/ScriptMaps "" "*,%aspnet_isapi%,0" /INSERT /COMMIT
GOTO Done

:IISSecuritySet
::http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/6cc53bc1-6487-412c-ae93-063cd86b4f6e.mspx?mfr=true
if "%SubSiteName%"=="%~1" (
echo cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/Authflags %~2
cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/Authflags %~2
)
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