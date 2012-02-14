@echo off & setLocal enableDELAYedeXpansion
::---------------------
::Get content
::Uninstall customer config
::Install customer config files
::Copy to content foler
::cxpack
::---------------------

::init
SET customerFile=%1
SET customer=%customerFile:~0,-4%

::Uninstall customer config
MSIExec /uninstall "%msi%" /passive

::Install customer config files
for /f "tokens=* delims= " %%a in (Customer\%customerFile%) do (
set S=!S!%%a 
)
::set S=!S:~0,-1!
set S=MSIExec /i "%msi%" INSTALLDIR="%ConfigPath%" !S! /passive
> installConfig.bat echo.!S!
CALL installConfig.bat

::AzureConfig is added by WISE
SET ConfigPath=%ConfigPath%\AzureConfig
IF NOT EXIST "%ConfigPath%\DummyFolder" mkdir "%ConfigPath%\DummyFolder"

::Copy Config (3) to Content foler (2)
for /f "tokens=2,3 delims=," %%g in (contentMapping.txt) do xcopy /e /d /y "%ConfigPath%\%%h" "%ContentDest%\%%g\" 

::Sign ClickOnce
SET ClickOnceSignPath=%ContentDest%\TeleoptiCCC\Tools\ClickOnceSign
SET ClientPath=%ContentDest%\Client
SET MyTimePath=%ContentDest%\MyTime
CD "%ClickOnceSignPath%"
"%ClickOnceSignPath%\ClickOnceSign.exe" -s -a Teleopti.Ccc.SmartClientPortal.Shell.application -m "Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest" -u "http://teleopticcc-%customer%.cloudapp.net/Client/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%ClientPath%"
"%ClickOnceSignPath%\ClickOnceSign.exe" -s -a Teleopti.Ccc.AgentPortal.application -m "Teleopti.Ccc.AgentPortal.exe.manifest" -u "http://teleopticcc-%customer%.cloudapp.net/MyTime/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%MyTimePath%"
CD "%ROOTDIR%"

::cxpack
echo building cspack ...
if not exist "%output%" MKDIR "%output%"
"c:\Program Files\Windows Azure SDK\v1.4\bin\cspack.exe" "ServiceDefinition.csdef" /role:TeleoptiCCC;Temp\AzureContent\TeleoptiCCC /out:"%output%\%customer%.cspkg"
echo building cspack. done!
