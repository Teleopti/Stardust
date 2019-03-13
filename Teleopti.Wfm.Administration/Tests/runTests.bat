@ECHO off

SET THIS=%~dp0
Set repoRoot=%THIS:~0,-35%


%repoRoot%\.nuget\nuget.exe install chutzpah -Version 4.4.4 -o %repoRoot%\packages

%repoRoot%\packages\Chutzpah.4.4.4\tools\chutzpah.console.exe /path %THIS%