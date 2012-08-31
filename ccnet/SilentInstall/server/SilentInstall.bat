@echo off
SETLOCAL
COLOR A
cls
::init
SET /A ERRORLEV=0
SET msiCommandString=
SET CCCServer=52613B22-2102-4BFB-AAFB-EF420F3A24B5
SET tempInstall=%temp%\temp.bat

::get to local dir. NOTE: this will NOT work if script is started from a different drive letter!
CD "%~dp0"

::concat all parameters into one string
SETLOCAL EnableDelayedExpansion
SET S=
del "%tempInstall%" /Q
for /f "tokens=* delims= " %%a in (%~2.txt) do (
set S=!S!%%a 
)
set S=start /wait MSIExec /i "%~1" !S!
 > "%tempInstall%" echo.!S! /passive /l* "%~1.log"

::show msiexec string
more "%tempInstall%"
::notepad "%tempInstall%"

::start in own process and wait for return
"%tempInstall%"

::this part does not Work ... must get error handling to bubble up to this level
set /A msierror=%errorlevel%
ECHO MSIExec Errorlevel: %msierror%

IF %msierror% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

::done
GOTO :eof

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO The msi is already installed. You must uninstall the current version before applying a new one
IF %ERRORLEV% EQU 2 ECHO The msi installation failed with errorlevel: %msierror%
ECHO.
ECHO --------
GOTO :EOF


:EOF
ENDLOCAL