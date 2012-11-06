@echo off
COLOR A
::Get a local IIS for Windows 7 only
CHOICE /C yn /M "Do you want to install IIS on your Windows 7 box?"
IF ERRORLEVEL 1 SET iis=add
if %iis%=="add" call "\\a380\hangaren\#PROGRAM\Develop\IIS7\install.bat"

CHOICE /C yn /M "Do you want to open your IIS and CCtray for remote access?"
IF ERRORLEVEL 1 SET /a remoteaccess=1
IF ERRORLEVEL 2 SET /a remoteaccess=0

::uninstall CruiseControl
SC qc CCService | FIND "The specified service does not exist as an installed service" > NUL
IF %errorlevel% NEQ 0 (
"C:\Program Files (x86)\CruiseControl.NET\uninst.exe"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/ccnet"
echo.
echo ========================
echo can not control the uninstall process. Press any key when it's done!
echo ========================
echo.
COLOR E
pause
COLOR A
)

::install
start /wait "SomeName" "\\a380\hangaren\#PROGRAM\Develop\Cruise control\CruiseControl.NET-1.8.2.0-Setup.exe" /S

::if this is a re-install, some extra steps are needed
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/ccnet"
"%systemroot%\system32\inetsrv\appcmd" add app /site.name:"Default Web Site" /path:"/ccnet" /physicalPath:"C:\Program Files (x86)\CruiseControl.NET\webdashboard"
"%systemroot%\System32\inetsrv\appcmd" add apppool /name:"ccnet" /managedRuntimeVersion:v2.0 /managedPipelineMode:Integrated /commit:apphost
"%systemroot%\System32\inetsrv\appcmd" set app "Default Web Site/ccnet" /applicationPool:"ccnet" /commit:apphost

::make ccnet use framework in x86 mode
"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\aspnet_regiis.exe" -s "W3SVC/1/ROOT/ccnet"

::stop sevice
NET STOP CCService

::CCNet needs to run under your credetials to fetch your SyncFusion license registry values
ECHO.
ECHO CCNet needs to run under your personal credentials, SyncFusion license sits in CURRENT_USER registry hive.
ECHO.
ECHO And .... sorry, I can't hide your password, make sure you are alone.
SET /P password=Please provide the password for %USERDOMAIN%\%USERNAME%:

::Set CCService to run with your credentials
sc config CCService obj= "%USERDOMAIN%\%USERNAME%" password= "%password%"

::fix local ccService config
COPY "\\a380\hangaren\#PROGRAM\Develop\Cruise control\Artifacts\ccService.config" "C:\Program Files (x86)\CruiseControl.NET\server\ccservice.exe.config" /Y
cscript "\\a380\hangaren\#PROGRAM\Develop\Cruise control\Artifacts\replace.vbs" $COMPUTERNAME$ %COMPUTERNAME% "C:\Program Files (x86)\CruiseControl.NET\server\ccservice.exe.config"

::Get a working webdashboard kit
ROBOCOPY "\\a380\T-files\Develop\Test\Baselines\7zip" "C:\temp"
COPY "\\a380\hangaren\#PROGRAM\Develop\Cruise control\Artifacts\webdashboard.zip" "C:\temp\webdashboard.zip" /Y
C:\temp\7z.exe x -y -o"c:\Program Files (x86)\CruiseControl.NET\webdashboard" "C:\temp\webdashboard.zip"

::open/close firewall
if %remoteaccess% equ 1 (
netsh advfirewall firewall add rule name = IIS_80 dir = in protocol = tcp action = allow localport = 80 remoteip = any profile = DOMAIN
netsh advfirewall firewall add rule name = CCTray dir = in protocol = tcp action = allow localport = 21234 remoteip = any profile = DOMAIN
)

if %remoteaccess% equ 0 (
netsh advfirewall firewall delete rule name = IIS_80
netsh advfirewall firewall delete rule name = CCTray
)

::get TFS config
MKDIR "C:\Program Files (x86)\CruiseControl.NET\server\ccnetserver\WorkingDirectory"
COPY "\\a380\hangaren\#PROGRAM\Develop\Cruise control\Artifacts\ccnet.config" "C:\Program Files (x86)\CruiseControl.NET\server\ccnetserver\WorkingDirectory\%COMPUTERNAME%_ccnet.config" /Y

::Before first build => register NCover64 once
CD %~dp0
CD NCover64
NCover.Registration.exe //License NC3CMPLIC.lic

::start
NET START CCService

::show web page
explorer "http://localhost/ccnet"

::edit and add your own projects
notepad "C:\Program Files (x86)\CruiseControl.NET\server\ccnetserver\WorkingDirectory\%COMPUTERNAME%_ccnet.config"