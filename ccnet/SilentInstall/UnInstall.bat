::uninstall MSI
MsiExec.exe /X{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /QB

::Drop IIS website and App-pools"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Analytics"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Client"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/ContextHelp"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/MyTime"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/MyTimeWeb"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/Web"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/PMService"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/RTA"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC/SDK"
"%systemroot%\System32\inetsrv\appcmd" delete app "Default Web Site/TeleoptiCCC"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v3.5"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0 Web"
"%systemroot%\system32\inetsrv\appcmd" delete AppPool "Teleopti ASP.NET v4.0 Broker"

::remove regkeys
reg delete HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC /va /f
reg delete HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC /f
