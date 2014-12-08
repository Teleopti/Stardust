@echo off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

SET NugetFolder=%ROOTDIR%\..\..\.nuget
SET HttpSource=http://hestia/nuget
SET FileSource=\\hestia\nugetpackages

SET packagesName=Teleopti.Cube.Xmla
cls
ECHO Current Version is:
"%NugetFolder%\NuGet.exe" list %packagesName% -s %HttpSource%
ECHO.
SET /P NugetVersion=Please provide new NuGet version: 

"%NugetFolder%\NuGet.exe" pack "%ROOTDIR%\%packagesName%.nuspec" -Version "%NugetVersion%"
XCOPY "%ROOTDIR%\%packagesName%.%NugetVersion%.nupkg" "%FileSource%"

PAUSE
