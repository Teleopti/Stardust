@ECHO OFF

::Deploy latest Azure package to Azure (teleoptirnd)
SET ThisScriptsDirectory=%~dp0
SET rootdir=%ThisScriptsDirectory:~0,-1%
SET CloudServiceName=%1
SET PowerShellScriptPath=%ThisScriptsDirectory%DeployToAzure.ps1
SET AzurePackagePath=%ThisScriptsDirectory%AzureRelease
IF "%AzurePackagePath%"=="" GOTO :NoAzurePackage

@ECHO ***************************************************************
@ECHO CloudServiceName			=	%CloudServiceName%
@ECHO AzurePackagePath			=	%AzurePackagePath%
@ECHO ThisScriptsDirectory		=	%ThisScriptsDirectory%
@ECHO VersionedName				=	%VersionedName%
@ECHO ***************************************************************

FOR /F %%G IN ('DIR "%AzurePackagePath%\*.cspkg" /B') DO SET VersionedName=%%G

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%' '%ThisScriptsDirectory%' 'Teleopti CCC Azure' '%CloudServiceName%' '%AzurePackagePath%\%VersionedName%' '%AzurePackagePath%\%CloudServiceName%.cscfg' 'production' '%ThisScriptsDirectory%AzureDemo.publishsettings'";
EXIT %ERRORLEVEL%

:NoAzurePackage
ECHO Please give the azure package path
GOTO :EOF
