@ECHO OFF
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Get input
SET SiteName=%1
SET SitePath=%2
SET DefaultSite="Default Web Site"
SET DefaultSiteOnly=Default Web Site


IF "%SiteName%"=="" CALL :GetInput

IF "%SiteName%"=="" (
ECHO no input give, I will quit
ping 127.0.0.1 -n 3 > NUL
EXIT
)

IF "%SitePath%"=="" (
SET SitePath=C:\inetpub\wwwroot\%SiteName%
)

::Set variables
SET AppName=%SiteName%
SET AppPoolName=%SiteName%

::Let us run unsigned scripts
ECHO powershell Set-ExecutionPolicy RemoteSigned
powershell Set-ExecutionPolicy RemoteSigned

::clean up previous build
ECHO powershell /file "%ROOTDIR%\CleanUp.ps1" %Sitepath% %DefaultSite% %SiteName% %AppPoolName%
powershell /file "%ROOTDIR%\CleanUp.ps1" %Sitepath% %DefaultSite% %SiteName% %AppPoolName%

::Create website + webapp + AppPool
ECHO powershell /file "%ROOTDIR%\CreateIISWebSiteAndApp.ps1" %Sitepath% %DefaultSite% %SiteName% %AppName% %AppPoolName%
powershell /file "%ROOTDIR%\CreateIISWebSiteAndApp.ps1" %Sitepath% %DefaultSite% %SiteName% %AppName% %AppPoolName%

::config VirDir
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%DefaultSiteOnly%/%SiteName%" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"False" /commit:apphost
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%DefaultSiteOnly%/%SiteName%" -section:system.webServer/security/authentication/basicAuthentication /enabled:"False" /commit:apphost
"%systemroot%\system32\inetsrv\AppCmd.exe" set config "%DefaultSiteOnly%/%SiteName%" -section:system.webServer/security/authentication/windowsAuthentication /enabled:"True" /commit:apphost

::Copy Content
::ECHO .cs > excluded.txt
XCOPY "%ROOTDIR%\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.Web" "%Sitepath%" /Y /R /S
::DEL /Q excluded.txt

::note: ON CCNet builds the Nhib file will be deployed by test projects via settings in intratest.ini

::Done
CLS
ECHO Deployed %SiteName% - Done!
ping 127.0.0.1 -n 3 > NUL
GOTO :EOF

:GetInput
SET /P SiteName=Please provdide a new siteName: 
GOTO :EOF