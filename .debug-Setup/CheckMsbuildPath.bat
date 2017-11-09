@ECHO OFF

SET MSBUILD=

:: Find in visual studio build tools
SET VS2017_PATH=%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\
SET MSBUILD_PATH=\MSBuild\15.0\Bin\MSBuild.exe
IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
   SET MSBUILD_PATH=\MSBuild\15.0\Bin\amd64\MSBuild.exe
)
IF EXIST "%VS2017_PATH%BuildTools%MSBUILD_PATH%" (
   SET MSBUILD="%VS2017_PATH%BuildTools%MSBUILD_PATH%"
) ELSE ( IF EXIST "%VS2017_PATH%Enterprise%MSBUILD_PATH%" (
   SET MSBUILD="%VS2017_PATH%Enterprise%MSBUILD_PATH%"
) ELSE ( IF EXIST "%VS2017_PATH%Professional%MSBUILD_PATH%" (
   SET MSBUILD="%VS2017_PATH%Professional%MSBUILD_PATH%"
)))
SET MSBUILD_PATH=
SET VS2017_PATH=


:: Find in framework
SET DOTNET_PATH=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\
SET MSBUILD_PATH=MSBuild.exe
IF #%MSBUILD%#==## (
   IF EXIST "%DOTNET_PATH%%MSBUILD_PATH%" (
      SET MSBUILD="%DOTNET_PATH%%MSBUILD_PATH%"
   )
)
SET MSBUILD_PATH=
SET DOTNET_PATH=


SET /A ERRORLEV=0
IF #%MSBUILD%#==## (
   COLOR C
   ECHO No valid msbuild.exe found, please install Microsoft Build Tools for Visual Studio 2017.
   ECHO Refer to https://www.visualstudio.com/downloads/#build-tools-for-visual-studio-2017
   ECHO Continue and something will probably fail, some scripts doesnt seem to exit on error codes
   PAUSE
   SET /A ERRORLEV=999
)
