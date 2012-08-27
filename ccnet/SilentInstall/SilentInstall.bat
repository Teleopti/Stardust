@echo off & setLocal enableDELAYedeXpansion
SET S=
SET tempInstall=%temp%\temp.bat
del "%tempInstall%" /Q

for /f "tokens=* delims= " %%a in (SilentInstall.txt) do (
set S=!S!%%a 
)

::set S=MSIExec /i "%msi%" !S! /passive
> "%tempInstall%" echo.!S!

::notepad "%tempInstall%"

call "%tempInstall%"
