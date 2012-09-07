@echo off
SETLOCAL
COLOR A
cls

::init
SET /A ERRORLEV=0
SET /A msierror=0
SET msiCommandString=
SET CCCServer=52613B22-2102-4BFB-AAFB-EF420F3A24B5
SET tempInstall=%temp%\temp.bat

::change drive letter
%~d0
::change path
CD "%~dp0"

::check argument
set argC=0
for %%x in (%*) do Set /A argC+=1
if %argC% EQU 0 goto :userinput
if %argC% EQU 3 goto :commandlineInput
goto :help

:userinput
set /P msipath=Provide the full path to your msi file: 
::remove any double quotes
set msipath=%msipath:"=%

set /P config=Provide config name: 

CHOICE /C si /M "Would you like to show (S) the msiExec string or install the product (I)?"
IF ERRORLEVEL 1 SET action=show
IF ERRORLEVEL 2 SET action=install
goto :start

:commandlineInput
set msipath=%~1
set config=%~2
set action=%~3

goto :start
:Start
::concat all parameters into one string
SETLOCAL EnableDelayedExpansion
SET S=
del "%tempInstall%" /Q
for /f "tokens=* delims= " %%a in (config/%config%.txt) do (
set S=!S!%%a 
)
set S=start /wait MSIExec /i "%msipath%" !S!
 > "%tempInstall%" echo.!S! /qn /l* "install.log"

echo action is: %action%
if "%action%"=="show" notepad "%tempInstall%"
if "%action%"=="install" (
more "%tempInstall%"
"%tempInstall%"
set msierror=%errorloevel%
)

::this part does not Work ... must get error handling to bubble up to this level
IF %msierror% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

::done
GOTO :eof

:help
COLOR E
ECHO Run this batch file manully with no paramters, to enter input manually.
ECHO OR, Run this batch file from command line with paramters:
ECHO Msipath ^{local path^} ConfigName ^{Name of config file^} Action ^{show^|install^}
ECHO.
ECHO Example: SilentInstall.bat "C:\Temp\Teleopti CCC 7.2.0.0.msi" localhostDemoNoPM install
GOTO :EOF

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