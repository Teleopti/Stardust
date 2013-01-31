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
PAUSE
endlocal