@ECHO off
::Example Call:
::IIS6ConfigWebAppsAndPool.bat "Teleopti ASP.NET v3.5" SDK v2.0 False Skip
::IIS6ConfigWebAppsAndPool.bat "Teleopti ASP.NET v3.5" SDK v2.0 True Skip
::IIS6ConfigWebAppsAndPool.bat "Teleopti ASP.NET v4.0" web v4.0 True Ntlm MySpecialIISuser MySpecialPwd

::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

SET PoolName=%~1
SET SubSiteName=%~2
SET NETVersion=%~3
SET SSL=%~4
SET SDKCREDPROT=%~5
SET CustomIISUsr=%~6
SET CustomIISPwd=%~7

IF "%PoolName%"=="" GOTO NoInput
IF "%SubSiteName%"=="" GOTO NoInput
IF "%NETVersion%"=="" GOTO NoInput
IF "%SDKCREDPROT%"=="" GOTO NoInput

SET MainSiteName=TeleoptiCCC
SET SitePath=%MainSiteName%/%SubSiteName%

::special case for TeleoptCCC root site, skip subsite
if "%SubSiteName%"=="TeleoptiCCC" SET SitePath=%MainSiteName%

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

::6 - Specify the authentication for the virtual dir
::http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/e3a60d16-1f4d-44a4-9866-5aded450956f.mspx?mfr=true

::We have this config in two places. Trust BuildArtifacts to be correct and let WISE do the dynamic content stuff
::anonymousAuthentication=always true
::windowsAuthentication=true/false + Forms=true/false is depends "%SDKCREDPROT%"
:: ...which in turn depends on "same" vs. "different domain" in Msi GUI.
::So, At this point we bail out for most vdir. Go for what we specify in web.config created
if not "%SubSiteName%"=="Web" GOTO Done

::But for vdir="Web" we continue
if "%SDKCREDPROT%"=="Ntlm" (
echo implementing Ntlm Auth
echo cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/Authflags 4
cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/Authflags 4
) else (
echo implementing Anonymous Auth
echo cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/Authflags 1
cscript "%ROOTDIR%\adsutil.vbs" set W3SVC/1/ROOT/%SitePath%/Authflags 1
)

GOTO Done

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