@echo off
SETLOCAL
COLOR A
cls

::init
set msipath=%~1
set config=%~2
set action=%3

SET /A ERRORLEV=0
SET msipath2=
SET msiCommandString=
SET CCCServer=52613B22-2102-4BFB-AAFB-EF420F3A24B5
SET tempInstall=%temp%\temp.bat

::get to local dir. NOTE: this will NOT work if script is started from a different drive letter!
CD "%~dp0"

if "%msipath%"=="" set /P msipath=Provide the full path to your msi file: 

::remove doube quotes
call:removeQuotes msipath2 %msipath%

if "%config%"=="" set /P config=Provide config name: 
IF "%action%"=="" CHOICE /C se /M "Would you like to reivew the msiExec string (S) or execute it (E)?"
IF ERRORLEVEL 1 SET action=show
IF ERRORLEVEL 2 SET action=install


::concat all parameters into one string
SETLOCAL EnableDelayedExpansion
SET S=
del "%tempInstall%" /Q
for /f "tokens=* delims= " %%a in (config/%config%.txt) do (
set S=!S!%%a 
)
set S=start /wait MSIExec /i "%msipath2%" !S!
 > "%tempInstall%" echo.!S! /qn /l* "install.log"

::show msiexec string
more "%tempInstall%"

if "%action%"=="show" notepad "%tempInstall%"
if "%action%"=="install" "%tempInstall%"

::this part does not Work ... must get error handling to bubble up to this level
set /A msierror=%errorlevel%
ECHO MSIExec Errorlevel: %msierror%

IF %msierror% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

::done
GOTO :eof
:removeQuotes
SETLOCAL
set string=%~2
(
ENDLOCAL
set "%~1=%string%"
)
goto:eof


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