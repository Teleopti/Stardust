@ECHO OFF

SET VS2017_PATH=%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\
SET MSBUILD_PATH=\MSBuild\15.0\Bin\MSBuild.exe

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
   SET MSBUILD_PATH=\MSBuild\15.0\Bin\amd64\MSBuild.exe
)

SET MSBUILD=
IF EXIST "%VS2017_PATH%BuildTools%MSBUILD_PATH%" (
   SET MSBUILD="%VS2017_PATH%BuildTools%MSBUILD_PATH%"
) ELSE ( IF EXIST "%VS2017_PATH%Enterprise%MSBUILD_PATH%" (
   SET MSBUILD="%VS2017_PATH%Enterprise%MSBUILD_PATH%"
) ELSE ( IF EXIST "%VS2017_PATH%Professional%MSBUILD_PATH%" (
   SET MSBUILD="%VS2017_PATH%Professional%MSBUILD_PATH%"
)))

SET /A ERRORLEV=0
IF #%MSBUILD%#==## (
   COLOR C
   ECHO No valid msbuild.exe found, please install Microsoft Build Tools for Visual Studio 2017.
   ECHO Refer to https://www.visualstudio.com/downloads/#build-tools-for-visual-studio-2017
   SET /A ERRORLEV=999
)

SET MSBUILD_PATH=
SET VS2017_PATH=
