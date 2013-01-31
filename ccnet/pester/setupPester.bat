SETLOCAL
set rootdir=%~dp0
SET rootdir=%rootdir:~0,-1%

"%rootdir%\..\..\.nuget\nuget.exe" install pester -o "%rootdir%" -Version 1.1.1
endlocal