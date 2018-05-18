@echo off
setlocal enabledelayedexpansion
if "%1" == "" goto usage
if "%2" == "" goto usage

dir "%~2" /B /AD /O-D > %temp%\dir.txt
set file=%temp%\dir.txt

SET /a counter=0

for /f "usebackq delims=" %%a in (%file%) do (
if "!counter!"=="%1" goto exit
echo %%a
set /a counter+=1
)

goto exit

:usage
echo Usage: head.bat COUNT FILENAME

:exit
