@ECHO off

::get CCnet working folder
SET CCNetWorkDir=%~1
IF "%CCNetWorkDir%"=="" (
SET ERRORMSG=Input param missing. You need to provide the current CCnet working directory
GOTO :Failed
)

SET smartassembly=C:\Program Files\Red Gate\SmartAssembly 6

::command line tool
SET smartassemblyCmd=%smartassembly%\SmartAssembly.com

IF NOT EXIST "%smartassemblyCmd%" (
SET ERRORMSG I can't find .exe for: "%smartassemblyCmd%"
GOTO :Failed
)
ECHO I have found: "%smartassemblyCmd%"
PAUSE

::Set variables
SET SMARTASSEMBLYPROJ=%CCNetWorkDir%\ccnet\Obfuscate
SET WORKDIR=%CCNetWorkDir%\ccnet\Obfuscate\WorkingFolder
SET OUTPUTFOLDER=%WORKDIR%\AssemblyOutput

::remove previous build
IF EXIST "%WORKDIR%" RMDIR "%WORKDIR%" /S /Q
MKDIR "%OUTPUTFOLDER%"

"%smartassemblyCmd%" /build "%SMARTASSEMBLYPROJ%\Obfuscated.{sa}proj"
IF %ERRORLEVEL% NEQ 0 (
SET ERRORLEV=1
SET ERRORMSG=Failed to obfuscate: Obfuscated.{sa}proj
GOTO :Failed
)
ECHO ----------
ECHO.

"%smartassemblyCmd%" /build "%SMARTASSEMBLYPROJ%\Infrastructure.{sa}proj"
IF %ERRORLEVEL% NEQ 0 (
SET ERRORLEV=3
SET ERRORMSG=Failed to obfuscate: Infrastructure.{sa}proj
GOTO :Failed
)
ECHO ----------
ECHO.

::Copy the obfuscated dll+pdbs to all locations where it exists
FOR /R "%CCNetWorkDir%" %%I IN (*Teleopti.Ccc.Obfuscated.dll*) DO COPY "%OUTPUTFOLDER%\Teleopti.Ccc.Obfuscated.dll" "%%I" /Y /V
FOR /R "%CCNetWorkDir%" %%I IN (*Teleopti.Ccc.Obfuscated.pdb*) DO COPY "%OUTPUTFOLDER%\Teleopti.Ccc.Obfuscated.pdb" "%%I" /Y /V

FOR /R "%CCNetWorkDir%"  %%I IN (*Teleopti.Ccc.Infrastructure.dll*) DO COPY "%OUTPUTFOLDER%\Teleopti.Ccc.Infrastructure.dll" "%%I" /Y /V
FOR /R "%CCNetWorkDir%"  %%I IN (*Teleopti.Ccc.Infrastructure.pdb*) DO COPY "%OUTPUTFOLDER%\Teleopti.Ccc.Infrastructure.pdb" "%%I" /Y /V

GOTO :eof

:Failed
ECHO Obfuscating failed. Check CCNET buildlog
ECHO %ERRORMSG%
PING 127.0.0.1 -n 5 >NUL
SET ERRORLEVEL=%ERRORMSG%
GOTO :eof

:eof
EXIT %ERRORLEVEL%
