@echo off
COLOR A

::Get path to this batchfile
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
CD "%ROOTDIR%"

SET SrcArtifacts=\\A380\Hangaren\#PROGRAM\Develop\Cruise control\Artifacts
SET TargetArtifacts=C:\Temp\CCService

::Get Files
Echo copying setup files ...
ECHO ROBOCOPY "%SrcArtifacts%" "%TargetArtifacts%" /MIR
ROBOCOPY "%SrcArtifacts%" "%TargetArtifacts%" /MIR > NUL
Echo Done!
ECHO.

::IIS available?
:IISAvailable
cscript "%TargetArtifacts%\Tools\BrowseUrl.vbs" "http://localhost/"
if %errorlevel% neq 200 Call :IISInstall
cscript "%TargetArtifacts%\Tools\BrowseUrl.vbs" "http://localhost/"
if %errorlevel% neq 200 Call :IISAvailable

cls
CHOICE /C yn /M "Do you want to open your IIS and CCtray for remote access?"
IF ERRORLEVEL 1 SET /a remoteaccess=1
IF ERRORLEVEL 2 SET /a remoteaccess=0

::uninstall CruiseControl
CALL "%ROOTDIR%\UnInstallLocalCCServer.bat"

::install
start /wait "SomeName" "%TargetArtifacts%\CruiseControl.NET-1.8.4.0-Setup.exe" /S

::if this is a re-install, some extra steps are needed
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/ccnet"
"%systemroot%\system32\inetsrv\appcmd" add app /site.name:"Default Web Site" /path:"/ccnet" /physicalPath:"C:\Program Files (x86)\CruiseControl.NET\webdashboard"
"%systemroot%\System32\inetsrv\appcmd" add apppool /name:"ccnet" /managedRuntimeVersion:v2.0 /managedPipelineMode:Integrated /commit:apphost
"%systemroot%\System32\inetsrv\appcmd" set app "Default Web Site/ccnet" /applicationPool:"ccnet" /commit:apphost

::make ccnet use framework in x86 mode
"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\aspnet_regiis.exe" -s "W3SVC/1/ROOT/ccnet"

::stop sevice
NET STOP CCService

::CCService needs to run under your credetials in order to fetch your SyncFusion license registry values
cls
ECHO.
ECHO CCNet needs to run under your personal credentials, In order to access
ECHO the SyncFusion license sits in the CURRENT_USER registry hive.
ECHO.
ECHO Sorry, I can't hide your password, make sure you are alone.
SET /P password=Please provide the password for %USERDOMAIN%\%USERNAME%:

::Grant LogON as a Service
"%TargetArtifacts%\Tools\ntrights.exe" +r SeServiceLogonRight -u "%USERDOMAIN%\%USERNAME%"
::Set CCService to run with your credentials
sc config CCService obj= "%USERDOMAIN%\%USERNAME%" password= "%password%"

::fix local ccService config
COPY "%TargetArtifacts%\Tools\ccService.config" "C:\Program Files (x86)\CruiseControl.NET\server\ccservice.exe.config" /Y
cscript "%TargetArtifacts%\Tools\replace.vbs" $COMPUTERNAME$ %COMPUTERNAME% "C:\Program Files (x86)\CruiseControl.NET\server\ccservice.exe.config"

::deploy a working webdashboard
ROBOCOPY "%TargetArtifacts%\webdashboard" "c:\Program Files (x86)\CruiseControl.NET\webdashboard" /MIR > NUL

::open/close firewall
if %remoteaccess% equ 1 (
netsh advfirewall firewall add rule name = IIS_80 dir = in protocol = tcp action = allow localport = 80 remoteip = any profile = DOMAIN
netsh advfirewall firewall add rule name = CCTray dir = in protocol = tcp action = allow localport = 21234 remoteip = any profile = DOMAIN
)

if %remoteaccess% equ 0 (
netsh advfirewall firewall delete rule name = IIS_80
netsh advfirewall firewall delete rule name = CCTray
)

::get one project config
MKDIR "C:\Program Files (x86)\CruiseControl.NET\server\ccnetserver\WorkingDirectory"
COPY "%TargetArtifacts%\Tools\ccnet.config" "C:\Program Files (x86)\CruiseControl.NET\server\ccnetserver\WorkingDirectory\%COMPUTERNAME%_ccnet.config" /Y

::start
NET START CCService

::show web page
explorer "http://localhost/ccnet"

::edit and add your own projects
notepad "C:\Program Files (x86)\CruiseControl.NET\server\ccnetserver\WorkingDirectory\%COMPUTERNAME%_ccnet.config"

goto :eof

:IISInstall
::Get a local IIS for Windows 7 only
cls
ECHO I can't  browse your localhost, are you missing IIS?
CHOICE /C yn /M "Do you want to install IIS on your Windows 7 box?"
IF ERRORLEVEL 1 set /A addIIS
if addIIS equ 1 (
ECHO installing IIS ...
call "\\a380\hangaren\#PROGRAM\Develop\IIS7\install.bat"
ECHO Done IIS!
) else (
echo.
echo Cruise Control wont be working without IIS
ping 127.0.0.1 > NUL
)
exit /b