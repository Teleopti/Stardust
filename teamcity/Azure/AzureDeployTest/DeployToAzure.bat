@ECHO OFF

::Deploy latest Azure package to Azure (teleoptirnd)
SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%DeployToAzure.ps1
::ECHO PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%' '%ThisScriptsDirectory%' 'Teleopti CCC Azure' 'teleoptirnd' 'Azure-8.1.419.33301.cspkg' 'teleoptirnd.cscfg' 'production' 'AzureDemo.publishsettings'";
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%' '%ThisScriptsDirectory%' 'Teleopti CCC Azure' 'teleoptirnd' 'Azure-8.1.419.33301.cspkg' 'teleoptirnd.cscfg' 'production' 'AzureDemo.publishsettings'";


