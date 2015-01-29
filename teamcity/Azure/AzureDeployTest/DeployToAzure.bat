@ECHO OFF

::Deploy latest Azure package to Azure (teleoptirnd)
SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%DeployToAzure.ps1
SET AzurePackagePath=%~1
IF "%AzurePackagePath%"=="" GOTO :NoAzurePackage

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%' '%ThisScriptsDirectory%' 'Teleopti CCC Azure' 'teleoptirnd' '%AzurePackagePath%' 'teleoptirnd.cscfg' 'production' 'AzureDemo.publishsettings'";
GOTO :EOF

:NoAzurePackage
ECHO Please give the azure package path
GOTO :EOF
