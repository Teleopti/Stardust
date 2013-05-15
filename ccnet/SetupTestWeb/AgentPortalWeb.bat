@ECHO OFF
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Get input
SET SiteName=%~1
SET CCC7DB=%~2
SET AnalyticsDB=%~3
SET AgentPortalWebURL=%~4

SET DefaultSite="Default Web Site"
SET DefaultSiteOnly=Default Web Site

IF "%SiteName%"=="" CALL :GetInput

IF "%SiteName%"=="" (
ECHO no input give, I will quit
ping 127.0.0.1 -n 3 > NUL
EXIT
)

SET SitePath=C:\inetpub\wwwroot\%SiteName%

::Set variables
SET AppName=%SiteName%
SET AppPoolName=%SiteName%

::Let us run unsigned scripts
ECHO powershell Set-ExecutionPolicy RemoteSigned
powershell Set-ExecutionPolicy RemoteSigned
ECHO powershell Set-Location "'%ROOTDIR%'"
powershell Set-Location "'%ROOTDIR%'"

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

::Copy template nhib and replace databases
COPY "%ROOTDIR%\WebTest.nhib.xml.template" "%Sitepath%\bin\WebTest.nhib.xml"
cscript "%ROOTDIR%\replace.vbs" $(CCC7DB) %CCC7DB% "%Sitepath%\bin\WebTest.nhib.xml"
cscript "%ROOTDIR%\replace.vbs" $(AnalyticsDB) %AnalyticsDB% "%Sitepath%\bin\WebTest.nhib.xml"
cscript "%ROOTDIR%\replace.vbs" $(AgentPortalWebURL) %AgentPortalWebURL% "%Sitepath%\web.config"

::Set esent permissions
"%Sitepath%\EsentPermissions.bat" "601" "1" "705" "IIS APPPOOL\%AppPoolName%"

::Do some finishing touches
ECHO powershell /file "%ROOTDIR%\AfterSetup.ps1" %Sitepath% %DefaultSite% %SiteName% %AppName% %AppPoolName%
powershell /file "%ROOTDIR%\AfterSetup.ps1" %Sitepath% %DefaultSite% %SiteName% %AppName% %AppPoolName%

::Done
CLS
ECHO Deployed %SiteName% - Done!
ping 127.0.0.1 -n 3 > NUL
GOTO :EOF

:GetInput
SET /P SiteName=Please provdide a new siteName: 
GOTO :EOF