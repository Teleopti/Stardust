::Test msbuild targets on movefilesToDeploymentPipeline.msbuild
:: movefilesToDeploymentPipeline.bat [destfolder] [target] [configuration]

@ECHO off

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET MsbuildProjRaptor2=ccnet\Raptor2.proj
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

set destination=%~1
set target=%2
set configuration=%3

::handle empty input
if "%destination%"=="" call :help & goto:eof
if "%target%"=="" call :help & goto:eof
if "%configuration%"=="" call :help & goto:eof

rmdir %destination%

set DestinationFolder=%destination%
%msbuild% %ROOTDIR%\ccnet\movefilesToDeploymentPipeline.msbuild /t:%target% /p:Configuration=%configuration%

echo Done!
pause
goto:eof

:help
ECHO Example call:
ECHO movefilesToDeploymentPipeline.bat [destfolder] [target] [configuration]
exit /b