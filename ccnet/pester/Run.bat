@echo off
SETLOCAL
set rootdir=%~dp0
SET rootdir=%rootdir:~0,-1%
set obsoletePester=1.1.1;1.0.0

::install
"%rootdir%\..\..\.nuget\nuget.exe" install pester -o "%rootdir%" -Version %currentPester%

::pester internal tests fails, remove for now
cd "%rootdir%\Pester.%currentPester%"
del /F /S /Q *.Tests.ps1 > NUL

::remove obsolete version
for /f "tokens=1* delims=;" %%a in ("%obsoletePester%") do  if exist "%rootdir%\Pester.%%a" rmdir "%rootdir%\Pester.%%a" /S /Q

::Run all test
echo CMD /C ""%rootdir%\Pester.%currentPester%\tools\bin\pester.bat" "%~1""
CMD /C ""%rootdir%\Pester.%currentPester%\tools\bin\pester.bat" "%~1""

goto :eof

:userInput
CHOICE /C yn /M "run all Poweshell tests recursive from: %rootdir%?"
IF ERRORLEVEL 1 set /P rootdir=%rootdir%
IF ERRORLEVEL 2 set /P rootdir=Please provide another path: 
exit /b

endlocal