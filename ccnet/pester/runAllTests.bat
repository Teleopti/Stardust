@echo off
SETLOCAL
set rootdir=%~dp0
SET rootdir=%rootdir:~0,-1%

::install
"%rootdir%\..\..\.nuget\nuget.exe" install pester -o "%rootdir%" -Version 1.1.1

::pester internal tests fails, remove for now
cd "%rootdir%\Pester.1.1.1"
del /F /S /Q *.Tests.ps1

::Run all test
"%rootdir%\Pester.1.1.1\tools\bin\pester.bat" "%rootdir%\..\.."
goto :eof

:userInput
CHOICE /C yn /M "run all Poweshell tests recursive from: %rootdir%?"
IF ERRORLEVEL 1 set /P rootdir=%rootdir%
IF ERRORLEVEL 2 set /P rootdir=Please provide another path: 
exit /b

endlocal