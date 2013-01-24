@echo off
COLOR A
cls
::init
SET /A ERRORLEV=0

::uninstall MSI
::64-bit
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /v DisplayName
if %errorlevel% NEQ 1 (
MsiExec.exe /X{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /qn /L "uninstall-server.log"
)

::32-bit
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /v DisplayName
if %errorlevel% NEQ 1 (
MsiExec.exe /X{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /qn /L "uninstall-server.log"
)

::Drop IIS website and App-pools"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Analytics"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Client"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/ContextHelp"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/MyTime"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/MyTimeWeb"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Web"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/PMService"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/RTA"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/SDK"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Broker"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Broker.backplane"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v3.5"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0 Web"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0 Broker"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0 RTA"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0 SDK"

::remove regkeys
reg delete HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC /va /f
reg delete HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC /f

::done
GOTO :eof

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO The msi did not uninstall, might not exists. ERRORLEV is: %ERRORLEV%
ECHO.
ECHO --------
GOTO :EOF


:EOF