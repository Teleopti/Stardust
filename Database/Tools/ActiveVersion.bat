@echo off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DATABASEFOLDER=%ROOTDIR%\..

setlocal enabledelayedexpansion
set max=0
for %%x in (%DATABASEFOLDER%\teleopticcc7\releases\*.sql) do (
  set "FN=%%~nx"
  set "FN=!FN:version-=!"
  if !FN! GTR !max! set max=!FN!
)
::remove trailing zeros
for /f "tokens=* delims=0" %%N in ("%max%") do set "max=%%N"
(
endlocal
set /a ActiveBranchVersion=%max%
)