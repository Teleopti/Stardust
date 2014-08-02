start /w pkgmgr /iu:IIS-WebServerRole;WAS-WindowsActivationService;WAS-ProcessModel;WAS-NetFxEnvironment;WAS-ConfigurationAPI

IF %PROCESSOR_ARCHITECTURE%=="AMD64" (
"C:\Windows\Microsoft.NET\Framework64\v3.0\Windows Communication Foundation\ServiceModelReg.exe" -i
) ELSE (
"C:\Windows\Microsoft.NET\Framework\v3.0\Windows Communication Foundation\ServiceModelReg.exe" -i
)

::Add IIS features needed by Teleopti CCC 7
start /w pkgmgr /iu:IIS-CommonHttpFeatures;IIS-StaticContent;IIS-DefaultDocument;IIS-HttpErrors;IIS-HttpRedirect;IIS-ApplicationDevelopment;IIS-ASPNET;IIS-NetFxExtensibility;IIS-ISAPIExtensions;IIS-ISAPIFilter;IIS-HealthAndDiagnostics;IIS-HttpLogging;IIS-LoggingLibraries;IIS-RequestMonitor;IIS-Security;IIS-BasicAuthentication;IIS-WindowsAuthentication;IIS-IIS6ManagementCompatibility;IIS-Metabase;IIS-ManagementScriptingTools;IIS-WMICompatibility;IIS-LegacyScripts

::Enable Windows Auth on Default Web site
C:
CD "%windir%\system32\inetsrv"
appcmd set config /section:windowsAuthentication /enabled:true

::register asp.net in IIS
IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\aspnet_regiis.exe" -i
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe" -iru
) ELSE (
"C:\Windows\Microsoft.NET\Framework\v2.0.50727\aspnet_regiis.exe" -i
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe" -iru
)

::make DefaultAppPool 2.0
"%systemroot%\system32\inetsrv\APPCMD.exe" set apppool "DefaultAppPool" /managedRuntimeVersion:v2.0 /commit:apphost