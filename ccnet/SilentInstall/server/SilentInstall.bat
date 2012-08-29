@echo off
SETLOCAL EnableDelayedExpansion
CD "%~dp0"

::concat all parameters into one string
SET S=
SET tempInstall=%temp%\temp.bat
del "%tempInstall%" /Q

for /f "tokens=* delims= " %%a in (%2.txt) do (
set S=!S!%%a 
)

set S=MSIExec /i "%~1" !S! /passive
 > "%tempInstall%" echo.!S!

more "%tempInstall%"
call "%tempInstall%"