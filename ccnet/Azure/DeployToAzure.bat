@ECHO OFF
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DeployShare=D:\Installation\PreviousBuilds
SET SrcShare=\\hebe\Installation\msi
SET SrcCode=\\hebe\c$\Program Files (x86)\CruiseControl.NET\server\BuildMSI-main\WorkingDirectory
SET /p ActiveMajorVersion= < "%SrcCode%\ActiveMajorVersion.txt"
SET /p ActiveBranchVersion= < "%SrcCode%\ActiveBranchVersion.txt"

::make last figure a integer
SET /a ActiveBranchVersion=%ActiveBranchVersion%

::Get latest version figure
CALL "%ROOTDIR%\..\SilentInstall\InstallLatest\head.bat" 1 %SrcShare%\%ActiveMajorVersion%.%ActiveBranchVersion%.* > %temp%\Version.txt
SET /p version= < %temp%\Version.txt
ECHO Latest MSI version: %version%

::Build Azure Package for latest MSI build
ECHO %DeployShare%\%version%\Azure\BuildAzurePackages.bat"
CALL "%DeployShare%\%version%\Azure\BuildAzurePackages.bat"

C:
CD %ROOTDIR%

::Deplot latest Azure package to Azure (teleopti-dev)
SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%DeployToAzure.ps1
ECHO PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%' '%ThisScriptsDirectory%' 'Teleopti CCC Azure' 'teleopticcc-dev' 'd:\%version%\Azure-%version%.cspkg' 'd:\%version%\teleopticcc-dev.cscfg' 'staging' 'AzureDemo.publishsettings' '%version%'";
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%' '%ThisScriptsDirectory%' 'Teleopti CCC Azure' 'teleopticcc-dev' 'd:\%version%\Azure-%version%.cspkg' 'd:\%version%\teleopticcc-dev.cscfg' 'staging' 'AzureDemo.publishsettings' '%version%'";



